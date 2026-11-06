using BAIDOXETHONGMINH.DAL.Database;
using BAIDOXETHONGMINH.DTO.Entities;
using BAIDOXETHONGMINH.Models;
using Microsoft.Data.Sqlite;

namespace BAIDOXETHONGMINH.GUI;

public class MonthlyPassRepository
{
    private const string SelectPass =
        "SELECT PassId, CustomerId, LicensePlate, VehicleTypeId, VehicleBrandColor, StartDate, EndDate, Status FROM MonthlyPasses";

    private static MonthlyPass Map(SqliteDataReader r) => new()
    {
        PassId = r.GetInt32(0),
        CustomerId = r.GetInt32(1),
        LicensePlate = r.GetString(2),
        VehicleTypeId = r.GetString(3),
        VehicleBrandColor = SqliteHelper.GetNullableString(r, 4),
        StartDate = r.GetDateTime(5),
        EndDate = r.GetDateTime(6),
        Status = r.GetString(7)
    };

    /// <summary>Vé tháng kèm tên và SĐT chủ xe .</summary>
    public Task<List<MonthlyPassView>> GetViewsAsync() =>
        SqliteHelper.QueryAsync(
            @"SELECT p.PassId, p.CustomerId, c.FullName, c.Phone, p.LicensePlate, p.VehicleTypeId,
                     p.VehicleBrandColor, p.StartDate, p.EndDate, p.Status
              FROM MonthlyPasses p
              JOIN Customers c ON c.CustomerId = p.CustomerId
              ORDER BY p.PassId DESC",
            r => new MonthlyPassView
            {
                PassId = r.GetInt32(0),
                CustomerId = r.GetInt32(1),
                OwnerName = r.GetString(2),
                Phone = r.GetString(3),
                LicensePlate = r.GetString(4),
                VehicleTypeId = r.GetString(5),
                Brand = SqliteHelper.GetNullableString(r, 6),
                StartDate = r.GetDateTime(7),
                EndDate = r.GetDateTime(8),
                Status = r.GetString(9)
            });

    public Task<MonthlyPass?> GetByIdAsync(int passId) =>
        SqliteHelper.QueryFirstAsync(SelectPass + " WHERE PassId = @id", Map, ("@id", passId));

    public Task<List<MonthlyPass>> GetActiveAsync() =>
        SqliteHelper.QueryAsync(SelectPass + " WHERE Status = @status", Map, ("@status", PassStatus.Active));

    public async Task<bool> HasAnyPassAsync(int customerId) =>
        await SqliteHelper.ScalarAsync("SELECT COUNT(*) FROM MonthlyPasses WHERE CustomerId = @id", ("@id", customerId)) > 0;

    /// <summary>
    /// Thêm vé tháng (và giao dịch thu tiền nếu có) trong một Transaction.
     public Task AddAsync(MonthlyPass pass, PaymentTransaction? payment = null) =>
        RunSavingAsync(() => AddCoreAsync(pass, payment));

    private async Task AddCoreAsync(MonthlyPass pass, PaymentTransaction? payment)
    {
        await SqliteHelper.InTransactionAsync(async (connection, tx) =>
        {
            long id = await SqliteHelper.InsertAsync(connection, tx,
                @"INSERT INTO MonthlyPasses (CustomerId, LicensePlate, VehicleTypeId, VehicleBrandColor, StartDate, EndDate, Status)
                  VALUES (@customer, @plate, @type, @brand, @start, @end, @status)",
                ("@customer", pass.CustomerId), ("@plate", pass.LicensePlate), ("@type", pass.VehicleTypeId),
                ("@brand", pass.VehicleBrandColor), ("@start", pass.StartDate), ("@end", pass.EndDate),
                ("@status", pass.Status));
            pass.PassId = (int)id;

            if (payment != null)
            {
                payment.ReferenceId = pass.PassId;
                await InsertPaymentAsync(connection, tx, payment);
            }
        });
    }

    /// <summary>Cập nhật vé tháng (và ghi giao dịch thu tiền nếu có) trong một Transaction.</summary>
    public Task UpdateAsync(MonthlyPass pass, PaymentTransaction? payment = null) =>
        RunSavingAsync(() => UpdateCoreAsync(pass, payment));

    private async Task UpdateCoreAsync(MonthlyPass pass, PaymentTransaction? payment)
    {
        await SqliteHelper.InTransactionAsync(async (connection, tx) =>
        {
            await SqliteHelper.ExecuteAsync(connection, tx,
                @"UPDATE MonthlyPasses
                  SET VehicleBrandColor = @brand, StartDate = @start, EndDate = @end, Status = @status
                  WHERE PassId = @id",
                ("@brand", pass.VehicleBrandColor), ("@start", pass.StartDate), ("@end", pass.EndDate),
                ("@status", pass.Status), ("@id", pass.PassId));

            if (payment != null)
            {
                payment.ReferenceId = pass.PassId;
                await InsertPaymentAsync(connection, tx, payment);
            }
        });
    }

    private static async Task RunSavingAsync(Func<Task> work)
    {
        try
        {
            await work();
        }
        catch (SqliteException ex) when (SqliteHelper.IsConstraintError(ex))
        {
            throw new ServiceException("Không lưu được vé tháng hoặc giao dịch thu tiền: khách hàng, loại xe hoặc người thu không hợp lệ.");
        }
    }

    private static Task<long> InsertPaymentAsync(SqliteConnection connection, SqliteTransaction tx, PaymentTransaction p) =>
        SqliteHelper.InsertAsync(connection, tx,
            @"INSERT INTO PaymentTransactions (TransactionType, ReferenceId, Amount, PaymentMethod, CollectedByUserId, ShiftId, Note, CreatedAt)
              VALUES (@type, @ref, @amount, @method, @user, @shift, @note, @created)",
            ("@type", p.TransactionType), ("@ref", p.ReferenceId), ("@amount", p.Amount),
            ("@method", p.PaymentMethod), ("@user", p.CollectedByUserId), ("@shift", p.ShiftId),
            ("@note", p.Note), ("@created", p.CreatedAt));

    /// <summary>Đổi hàng loạt vé quá hạn sang Expired bằng một lệnh UPDATE.</summary>
    public Task ExpireOverdueAsync(DateTime today) =>
        SqliteHelper.ExecuteAsync(
            "UPDATE MonthlyPasses SET Status = @expired WHERE Status = @active AND EndDate < @today",
            ("@expired", PassStatus.Expired), ("@active", PassStatus.Active), ("@today", today));
}

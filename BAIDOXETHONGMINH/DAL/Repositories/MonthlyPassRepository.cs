using System.Globalization;
using BAIDOXETHONGMINH.DAL.Database;
using BAIDOXETHONGMINH.DTO.Entities;
using BAIDOXETHONGMINH.Models;
using Microsoft.Data.Sqlite;

namespace BAIDOXETHONGMINH.GUI;

public class MonthlyPassRepository
{
    private const string SelectPass =
        "SELECT PassId, CustomerId, LicensePlate, VehicleTypeId, VehicleBrandColor, StartDate, EndDate, Status FROM MonthlyPasses";

    // Hàm chuyển đổi an toàn chuỗi ngày từ SQLite sang DateTime
    private static DateTime ParseDateSafe(SqliteDataReader reader, int index)
    {
        if (reader.IsDBNull(index))
        {
            return DateTime.Today;
        }

        string raw = reader.GetString(index);
        string[] formats = new string[] { "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy", "dd/MM/yyyy HH:mm:ss" };
        if (DateTime.TryParseExact(raw, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime exactDate))
        {
            return exactDate;
        }

        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
        {
            return parsedDate;
        }

        return DateTime.Today;
    }

    // Hàm ánh xạ một dòng dữ liệu SqliteDataReader sang đối tượng MonthlyPass
    private static MonthlyPass Map(SqliteDataReader r)
    {
        return new MonthlyPass
        {
            PassId = r.GetInt32(0),
            CustomerId = r.GetInt32(1),
            LicensePlate = r.GetString(2),
            VehicleTypeId = r.GetString(3),
            VehicleBrandColor = SqliteHelper.GetNullableString(r, 4),
            StartDate = ParseDateSafe(r, 5),
            EndDate = ParseDateSafe(r, 6),
            Status = r.GetString(7)
        };
    }

    // Hàm lấy danh sách hiển thị các vé tháng kèm thông tin chủ xe
    public Task<List<MonthlyPassView>> GetViewsAsync()
    {
        return SqliteHelper.QueryAsync(
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
                StartDate = ParseDateSafe(r, 7),
                EndDate = ParseDateSafe(r, 8),
                Status = r.GetString(9)
            });
    }

    // Hàm tìm kiếm vé tháng theo mã vé
    public Task<MonthlyPass?> GetByIdAsync(int passId)
    {
        return SqliteHelper.QueryFirstAsync(SelectPass + " WHERE PassId = @id", Map, ("@id", passId));
    }

    // Hàm lấy danh sách các vé tháng đang có trạng thái Active
    public Task<List<MonthlyPass>> GetActiveAsync()
    {
        return SqliteHelper.QueryAsync(SelectPass + " WHERE Status = @status", Map, ("@status", PassStatus.Active));
    }

    // Hàm kiểm tra khách hàng đã từng có vé tháng nào chưa
    public async Task<bool> HasAnyPassAsync(int customerId)
    {
        return await SqliteHelper.ScalarAsync("SELECT COUNT(*) FROM MonthlyPasses WHERE CustomerId = @id", ("@id", customerId)) > 0;
    }

    // Hàm đăng ký vé tháng mới kèm tạo/cập nhật khách hàng và giao dịch thu tiền trong cùng một Transaction
    public Task RegisterWithCustomerAsync(Customer customer, MonthlyPass pass, PaymentTransaction payment)
    {
        return RunSavingAsync(() => RegisterWithCustomerCoreAsync(customer, pass, payment));
    }

    // Hàm thực thi Transaction gộp lưu Khách hàng, Vé tháng và Hóa đơn thu tiền đồng bộ
    private async Task RegisterWithCustomerCoreAsync(Customer customer, MonthlyPass pass, PaymentTransaction payment)
    {
        await SqliteHelper.InTransactionAsync(async (connection, tx) =>
        {
            using SqliteCommand findCustomerCmd = connection.CreateCommand();
            findCustomerCmd.Transaction = tx;
            findCustomerCmd.CommandText = "SELECT CustomerId FROM Customers WHERE IdentityCard = @cccd LIMIT 1;";
            findCustomerCmd.Parameters.AddWithValue("@cccd", customer.IdentityCard);
            object? existingCustomerIdObj = await findCustomerCmd.ExecuteScalarAsync();

            if (existingCustomerIdObj != null && existingCustomerIdObj != DBNull.Value)
            {
                customer.CustomerId = Convert.ToInt32(existingCustomerIdObj);
                await SqliteHelper.ExecuteAsync(
                    connection,
                    tx,
                    "UPDATE Customers SET FullName = @name, Phone = @phone WHERE CustomerId = @id",
                    ("@name", customer.FullName),
                    ("@phone", customer.Phone),
                    ("@id", customer.CustomerId)
                );
            }
            else
            {
                long newCustomerId = await SqliteHelper.InsertAsync(
                    connection,
                    tx,
                    "INSERT INTO Customers (FullName, Phone, IdentityCard, CreatedAt) VALUES (@name, @phone, @cccd, @created)",
                    ("@name", customer.FullName),
                    ("@phone", customer.Phone),
                    ("@cccd", customer.IdentityCard),
                    ("@created", customer.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
                );
                customer.CustomerId = (int)newCustomerId;
            }

            pass.CustomerId = customer.CustomerId;
            long passId = await SqliteHelper.InsertAsync(
                connection,
                tx,
                @"INSERT INTO MonthlyPasses (CustomerId, LicensePlate, VehicleTypeId, VehicleBrandColor, StartDate, EndDate, Status)
                  VALUES (@customer, @plate, @type, @brand, @start, @end, @status)",
                ("@customer", pass.CustomerId),
                ("@plate", pass.LicensePlate),
                ("@type", pass.VehicleTypeId),
                ("@brand", pass.VehicleBrandColor),
                ("@start", pass.StartDate.ToString("yyyy-MM-dd")),
                ("@end", pass.EndDate.ToString("yyyy-MM-dd")),
                ("@status", pass.Status)
            );
            pass.PassId = (int)passId;

            payment.ReferenceId = pass.PassId;
            await InsertPaymentAsync(connection, tx, payment);
        });
    }

    // Hàm thêm mới vé tháng (và giao dịch thanh toán nếu có) trong một Transaction
    public Task AddAsync(MonthlyPass pass, PaymentTransaction? payment = null)
    {
        return RunSavingAsync(() => AddCoreAsync(pass, payment));
    }

    // Hàm thực thi thêm mới vé tháng vào CSDL
    private async Task AddCoreAsync(MonthlyPass pass, PaymentTransaction? payment)
    {
        await SqliteHelper.InTransactionAsync(async (connection, tx) =>
        {
            long id = await SqliteHelper.InsertAsync(
                connection,
                tx,
                @"INSERT INTO MonthlyPasses (CustomerId, LicensePlate, VehicleTypeId, VehicleBrandColor, StartDate, EndDate, Status)
                  VALUES (@customer, @plate, @type, @brand, @start, @end, @status)",
                ("@customer", pass.CustomerId),
                ("@plate", pass.LicensePlate),
                ("@type", pass.VehicleTypeId),
                ("@brand", pass.VehicleBrandColor),
                ("@start", pass.StartDate.ToString("yyyy-MM-dd")),
                ("@end", pass.EndDate.ToString("yyyy-MM-dd")),
                ("@status", pass.Status)
            );
            pass.PassId = (int)id;

            if (payment != null)
            {
                payment.ReferenceId = pass.PassId;
                await InsertPaymentAsync(connection, tx, payment);
            }
        });
    }

    // Hàm cập nhật vé tháng (gia hạn, sửa mô tả hoặc hủy vé) kèm giao dịch thu tiền nếu có
    public Task UpdateAsync(MonthlyPass pass, PaymentTransaction? payment = null)
    {
        return RunSavingAsync(() => UpdateCoreAsync(pass, payment));
    }

    // Hàm thực thi cập nhật thông tin vé tháng và ghi giao dịch thanh toán trong cùng Transaction
    private async Task UpdateCoreAsync(MonthlyPass pass, PaymentTransaction? payment)
    {
        await SqliteHelper.InTransactionAsync(async (connection, tx) =>
        {
            await SqliteHelper.ExecuteAsync(
                connection,
                tx,
                @"UPDATE MonthlyPasses
                  SET VehicleBrandColor = @brand, StartDate = @start, EndDate = @end, Status = @status
                  WHERE PassId = @id",
                ("@brand", pass.VehicleBrandColor),
                ("@start", pass.StartDate.ToString("yyyy-MM-dd")),
                ("@end", pass.EndDate.ToString("yyyy-MM-dd")),
                ("@status", pass.Status),
                ("@id", pass.PassId)
            );

            if (payment != null)
            {
                payment.ReferenceId = pass.PassId;
                await InsertPaymentAsync(connection, tx, payment);
            }
        });
    }

    // Hàm bọc xử lý lỗi ràng buộc khóa ngoại khi lưu dữ liệu vé tháng
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

    // Hàm lưu giao dịch thu tiền vé tháng vào PaymentTransactions, tự gắn ShiftId ca đang mở và cộng doanh thu ca
    private static async Task<long> InsertPaymentAsync(SqliteConnection connection, SqliteTransaction tx, PaymentTransaction p)
    {
        int? shiftId = p.ShiftId;
        if (!shiftId.HasValue)
        {
            using SqliteCommand shiftCmd = connection.CreateCommand();
            shiftCmd.Transaction = tx;
            shiftCmd.CommandText = "SELECT ShiftId FROM WorkShifts WHERE Status = 'Open' ORDER BY ShiftId DESC LIMIT 1;";
            object? shiftResult = await shiftCmd.ExecuteScalarAsync();
            if (shiftResult != null && shiftResult != DBNull.Value)
            {
                shiftId = Convert.ToInt32(shiftResult);
            }
        }

        p.ShiftId = shiftId;

        long transactionId = await SqliteHelper.InsertAsync(
            connection,
            tx,
            @"INSERT INTO PaymentTransactions (TransactionType, ReferenceId, Amount, PaymentMethod, CollectedByUserId, ShiftId, Note, CreatedAt)
              VALUES (@type, @ref, @amount, @method, @user, @shift, @note, @created)",
            ("@type", p.TransactionType),
            ("@ref", p.ReferenceId),
            ("@amount", p.Amount),
            ("@method", p.PaymentMethod),
            ("@user", p.CollectedByUserId),
            ("@shift", shiftId),
            ("@note", p.Note),
            ("@created", p.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
        );

        if (shiftId.HasValue && p.Amount > 0m)
        {
            await SqliteHelper.ExecuteAsync(
                connection,
                tx,
                "UPDATE WorkShifts SET TotalRevenue = TotalRevenue + @amount WHERE ShiftId = @shift;",
                ("@amount", p.Amount),
                ("@shift", shiftId.Value)
            );
        }

        return transactionId;
    }

    // Hàm cập nhật trạng thái hết hạn cho các vé có ngày hết hạn nhỏ hơn ngày hiện tại
    public Task ExpireOverdueAsync(DateTime today)
    {
        return SqliteHelper.ExecuteAsync(
            "UPDATE MonthlyPasses SET Status = @expired WHERE Status = @active AND date(EndDate) < date(@today)",
            ("@expired", PassStatus.Expired),
            ("@active", PassStatus.Active),
            ("@today", today.ToString("yyyy-MM-dd"))
        );
    }
}

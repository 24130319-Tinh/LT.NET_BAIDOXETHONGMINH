using BAIDOXETHONGMINH.DAL.Database;
using BAIDOXETHONGMINH.DTO.Entities;
using Microsoft.Data.Sqlite;

namespace BAIDOXETHONGMINH.GUI;

public sealed record ParkingVisit(int SessionId, string CardCode, string LicensePlate,
    string VehicleTypeId, string TypeName, string SlotCode, DateTime CheckInTime,
    bool IsMonthlyPass, string? ImagePath)
{
    public override string ToString() => $"{CardCode} — {LicensePlate} ({TypeName} • {SlotCode})";
}

public sealed record CheckoutQuote(ParkingVisit Visit, DateTime ExitTime, decimal ParkingFee, decimal Penalty)
{
    public decimal Total => ParkingFee + Penalty;
}

public sealed record LostCardReport(string FullName, string Phone, string IdentityCard);

public class ParkingService
{
    private const string SelectVisits = """
        SELECT s.SessionId, s.CardCode, s.LicensePlate, s.VehicleTypeId, v.TypeName,
               s.SlotCode, s.CheckInTime, s.IsMonthlyPass, s.CheckInImagePath
        FROM ParkingSessions s JOIN VehicleTypes v ON v.VehicleTypeId = s.VehicleTypeId
        WHERE s.Status = 'Parking' AND s.CheckOutTime IS NULL
        """;

    private static ParkingVisit Map(SqliteDataReader r) => new(r.GetInt32(0), r.GetString(1),
        r.GetString(2), r.GetString(3), r.GetString(4), r.GetString(5), r.GetDateTime(6),
        r.GetBoolean(7), SqliteHelper.GetNullableString(r, 8));

    public Task<List<ParkingVisit>> GetActiveVisitsAsync() =>
        SqliteHelper.QueryAsync(SelectVisits + " ORDER BY s.CheckInTime", Map);

    // Ô tô tính giờ, làm tròn lên; loại xe khác tính lượt ngày/qua đêm.
    // Mỗi khoảng 24 giờ là một lượt; phần lẻ có đi qua 22:00–06:00 tính giá đêm.
    public static decimal CalculateFee(DateTime entry, DateTime exit, bool monthly, PricingRule rule)
    {
        if (exit < entry) throw new ServiceException("Giờ ra không được trước giờ vào.");
        if (monthly) return 0;
        if (rule.VehicleTypeId == "OTO")
        {
            if (rule.HourlyPrice <= 0) throw new ServiceException("Chưa cấu hình giá giờ cho ô tô.");
            return Math.Max(1m, (decimal)Math.Ceiling((exit - entry).TotalHours)) * rule.HourlyPrice;
        }
        if (rule.DayTurnPrice <= 0 || rule.OvernightPrice <= 0)
            throw new ServiceException("Chưa cấu hình đủ giá ngày/đêm cho loại xe.");
        decimal fee = 0;
        do
        {
            var end = entry.AddDays(1) < exit ? entry.AddDays(1) : exit;
            bool night = entry.TimeOfDay < TimeSpan.FromHours(6) ||
                         entry.TimeOfDay >= TimeSpan.FromHours(22) || end > entry.Date.AddHours(22);
            fee += night ? rule.OvernightPrice : rule.DayTurnPrice;
            entry = end;
        } while (entry < exit);
        return fee;
    }

    public async Task<CheckoutQuote> GetQuoteAsync(int sessionId, bool lostCard)
    {
        var visit = await SqliteHelper.QueryFirstAsync(SelectVisits + " AND s.SessionId = @id", Map, ("@id", sessionId))
            ?? throw new ServiceException("Lượt gửi xe đã kết thúc hoặc không tồn tại.");
        var rule = await new ParkingRepository().GetPricingRuleAsync(visit.VehicleTypeId)
            ?? throw new ServiceException("Chưa có bảng giá cho loại xe này.");
        var now = DateTime.Now;
        return new(visit, now, CalculateFee(visit.CheckInTime, now, visit.IsMonthlyPass, rule),
            lostCard ? rule.LostCardPenalty : 0);
    }

    public async Task CheckoutAsync(CheckoutQuote quote, bool lostCard, LostCardReport? report)
    {
        int user = AppService.CurrentUserId ?? throw new ServiceException("Chưa xác định người thu tiền.");
        if (lostCard && (report == null || string.IsNullOrWhiteSpace(report.FullName) ||
            string.IsNullOrWhiteSpace(report.Phone) || string.IsNullOrWhiteSpace(report.IdentityCard)))
            throw new ServiceException("Hãy nhập đủ họ tên, điện thoại và CCCD cho biên bản mất thẻ.");
        await SqliteHelper.InTransactionAsync(async (connection, tx) =>
        {
            using var read = connection.CreateCommand();
            read.Transaction = tx;
            read.CommandText = SelectVisits + " AND s.SessionId = @id";
            read.Parameters.AddWithValue("@id", quote.Visit.SessionId);
            ParkingVisit current;
            using (var reader = await read.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync()) throw new ServiceException("Lượt gửi xe đã được checkout ở nơi khác.");
                current = Map(reader);
            }
            if (current != quote.Visit) throw new ServiceException("Dữ liệu lượt xe đã thay đổi. Hãy chọn lại xe.");
            using var pricing = connection.CreateCommand();
            pricing.Transaction = tx;
            pricing.CommandText = "SELECT DayTurnPrice, OvernightPrice, HourlyPrice, LostCardPenalty FROM PricingRules WHERE VehicleTypeId = @type";
            pricing.Parameters.AddWithValue("@type", current.VehicleTypeId);
            PricingRule rule;
            using (var reader = await pricing.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync()) throw new ServiceException("Không tìm thấy bảng giá.");
                rule = new() { VehicleTypeId = current.VehicleTypeId, DayTurnPrice = reader.GetDecimal(0),
                    OvernightPrice = reader.GetDecimal(1), HourlyPrice = reader.GetDecimal(2), LostCardPenalty = reader.GetDecimal(3) };
            }
            if (quote.ExitTime > DateTime.Now || DateTime.Now - quote.ExitTime > TimeSpan.FromMinutes(5) ||
                quote.ParkingFee != CalculateFee(current.CheckInTime, quote.ExitTime, current.IsMonthlyPass, rule) ||
                quote.Penalty != (lostCard ? rule.LostCardPenalty : 0))
                throw new ServiceException("Báo giá đã thay đổi hoặc hết hạn. Hãy xác nhận lại.");
            var count = await SqliteHelper.ExecuteAsync(connection, tx, """
                UPDATE ParkingSessions SET CheckOutTime=@time, CheckOutUserId=@user, ParkingFee=@fee,
                    PenaltyFee=@penalty, TotalAmount=@total, Status='Completed'
                WHERE SessionId=@id AND Status='Parking' AND CheckOutTime IS NULL
                """, ("@time", quote.ExitTime), ("@user", user), ("@fee", quote.ParkingFee),
                ("@penalty", quote.Penalty), ("@total", quote.Total), ("@id", current.SessionId));
            if (count != 1) throw new ServiceException("Lượt xe đã được xử lý.");
            if (await SqliteHelper.ExecuteAsync(connection, tx, """
                UPDATE ParkingSlots SET Status='Available', CurrentLicensePlate=NULL
                WHERE SlotCode=@slot AND Status='Occupied' AND CurrentLicensePlate=@plate
                AND NOT EXISTS (SELECT 1 FROM ParkingSessions WHERE SlotCode=@slot AND Status='Parking' AND CheckOutTime IS NULL)
                """, ("@slot", current.SlotCode), ("@plate", current.LicensePlate)) != 1)
                throw new ServiceException("Ô đỗ không khớp với lượt xe. Hãy kiểm tra dữ liệu.");
            if (await SqliteHelper.ExecuteAsync(connection, tx, """
                UPDATE ParkingCards SET Status=@status, LicensePlate=NULL
                WHERE CardCode=@card AND (Status='InUse' OR (Status='Lost' AND @status='Lost')) AND LicensePlate=@plate
                AND NOT EXISTS (SELECT 1 FROM ParkingSessions WHERE CardCode=@card AND Status='Parking' AND CheckOutTime IS NULL)
                """, ("@status", lostCard ? CardStatus.Lost : CardStatus.Available),
                ("@card", current.CardCode), ("@plate", current.LicensePlate)) != 1)
                throw new ServiceException("Thẻ không khớp với lượt xe. Hãy kiểm tra dữ liệu.");
            using var shiftQuery = connection.CreateCommand();
            shiftQuery.Transaction = tx;
            shiftQuery.CommandText = "SELECT ShiftId FROM WorkShifts WHERE UserId=@user AND Status='Open' AND EndTime IS NULL ORDER BY StartTime DESC LIMIT 1";
            shiftQuery.Parameters.AddWithValue("@user", user);
            var shiftValue = await shiftQuery.ExecuteScalarAsync();
            int? shiftId = shiftValue == null || shiftValue == DBNull.Value ? null : Convert.ToInt32(shiftValue);
            if (shiftId.HasValue)
                await SqliteHelper.ExecuteAsync(connection, tx,
                    "UPDATE WorkShifts SET TotalVehiclesOut=TotalVehiclesOut+1, TotalRevenue=TotalRevenue+@total WHERE ShiftId=@shift",
                    ("@total", quote.Total), ("@shift", shiftId.Value));
            await SqliteHelper.ExecuteAsync(connection, tx, """
                INSERT INTO PaymentTransactions (TransactionType,ReferenceId,Amount,PaymentMethod,CollectedByUserId,ShiftId,Note,CreatedAt)
                VALUES ('ParkingFee',@id,@total,'Cash',@user,@shift,@note,@time)
                """, ("@id", current.SessionId), ("@total", quote.Total), ("@user", user),
                ("@shift", shiftId), ("@note", $"Xe ra {current.LicensePlate}; phí {quote.ParkingFee}; phạt {quote.Penalty}"), ("@time", quote.ExitTime));
            if (lostCard)
                await SqliteHelper.ExecuteAsync(connection, tx, """
                    INSERT INTO IncidentReports (SessionId,CardCode,LicensePlate,CustomerName,CustomerPhone,
                        CustomerIdentityCard,IncidentType,CompensationFee,HandledByUserId,CreatedAt)
                    VALUES (@id,@card,@plate,@name,@phone,@identity,'LostCard',@penalty,@user,@time)
                    """, ("@id", current.SessionId), ("@card", current.CardCode), ("@plate", current.LicensePlate),
                    ("@name", report!.FullName.Trim()), ("@phone", report.Phone.Trim()),
                    ("@identity", report.IdentityCard.Trim()), ("@penalty", quote.Penalty), ("@user", user), ("@time", quote.ExitTime));
        });
    }
}

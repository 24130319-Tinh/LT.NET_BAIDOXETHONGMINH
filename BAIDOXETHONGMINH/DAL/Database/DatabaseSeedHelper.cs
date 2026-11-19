using System.Text;
using Microsoft.Data.Sqlite;

namespace BAIDOXETHONGMINH.DAL.Database;

public static class DatabaseSeedHelper
{
    // Hàm xóa sạch toàn bộ dữ liệu trong cả 12 bảng và đặt lại bộ đếm tự tăng về 1
    public static void ClearAllData()
    {
        using SqliteConnection connection = SqliteDbContext.GetConnection();
        using SqliteTransaction transaction = connection.BeginTransaction();

        try
        {
            const string deleteAllSql = @"
                DELETE FROM PaymentTransactions;
                DELETE FROM IncidentReports;
                DELETE FROM ParkingSessions;
                DELETE FROM WorkShifts;
                DELETE FROM MonthlyPasses;
                DELETE FROM Customers;
                DELETE FROM ParkingCards;
                DELETE FROM ParkingSlots;
                DELETE FROM ParkingZones;
                DELETE FROM PricingRules;
                DELETE FROM VehicleTypes;
                DELETE FROM Users;
                DELETE FROM sqlite_sequence;";

            using SqliteCommand cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = deleteAllSql;
            cmd.ExecuteNonQuery();

            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    // Hàm nạp bộ dữ liệu khởi tạo và dữ liệu kiểm thử mẫu quy mô 6 khu, 96 ô và 96 thẻ vào CSDL
    public static void SeedTestData()
    {
        using SqliteConnection connection = SqliteDbContext.GetConnection();
        using SqliteTransaction transaction = connection.BeginTransaction();

        try
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine(@"
                INSERT OR IGNORE INTO Users (UserId, Username, PasswordHash, FullName, Phone, Email, IdentityCard, EmployeeCode, IsActive) VALUES
                (1, 'admin', '1', 'Admin Tính', '0901234567', 'admin@gmail.com', '079204009999', 'NV01', 1);

                INSERT OR IGNORE INTO VehicleTypes (VehicleTypeId, TypeName, Description) VALUES
                ('XEMAY', 'Xe máy', 'Xe máy 2 bánh / Xe tay ga'),
                ('OTO', 'Ô tô', 'Ô tô con 4 - 7 chỗ');

                INSERT OR IGNORE INTO PricingRules
                (RuleId, VehicleTypeId, DayTurnPrice, OvernightPrice, HourlyPrice, MonthlyPassPrice, LostCardPenalty, UpdatedAt)
                VALUES
                (1, 'XEMAY', 5000, 10000, 3000, 150000, 50000, datetime('now', 'localtime')),
                (2, 'OTO', 30000, 60000, 20000, 1200000, 50000, datetime('now', 'localtime'));

                UPDATE PricingRules SET LostCardPenalty = 50000 WHERE LostCardPenalty <> 50000;

                INSERT OR IGNORE INTO ParkingZones (ZoneId, ZoneName, VehicleTypeId, MaxCapacity) VALUES
                ('ZONE_A', 'Khu A - Xe máy', 'XEMAY', 20),
                ('ZONE_B', 'Khu B - Xe máy', 'XEMAY', 20),
                ('ZONE_C', 'Khu C - Xe máy', 'XEMAY', 20),
                ('ZONE_D', 'Khu D - Xe máy', 'XEMAY', 20),
                ('ZONE_E', 'Khu E - Ô tô', 'OTO', 8),
                ('ZONE_F', 'Khu F - Ô tô', 'OTO', 8);");

            AppendZoneSlotsSql(sqlBuilder, "ZONE_A", "A", 20);
            AppendZoneSlotsSql(sqlBuilder, "ZONE_B", "B", 20);
            AppendZoneSlotsSql(sqlBuilder, "ZONE_C", "C", 20);
            AppendZoneSlotsSql(sqlBuilder, "ZONE_D", "D", 20);
            AppendZoneSlotsSql(sqlBuilder, "ZONE_E", "E", 8);
            AppendZoneSlotsSql(sqlBuilder, "ZONE_F", "F", 8);

            sqlBuilder.AppendLine(@"
                UPDATE ParkingSlots SET Status = 'Occupied', CurrentLicensePlate = '59X3-112.23' WHERE SlotCode = 'A-01';
                UPDATE ParkingSlots SET Status = 'Occupied', CurrentLicensePlate = '51H-999.99' WHERE SlotCode = 'E-01';
                UPDATE ParkingSlots SET Status = 'Occupied', CurrentLicensePlate = '51G-456.78' WHERE SlotCode = 'E-02';");

            for (int i = 1; i <= 96; i++)
            {
                string cardCode = $"THE{i:D3}";
                sqlBuilder.AppendLine($"INSERT OR IGNORE INTO ParkingCards (CardCode, LicensePlate, Status) VALUES ('{cardCode}', NULL, 'Available');");
            }

            sqlBuilder.AppendLine(@"
                UPDATE ParkingCards SET Status = 'InUse', LicensePlate = '59X3-112.23' WHERE CardCode = 'THE001';
                UPDATE ParkingCards SET Status = 'InUse', LicensePlate = '51G-456.78' WHERE CardCode = 'THE002';
                UPDATE ParkingCards SET Status = 'InUse', LicensePlate = '51H-999.99' WHERE CardCode = 'THE003';

                INSERT OR IGNORE INTO Customers (CustomerId, FullName, Phone, IdentityCard, CreatedAt) VALUES
                (1, 'Nguyễn Văn An', '0909111222', '079204001111', datetime('now', 'localtime')),
                (2, 'Trần Thị Bình', '0909333444', '079204002222', datetime('now', 'localtime'));

                INSERT OR IGNORE INTO MonthlyPasses
                (PassId, CustomerId, LicensePlate, VehicleTypeId, VehicleBrandColor, StartDate, EndDate, Status)
                VALUES
                (1, 1, '51H-999.99', 'OTO', 'Mazda CX-5 Trắng', date('now', '-10 days'), date('now', '+20 days'), 'Active'),
                (2, 2, '59P2-345.67', 'XEMAY', 'Honda Vision Đen', date('now', '-5 days'), date('now', '+25 days'), 'Active');

                INSERT OR IGNORE INTO WorkShifts
                (ShiftId, UserId, ShiftName, StartTime, EndTime, TotalVehiclesIn, TotalVehiclesOut, TotalRevenue, Status)
                VALUES
                (1, 1, 'Ca Sáng', datetime('now', 'localtime', '-4 hours'), NULL, 3, 0, 0, 'Open');

                INSERT OR IGNORE INTO ParkingSessions
                (
                    SessionId, CardCode, LicensePlate, VehicleTypeId, SlotCode, CheckInTime,
                    CheckOutTime, CheckInImagePath, CheckOutImagePath, CheckInUserId, CheckOutUserId,
                    IsMonthlyPass, ParkingFee, PenaltyFee, TotalAmount, Status
                )
                VALUES
                (1, 'THE003', '51H-999.99', 'OTO', 'E-01', datetime('now', 'localtime', '-100 minutes'), NULL, NULL, NULL, 1, NULL, 1, 0, 0, 0, 'Parking'),
                (2, 'THE002', '51G-456.78', 'OTO', 'E-02', datetime('now', 'localtime', '-185 minutes'), NULL, NULL, NULL, 1, NULL, 0, 0, 0, 0, 'Parking'),
                (3, 'THE001', '59X3-112.23', 'XEMAY', 'A-01', datetime('now', 'localtime', '-135 minutes'), NULL, NULL, NULL, 1, NULL, 0, 0, 0, 0, 'Parking');");

            using SqliteCommand cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = sqlBuilder.ToString();
            cmd.ExecuteNonQuery();

            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    // Hàm sinh các câu lệnh SQL tạo danh sách ô đỗ trống cho một khu vực theo tiền tố và sức chứa
    private static void AppendZoneSlotsSql(StringBuilder sqlBuilder, string zoneId, string prefix, int capacity)
    {
        for (int i = 1; i <= capacity; i++)
        {
            string slotCode = $"{prefix}-{i:D2}";
            sqlBuilder.AppendLine($"INSERT OR IGNORE INTO ParkingSlots (SlotCode, ZoneId, Status, CurrentLicensePlate) VALUES ('{slotCode}', '{zoneId}', 'Available', NULL);");
        }
    }

    // Hàm kiểm tra nếu bảng danh mục đang trống thì mới nạp dữ liệu mẫu
    public static void SeedTestDataIfEmpty()
    {
        using (SqliteConnection connection = SqliteDbContext.GetConnection())
        {
            using SqliteCommand checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM VehicleTypes;";
            object? scalar = checkCmd.ExecuteScalar();
            long count = scalar != null && scalar != DBNull.Value ? Convert.ToInt64(scalar) : 0;

            if (count > 0)
            {
                return;
            }
        }

        SeedTestData();
    }

    // Hàm xóa toàn bộ dữ liệu cũ và nạp lại ngay bộ dữ liệu khởi tạo chuẩn
    public static void ResetAndSeedDatabase()
    {
        ClearAllData();
        SeedTestData();
    }
}

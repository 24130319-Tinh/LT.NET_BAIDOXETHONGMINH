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

    // Hàm nạp bộ dữ liệu khởi tạo và dữ liệu kiểm thử mẫu vào cơ sở dữ liệu
    public static void SeedTestData()
    {
        using SqliteConnection connection = SqliteDbContext.GetConnection();
        using SqliteTransaction transaction = connection.BeginTransaction();

        try
        {
            const string seedSql = @"
                INSERT OR IGNORE INTO Users (UserId, Username, PasswordHash, FullName, Phone, IsActive) VALUES
                (1, 'admin', '123456', 'Admin Tính', '0901234567', 1);

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
                ('ZONE_A', 'Khu A - Xe máy', 'XEMAY', 12),
                ('ZONE_B', 'Khu B - Ô tô', 'OTO', 8);

                INSERT OR IGNORE INTO ParkingSlots (SlotCode, ZoneId, Status, CurrentLicensePlate) VALUES
                ('A-01', 'ZONE_A', 'Occupied', '59X3-112.23'),
                ('A-02', 'ZONE_A', 'Available', NULL),
                ('A-03', 'ZONE_A', 'Available', NULL),
                ('A-04', 'ZONE_A', 'Available', NULL),
                ('A-05', 'ZONE_A', 'Available', NULL),
                ('A-06', 'ZONE_A', 'Available', NULL),
                ('A-07', 'ZONE_A', 'Available', NULL),
                ('A-08', 'ZONE_A', 'Available', NULL),
                ('A-09', 'ZONE_A', 'Available', NULL),
                ('A-10', 'ZONE_A', 'Available', NULL),
                ('A-11', 'ZONE_A', 'Available', NULL),
                ('A-12', 'ZONE_A', 'Available', NULL),
                ('B-01', 'ZONE_B', 'Occupied', '51H-999.99'),
                ('B-02', 'ZONE_B', 'Occupied', '51G-456.78'),
                ('B-03', 'ZONE_B', 'Available', NULL),
                ('B-04', 'ZONE_B', 'Available', NULL),
                ('B-05', 'ZONE_B', 'Available', NULL),
                ('B-06', 'ZONE_B', 'Available', NULL),
                ('B-07', 'ZONE_B', 'Available', NULL),
                ('B-08', 'ZONE_B', 'Available', NULL);

                INSERT OR IGNORE INTO ParkingCards (CardCode, LicensePlate, Status) VALUES
                ('THE001', '59X3-112.23', 'InUse'),
                ('THE002', '51G-456.78', 'InUse'),
                ('THE003', '51H-999.99', 'InUse'),
                ('THE004', NULL, 'Available'),
                ('THE005', NULL, 'Available'),
                ('THE006', NULL, 'Available'),
                ('THE007', NULL, 'Available'),
                ('THE008', NULL, 'Available'),
                ('THE009', NULL, 'Available'),
                ('THE010', NULL, 'Available'),
                ('THE011', NULL, 'Available'),
                ('THE012', NULL, 'Available'),
                ('THE013', NULL, 'Available'),
                ('THE014', NULL, 'Available'),
                ('THE015', NULL, 'Available'),
                ('THE016', NULL, 'Available'),
                ('THE017', NULL, 'Available'),
                ('THE018', NULL, 'Available'),
                ('THE019', NULL, 'Available'),
                ('THE020', NULL, 'Available');

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
                    SessionId,
                    CardCode,
                    LicensePlate,
                    VehicleTypeId,
                    SlotCode,
                    CheckInTime,
                    CheckOutTime,
                    CheckInImagePath,
                    CheckOutImagePath,
                    CheckInUserId,
                    CheckOutUserId,
                    IsMonthlyPass,
                    ParkingFee,
                    PenaltyFee,
                    TotalAmount,
                    Status
                )
                VALUES
                (1, 'THE003', '51H-999.99', 'OTO', 'B-01', datetime('now', 'localtime', '-100 minutes'), NULL, NULL, NULL, 1, NULL, 1, 0, 0, 0, 'Parking'),
                (2, 'THE002', '51G-456.78', 'OTO', 'B-02', datetime('now', 'localtime', '-185 minutes'), NULL, NULL, NULL, 1, NULL, 0, 0, 0, 0, 'Parking'),
                (3, 'THE001', '59X3-112.23', 'XEMAY', 'A-01', datetime('now', 'localtime', '-135 minutes'), NULL, NULL, NULL, 1, NULL, 0, 0, 0, 0, 'Parking');";

            using SqliteCommand cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = seedSql;
            cmd.ExecuteNonQuery();

            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
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

using Microsoft.Data.Sqlite;

namespace BAIDOXETHONGMINH.DAL.Database;

/// <summary>
/// Tiện ích hỗ trợ xóa sạch toàn bộ dữ liệu trong smart_parking.db
/// và nạp lại bộ dữ liệu khởi tạo chuẩn giống trong schema.sql.
/// </summary>
public static class DatabaseSeedHelper
{
    /// <summary>
    /// Chức năng 1: Xóa sạch toàn bộ dữ liệu trong cả 12 bảng của cơ sở dữ liệu
    /// và đưa bộ đếm tự tăng (AUTOINCREMENT) về 1.
    /// </summary>
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

    /// <summary>
    /// Chức năng 2: Nạp bộ dữ liệu khởi tạo & dữ liệu test giống trong schema.sql.
    /// </summary>
    public static void SeedTestData()
    {
        using SqliteConnection connection = SqliteDbContext.GetConnection();
        using SqliteTransaction transaction = connection.BeginTransaction();

        try
        {
            const string seedSql = @"
                -- 1. Tài khoản nhân viên trực ca mặc định
                INSERT OR IGNORE INTO Users (UserId, Username, PasswordHash, FullName, Phone, IsActive) VALUES
                (1, 'admin', '123456', 'Admin Tính', '0901234567', 1);

                -- 2. Danh mục 2 loại phương tiện (XEMAY, OTO)
                INSERT OR IGNORE INTO VehicleTypes (VehicleTypeId, TypeName, Description) VALUES
                ('XEMAY', 'Xe máy', 'Xe máy 2 bánh / Xe tay ga'),
                ('OTO', 'Ô tô', 'Ô tô con 4 - 7 chỗ');

                -- 3. Bảng giá đỗ xe (Phạt mất thẻ thống nhất 50.000đ cho mọi loại thẻ)
                INSERT OR IGNORE INTO PricingRules
                (RuleId, VehicleTypeId, DayTurnPrice, OvernightPrice, HourlyPrice, MonthlyPassPrice, LostCardPenalty, UpdatedAt)
                VALUES
                (1, 'XEMAY', 5000, 10000, 3000, 150000, 50000, datetime('now', 'localtime')),
                (2, 'OTO', 30000, 60000, 20000, 1200000, 50000, datetime('now', 'localtime'));

                UPDATE PricingRules SET LostCardPenalty = 50000 WHERE LostCardPenalty <> 50000;

                -- 4. Hai khu vực đỗ xe: Khu A (Xe máy - 12 ô) & Khu B (Ô tô - 8 ô)
                INSERT OR IGNORE INTO ParkingZones (ZoneId, ZoneName, VehicleTypeId, MaxCapacity) VALUES
                ('ZONE_A', 'Khu A - Xe máy', 'XEMAY', 12),
                ('ZONE_B', 'Khu B - Ô tô', 'OTO', 8);

                -- 5. Danh sách 20 ô đỗ xe (12 ô Khu A, 8 ô Khu B)
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

                -- 6. Danh sách 20 thẻ từ (THE001 .. THE020)
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

                -- 7. Hồ sơ khách hàng đăng ký Vé tháng mẫu
                INSERT OR IGNORE INTO Customers (CustomerId, FullName, Phone, IdentityCard, CreatedAt) VALUES
                (1, 'Nguyễn Văn An', '0909111222', '079204001111', datetime('now', 'localtime')),
                (2, 'Trần Thị Bình', '0909333444', '079204002222', datetime('now', 'localtime'));

                -- 8. Danh sách Vé tháng còn hạn (51H-999.99 và 59P2-345.67)
                INSERT OR IGNORE INTO MonthlyPasses
                (PassId, CustomerId, LicensePlate, VehicleTypeId, VehicleBrandColor, StartDate, EndDate, Status)
                VALUES
                (1, 1, '51H-999.99', 'OTO', 'Mazda CX-5 Trắng', date('now', '-10 days'), date('now', '+20 days'), 'Active'),
                (2, 2, '59P2-345.67', 'XEMAY', 'Honda Vision Đen', date('now', '-5 days'), date('now', '+25 days'), 'Active');

                -- 9. Ca trực đang mở của nhân viên
                INSERT OR IGNORE INTO WorkShifts
                (ShiftId, UserId, ShiftName, StartTime, EndTime, TotalVehiclesIn, TotalVehiclesOut, TotalRevenue, Status)
                VALUES
                (1, 1, 'Ca Sáng', datetime('now', 'localtime', '-4 hours'), NULL, 3, 0, 0, 'Open');

                -- 10. Ba lượt xe đang đỗ trong bãi (Status = 'Parking')
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

    /// <summary>
    /// Xóa sạch dữ liệu cũ và nạp lại ngay bộ dữ liệu test khởi tạo chuẩn.
    /// </summary>
    public static void ResetAndSeedDatabase()
    {
        ClearAllData();
        SeedTestData();
    }
}

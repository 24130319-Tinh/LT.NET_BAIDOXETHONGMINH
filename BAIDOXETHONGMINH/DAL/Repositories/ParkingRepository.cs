using System.Globalization;
using BAIDOXETHONGMINH.DAL.Database;
using BAIDOXETHONGMINH.DTO.Entities;
using Microsoft.Data.Sqlite;

namespace BAIDOXETHONGMINH.DAL.Repositories;

/// <summary>
/// Tầng truy xuất dữ liệu SQLite cho Nghiệp vụ 1 (Check-in) & Nghiệp vụ 2 (Check-out)
/// </summary>
public class ParkingRepository
{
    /// <summary>
    /// Nạp danh mục nền tảng (User, Loại xe, Bảng giá, Khu A/B, 20 Ô đỗ, 20 Thẻ xe)
    /// nếu các bảng danh mục đang rỗng, giúp test độc lập Cổng Vào / Cổng Ra.
    /// </summary>
    public void SeedBaseCatalogIfEmpty()
    {
        using SqliteConnection connection = SqliteDbContext.GetConnection();

        using SqliteCommand checkCmd = connection.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM VehicleTypes;";
        object? scalarResult = checkCmd.ExecuteScalar();
        long existingCount = scalarResult != null ? Convert.ToInt64(scalarResult) : 0;

        if (existingCount > 0)
        {
            return;
        }

        using SqliteTransaction transaction = connection.BeginTransaction();
        try
        {
            string nowText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // 1. Tài khoản trực ca mặc định (UserId = 1)
            ExecuteNonQuery(
                connection,
                transaction,
                @"INSERT OR IGNORE INTO Users (Username, PasswordHash, FullName, Phone, IsActive)
                  VALUES ('admin', '123456', 'Admin Tính', '0901234567', 1);"
            );

            // 2. Hai loại phương tiện cơ bản: XEMAY & OTO
            ExecuteNonQuery(
                connection,
                transaction,
                @"INSERT OR IGNORE INTO VehicleTypes (VehicleTypeId, TypeName, Description) VALUES
                  ('XEMAY', 'Xe máy', 'Xe máy 2 bánh / Xe tay ga'),
                  ('OTO', 'Ô tô', 'Ô tô con 4 - 7 chỗ');"
            );

            // 3. Bảng giá mặc định cho Xe máy & Ô tô
            ExecuteNonQuery(
                connection,
                transaction,
                $@"INSERT OR IGNORE INTO PricingRules
                   (VehicleTypeId, DayTurnPrice, OvernightPrice, HourlyPrice, MonthlyPassPrice, LostCardPenalty, UpdatedAt)
                   VALUES
                   ('XEMAY', 5000, 10000, 3000, 150000, 50000, '{nowText}'),
                   ('OTO', 30000, 60000, 20000, 1200000, 100000, '{nowText}');"
            );

            // 4. Hai khu vực đỗ xe: Khu A (Xe máy - 12 ô) & Khu B (Ô tô - 8 ô)
            ExecuteNonQuery(
                connection,
                transaction,
                @"INSERT OR IGNORE INTO ParkingZones (ZoneId, ZoneName, VehicleTypeId, MaxCapacity) VALUES
                  ('ZONE_A', 'Khu A - Xe máy', 'XEMAY', 12),
                  ('ZONE_B', 'Khu B - Ô tô', 'OTO', 8);"
            );

            // 5. 12 ô đỗ Khu A (A-01 .. A-12) và 8 ô đỗ Khu B (B-01 .. B-08)
            for (int i = 1; i <= 12; i++)
            {
                string slotCode = $"A-{i:D2}";
                ExecuteNonQuery(
                    connection,
                    transaction,
                    $"INSERT OR IGNORE INTO ParkingSlots (SlotCode, ZoneId, Status, CurrentLicensePlate) VALUES ('{slotCode}', 'ZONE_A', 'Available', NULL);"
                );
            }

            for (int i = 1; i <= 8; i++)
            {
                string slotCode = $"B-{i:D2}";
                ExecuteNonQuery(
                    connection,
                    transaction,
                    $"INSERT OR IGNORE INTO ParkingSlots (SlotCode, ZoneId, Status, CurrentLicensePlate) VALUES ('{slotCode}', 'ZONE_B', 'Available', NULL);"
                );
            }

            // 6. 20 thẻ xe (THE001 .. THE020)
            for (int i = 1; i <= 20; i++)
            {
                string cardCode = $"THE{i:D3}";
                ExecuteNonQuery(
                    connection,
                    transaction,
                    $"INSERT OR IGNORE INTO ParkingCards (CardCode, LicensePlate, Status) VALUES ('{cardCode}', NULL, 'Available');"
                );
            }

            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách tất cả các loại phương tiện (VD: XEMAY, OTO)
    /// </summary>
    public List<VehicleType> GetVehicleTypes()
    {
        List<VehicleType> vehicleTypes = new List<VehicleType>();

        using SqliteConnection connection = SqliteDbContext.GetConnection();
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT VehicleTypeId, TypeName, Description
            FROM VehicleTypes
            ORDER BY VehicleTypeId DESC;";

        using SqliteDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            VehicleType item = new VehicleType
            {
                VehicleTypeId = reader.GetString(0),
                TypeName = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2)
            };
            vehicleTypes.Add(item);
        }

        return vehicleTypes;
    }

    /// <summary>
    /// Lấy danh sách các thẻ xe đang ở trạng thái sẵn sàng cấp phát (Status = 'Available')
    /// </summary>
    public List<ParkingCard> GetAvailableCards()
    {
        List<ParkingCard> availableCards = new List<ParkingCard>();

        using SqliteConnection connection = SqliteDbContext.GetConnection();
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT CardCode, LicensePlate, Status
            FROM ParkingCards
            WHERE Status = 'Available'
            ORDER BY CardCode ASC;";

        using SqliteDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            ParkingCard card = new ParkingCard
            {
                CardCode = reader.GetString(0),
                LicensePlate = reader.IsDBNull(1) ? null : reader.GetString(1),
                Status = reader.GetString(2)
            };
            availableCards.Add(card);
        }

        return availableCards;
    }

    /// <summary>
    /// Lấy danh sách các ô đỗ còn trống (Status = 'Available') tương ứng với loại phương tiện
    /// </summary>
    public List<ParkingSlot> GetAvailableSlotsByVehicleType(string vehicleTypeId)
    {
        List<ParkingSlot> availableSlots = new List<ParkingSlot>();

        using SqliteConnection connection = SqliteDbContext.GetConnection();
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT s.SlotCode, s.ZoneId, s.Status, s.CurrentLicensePlate
            FROM ParkingSlots s
            INNER JOIN ParkingZones z ON s.ZoneId = z.ZoneId
            WHERE z.VehicleTypeId = @VehicleTypeId
              AND s.Status = 'Available'
            ORDER BY s.SlotCode ASC;";
        cmd.Parameters.AddWithValue("@VehicleTypeId", vehicleTypeId);

        using SqliteDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            ParkingSlot slot = new ParkingSlot
            {
                SlotCode = reader.GetString(0),
                ZoneId = reader.GetString(1),
                Status = reader.GetString(2),
                CurrentLicensePlate = reader.IsDBNull(3) ? null : reader.GetString(3)
            };
            availableSlots.Add(slot);
        }

        return availableSlots;
    }

    /// <summary>
    /// Đếm số lượng ô đỗ còn trống trên tổng số ô đỗ của toàn bãi xe
    /// </summary>
    public (int AvailableSlots, int TotalSlots) GetSlotCapacitySummary()
    {
        using SqliteConnection connection = SqliteDbContext.GetConnection();
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT
                SUM(CASE WHEN Status = 'Available' THEN 1 ELSE 0 END) AS AvailableCount,
                COUNT(*) AS TotalCount
            FROM ParkingSlots;";

        using SqliteDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            int availableCount = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
            int totalCount = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
            return (availableCount, totalCount);
        }

        return (0, 0);
    }

    /// <summary>
    /// Kiểm tra xem biển số xe này có đang gửi trong bãi chưa lấy ra (Status = 'Parking') hay không
    /// </summary>
    public bool IsLicensePlateCurrentlyParked(string licensePlate)
    {
        string normalizedPlate = NormalizePlate(licensePlate);
        if (string.IsNullOrEmpty(normalizedPlate))
        {
            return false;
        }

        using SqliteConnection connection = SqliteDbContext.GetConnection();
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT COUNT(*)
            FROM ParkingSessions
            WHERE UPPER(REPLACE(REPLACE(LicensePlate, ' ', ''), '-', '')) = @NormalizedPlate
              AND Status = 'Parking';";
        cmd.Parameters.AddWithValue("@NormalizedPlate", normalizedPlate);

        object? scalar = cmd.ExecuteScalar();
        long count = scalar != null ? Convert.ToInt64(scalar) : 0;
        return count > 0;
    }

    /// <summary>
    /// Tìm Vé tháng đang hoạt động (Status = 'Active') và còn thời hạn của biển số xe
    /// </summary>
    public MonthlyPass? GetActiveMonthlyPassByPlate(string licensePlate)
    {
        string normalizedPlate = NormalizePlate(licensePlate);
        if (string.IsNullOrEmpty(normalizedPlate))
        {
            return null;
        }

        using SqliteConnection connection = SqliteDbContext.GetConnection();
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT PassId, CustomerId, LicensePlate, VehicleTypeId, VehicleBrandColor, StartDate, EndDate, Status
            FROM MonthlyPasses
            WHERE UPPER(REPLACE(REPLACE(LicensePlate, ' ', ''), '-', '')) = @NormalizedPlate
              AND Status = 'Active'
            ORDER BY EndDate DESC;";
        cmd.Parameters.AddWithValue("@NormalizedPlate", normalizedPlate);

        using SqliteDataReader reader = cmd.ExecuteReader();
        DateTime today = DateTime.Now.Date;

        while (reader.Read())
        {
            DateTime startDate = ParseSqliteDateTime(reader.GetString(5));
            DateTime endDate = ParseSqliteDateTime(reader.GetString(6));

            if (today >= startDate.Date && today <= endDate.Date)
            {
                MonthlyPass pass = new MonthlyPass
                {
                    PassId = reader.GetInt32(0),
                    CustomerId = reader.GetInt32(1),
                    LicensePlate = reader.GetString(2),
                    VehicleTypeId = reader.GetString(3),
                    VehicleBrandColor = reader.IsDBNull(4) ? null : reader.GetString(4),
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = reader.GetString(7)
                };
                return pass;
            }
        }

        return null;
    }

    private static string NormalizePlate(string rawPlate)
    {
        if (string.IsNullOrWhiteSpace(rawPlate))
        {
            return string.Empty;
        }

        return rawPlate.Trim().Replace(" ", string.Empty).Replace("-", string.Empty).ToUpperInvariant();
    }

    private static DateTime ParseSqliteDateTime(string rawDate)
    {
        if (DateTime.TryParse(rawDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
        {
            return parsed;
        }

        string[] formats = new string[] { "dd/MM/yyyy", "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy HH:mm:ss" };
        if (DateTime.TryParseExact(rawDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime exactParsed))
        {
            return exactParsed;
        }

        return DateTime.MinValue;
    }

    private static void ExecuteNonQuery(SqliteConnection connection, SqliteTransaction transaction, string sql)
    {
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}

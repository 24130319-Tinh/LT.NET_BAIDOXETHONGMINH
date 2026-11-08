using BAIDOXETHONGMINH.DAL.Database;
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

    private static void ExecuteNonQuery(SqliteConnection connection, SqliteTransaction transaction, string sql)
    {
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}

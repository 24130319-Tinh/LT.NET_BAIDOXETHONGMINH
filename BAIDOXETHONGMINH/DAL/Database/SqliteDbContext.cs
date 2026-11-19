using Microsoft.Data.Sqlite;

namespace BAIDOXETHONGMINH.DAL.Database;

public static class SqliteDbContext
{
    public static string DatabaseFilePath { get; } = ResolveDatabasePath();

    public static string ConnectionString => $"Data Source={DatabaseFilePath}";

    // Hàm xác định đường dẫn tuyệt đối tới file cơ sở dữ liệu smart_parking.db
    private static string ResolveDatabasePath()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        DirectoryInfo? dir = new DirectoryInfo(baseDir);

        while (dir != null)
        {
            string candidate = Path.Combine(dir.FullName, "DAL", "Database", "smart_parking.db");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            string nestedCandidate = Path.Combine(dir.FullName, "BAIDOXETHONGMINH", "DAL", "Database", "smart_parking.db");
            if (File.Exists(nestedCandidate))
            {
                return nestedCandidate;
            }

            dir = dir.Parent;
        }

        string fallbackDir = Path.Combine(baseDir, "DAL", "Database");
        Directory.CreateDirectory(fallbackDir);
        return Path.Combine(fallbackDir, "smart_parking.db");
    }

    // Hàm mở kết nối SQLite và bật chế độ kiểm tra khóa ngoại (PRAGMA foreign_keys = ON)
    public static SqliteConnection GetConnection()
    {
        SqliteConnection connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using SqliteCommand pragmaCmd = connection.CreateCommand();
        pragmaCmd.CommandText = "PRAGMA foreign_keys = ON;";
        pragmaCmd.ExecuteNonQuery();

        return connection;
    }

    // Hàm khởi tạo cấu trúc 12 bảng cho CSDL, nâng cấp cột Users, nạp dữ liệu mẫu nếu trống và nâng cấp bố cục 6 khu
    public static void InitializeDatabase()
    {
        using (SqliteConnection connection = GetConnection())
        {
            using SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = GetSchemaSql();
            cmd.ExecuteNonQuery();
        }

        EnsureUserSchemaAndDefaultCredentials();
        DatabaseSeedHelper.SeedTestDataIfEmpty();
        UpgradeParkingLayoutIfNeeded();
    }

    // Hàm đảm bảo bảng Users có đủ 3 cột Email, IdentityCard, EmployeeCode và khởi tạo thông tin đăng nhập mặc định (mật khẩu 1)
    private static void EnsureUserSchemaAndDefaultCredentials()
    {
        using SqliteConnection connection = GetConnection();
        using SqliteTransaction transaction = connection.BeginTransaction();

        try
        {
            HashSet<string> existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (SqliteCommand infoCmd = connection.CreateCommand())
            {
                infoCmd.Transaction = transaction;
                infoCmd.CommandText = "PRAGMA table_info(Users);";
                using SqliteDataReader reader = infoCmd.ExecuteReader();
                while (reader.Read())
                {
                    existingColumns.Add(reader.GetString(1));
                }
            }

            if (!existingColumns.Contains("Email"))
            {
                using SqliteCommand addEmailCmd = connection.CreateCommand();
                addEmailCmd.Transaction = transaction;
                addEmailCmd.CommandText = "ALTER TABLE Users ADD COLUMN Email TEXT NOT NULL DEFAULT '';";
                addEmailCmd.ExecuteNonQuery();
            }

            if (!existingColumns.Contains("IdentityCard"))
            {
                using SqliteCommand addCccdCmd = connection.CreateCommand();
                addCccdCmd.Transaction = transaction;
                addCccdCmd.CommandText = "ALTER TABLE Users ADD COLUMN IdentityCard TEXT NOT NULL DEFAULT '';";
                addCccdCmd.ExecuteNonQuery();
            }

            if (!existingColumns.Contains("EmployeeCode"))
            {
                using SqliteCommand addEmpCodeCmd = connection.CreateCommand();
                addEmpCodeCmd.Transaction = transaction;
                addEmpCodeCmd.CommandText = "ALTER TABLE Users ADD COLUMN EmployeeCode TEXT NOT NULL DEFAULT '';";
                addEmpCodeCmd.ExecuteNonQuery();
            }

            using (SqliteCommand ensureSettingsCmd = connection.CreateCommand())
            {
                ensureSettingsCmd.Transaction = transaction;
                ensureSettingsCmd.CommandText = "CREATE TABLE IF NOT EXISTS AppSettings (SettingKey TEXT PRIMARY KEY, SettingValue TEXT NOT NULL);";
                ensureSettingsCmd.ExecuteNonQuery();
            }

            using (SqliteCommand ensureAdminCmd = connection.CreateCommand())
            {
                ensureAdminCmd.Transaction = transaction;
                ensureAdminCmd.CommandText = @"
                    INSERT OR IGNORE INTO Users (UserId, Username, PasswordHash, FullName, Phone, Email, IdentityCard, EmployeeCode, IsActive)
                    VALUES (1, 'admin', '1', 'Admin Tính', '0901234567', 'admin@gmail.com', '079204009999', 'NV01', 1);

                    UPDATE Users
                    SET Email = CASE WHEN TRIM(COALESCE(Email, '')) = '' THEN 'admin@gmail.com' ELSE Email END,
                        IdentityCard = CASE WHEN TRIM(COALESCE(IdentityCard, '')) = '' THEN '079204009999' ELSE IdentityCard END,
                        EmployeeCode = CASE WHEN TRIM(COALESCE(EmployeeCode, '')) = '' THEN 'NV01' ELSE EmployeeCode END,
                        IsActive = 1
                    WHERE Username = 'admin' OR UserId = 1;";
                ensureAdminCmd.ExecuteNonQuery();
            }

            bool passwordInitialized = false;
            using (SqliteCommand checkPwdFlagCmd = connection.CreateCommand())
            {
                checkPwdFlagCmd.Transaction = transaction;
                checkPwdFlagCmd.CommandText = "SELECT SettingValue FROM AppSettings WHERE SettingKey = 'DefaultAdminPasswordV1_SetTo1' LIMIT 1;";
                object? flagVal = checkPwdFlagCmd.ExecuteScalar();
                passwordInitialized = flagVal != null && flagVal != DBNull.Value && string.Equals(flagVal.ToString(), "1", StringComparison.Ordinal);
            }

            if (!passwordInitialized)
            {
                using SqliteCommand setPwdCmd = connection.CreateCommand();
                setPwdCmd.Transaction = transaction;
                setPwdCmd.CommandText = @"
                    UPDATE Users SET PasswordHash = '1' WHERE Username = 'admin' OR UserId = 1;
                    INSERT OR REPLACE INTO AppSettings (SettingKey, SettingValue) VALUES ('DefaultAdminPasswordV1_SetTo1', '1');";
                setPwdCmd.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    // Hàm nâng cấp tự động một lần từ bãi 2 khu (20 chỗ) lên 6 khu (A-D Xe máy 20 chỗ, E-F Ô tô 8 chỗ, 96 thẻ) mà giữ nguyên dữ liệu cũ
    private static void UpgradeParkingLayoutIfNeeded()
    {
        using SqliteConnection connection = GetConnection();
        using SqliteTransaction transaction = connection.BeginTransaction();

        try
        {
            using (SqliteCommand ensureSettingsCmd = connection.CreateCommand())
            {
                ensureSettingsCmd.Transaction = transaction;
                ensureSettingsCmd.CommandText = "CREATE TABLE IF NOT EXISTS AppSettings (SettingKey TEXT PRIMARY KEY, SettingValue TEXT NOT NULL);";
                ensureSettingsCmd.ExecuteNonQuery();
            }

            using (SqliteCommand checkFlagCmd = connection.CreateCommand())
            {
                checkFlagCmd.Transaction = transaction;
                checkFlagCmd.CommandText = "SELECT SettingValue FROM AppSettings WHERE SettingKey = 'ParkingLotLayoutV2_96Slots' LIMIT 1;";
                object? flagVal = checkFlagCmd.ExecuteScalar();
                if (flagVal != null && flagVal != DBNull.Value && string.Equals(flagVal.ToString(), "1", StringComparison.Ordinal))
                {
                    transaction.Commit();
                    return;
                }
            }

            using (SqliteCommand ensureTypesCmd = connection.CreateCommand())
            {
                ensureTypesCmd.Transaction = transaction;
                ensureTypesCmd.CommandText = @"
                    INSERT OR IGNORE INTO VehicleTypes (VehicleTypeId, TypeName, Description) VALUES
                    ('XEMAY', 'Xe máy', 'Xe máy 2 bánh / Xe tay ga'),
                    ('OTO', 'Ô tô', 'Ô tô con 4 - 7 chỗ');";
                ensureTypesCmd.ExecuteNonQuery();
            }

            using (SqliteCommand upsertZonesCmd = connection.CreateCommand())
            {
                upsertZonesCmd.Transaction = transaction;
                upsertZonesCmd.CommandText = @"
                    INSERT INTO ParkingZones (ZoneId, ZoneName, VehicleTypeId, MaxCapacity) VALUES
                    ('ZONE_A', 'Khu A - Xe máy', 'XEMAY', 20),
                    ('ZONE_B', 'Khu B - Xe máy', 'XEMAY', 20),
                    ('ZONE_C', 'Khu C - Xe máy', 'XEMAY', 20),
                    ('ZONE_D', 'Khu D - Xe máy', 'XEMAY', 20),
                    ('ZONE_E', 'Khu E - Ô tô', 'OTO', 8),
                    ('ZONE_F', 'Khu F - Ô tô', 'OTO', 8)
                    ON CONFLICT(ZoneId) DO UPDATE SET
                        ZoneName = excluded.ZoneName,
                        VehicleTypeId = excluded.VehicleTypeId,
                        MaxCapacity = excluded.MaxCapacity;";
                upsertZonesCmd.ExecuteNonQuery();
            }

            (string ZoneId, string Prefix, int Capacity)[] zoneLayouts = new (string ZoneId, string Prefix, int Capacity)[]
            {
                ("ZONE_A", "A", 20),
                ("ZONE_B", "B", 20),
                ("ZONE_C", "C", 20),
                ("ZONE_D", "D", 20),
                ("ZONE_E", "E", 8),
                ("ZONE_F", "F", 8)
            };

            foreach ((string zoneId, string prefix, int capacity) in zoneLayouts)
            {
                for (int i = 1; i <= capacity; i++)
                {
                    string slotCode = $"{prefix}-{i:D2}";
                    using SqliteCommand insertSlotCmd = connection.CreateCommand();
                    insertSlotCmd.Transaction = transaction;
                    insertSlotCmd.CommandText = "INSERT OR IGNORE INTO ParkingSlots (SlotCode, ZoneId, Status, CurrentLicensePlate) VALUES (@code, @zone, 'Available', NULL);";
                    insertSlotCmd.Parameters.AddWithValue("@code", slotCode);
                    insertSlotCmd.Parameters.AddWithValue("@zone", zoneId);
                    insertSlotCmd.ExecuteNonQuery();
                }
            }

            for (int i = 1; i <= 96; i++)
            {
                string cardCode = $"THE{i:D3}";
                using SqliteCommand insertCardCmd = connection.CreateCommand();
                insertCardCmd.Transaction = transaction;
                insertCardCmd.CommandText = "INSERT OR IGNORE INTO ParkingCards (CardCode, LicensePlate, Status) VALUES (@card, NULL, 'Available');";
                insertCardCmd.Parameters.AddWithValue("@card", cardCode);
                insertCardCmd.ExecuteNonQuery();
            }

            List<(int SessionId, string OldSlot, string LicensePlate)> activeCarSessionsInB = new List<(int SessionId, string OldSlot, string LicensePlate)>();
            using (SqliteCommand queryActiveCarsCmd = connection.CreateCommand())
            {
                queryActiveCarsCmd.Transaction = transaction;
                queryActiveCarsCmd.CommandText = @"
                    SELECT SessionId, SlotCode, LicensePlate
                    FROM ParkingSessions
                    WHERE Status = 'Parking'
                      AND UPPER(VehicleTypeId) = 'OTO'
                      AND SlotCode LIKE 'B-%'
                    ORDER BY SessionId ASC;";
                using SqliteDataReader reader = queryActiveCarsCmd.ExecuteReader();
                while (reader.Read())
                {
                    activeCarSessionsInB.Add((reader.GetInt32(0), reader.GetString(1), reader.GetString(2)));
                }
            }

            foreach ((int sessionId, string oldSlot, string licensePlate) in activeCarSessionsInB)
            {
                string preferredNewSlot = "E-" + oldSlot.Substring(2);
                string? targetNewSlot = null;

                using (SqliteCommand checkPreferredCmd = connection.CreateCommand())
                {
                    checkPreferredCmd.Transaction = transaction;
                    checkPreferredCmd.CommandText = "SELECT SlotCode FROM ParkingSlots WHERE SlotCode = @slot AND Status = 'Available' LIMIT 1;";
                    checkPreferredCmd.Parameters.AddWithValue("@slot", preferredNewSlot);
                    object? prefObj = checkPreferredCmd.ExecuteScalar();
                    if (prefObj != null && prefObj != DBNull.Value)
                    {
                        targetNewSlot = prefObj.ToString();
                    }
                }

                if (string.IsNullOrEmpty(targetNewSlot))
                {
                    using SqliteCommand findCarSlotCmd = connection.CreateCommand();
                    findCarSlotCmd.Transaction = transaction;
                    findCarSlotCmd.CommandText = "SELECT SlotCode FROM ParkingSlots WHERE ZoneId IN ('ZONE_E', 'ZONE_F') AND Status = 'Available' ORDER BY SlotCode ASC LIMIT 1;";
                    object? foundObj = findCarSlotCmd.ExecuteScalar();
                    if (foundObj != null && foundObj != DBNull.Value)
                    {
                        targetNewSlot = foundObj.ToString();
                    }
                }

                if (!string.IsNullOrEmpty(targetNewSlot))
                {
                    using (SqliteCommand occupyNewCmd = connection.CreateCommand())
                    {
                        occupyNewCmd.Transaction = transaction;
                        occupyNewCmd.CommandText = "UPDATE ParkingSlots SET Status = 'Occupied', CurrentLicensePlate = @plate WHERE SlotCode = @newSlot;";
                        occupyNewCmd.Parameters.AddWithValue("@plate", licensePlate);
                        occupyNewCmd.Parameters.AddWithValue("@newSlot", targetNewSlot);
                        occupyNewCmd.ExecuteNonQuery();
                    }

                    using (SqliteCommand moveSessionCmd = connection.CreateCommand())
                    {
                        moveSessionCmd.Transaction = transaction;
                        moveSessionCmd.CommandText = "UPDATE ParkingSessions SET SlotCode = @newSlot WHERE SessionId = @sessionId;";
                        moveSessionCmd.Parameters.AddWithValue("@newSlot", targetNewSlot);
                        moveSessionCmd.Parameters.AddWithValue("@sessionId", sessionId);
                        moveSessionCmd.ExecuteNonQuery();
                    }

                    using (SqliteCommand freeOldCmd = connection.CreateCommand())
                    {
                        freeOldCmd.Transaction = transaction;
                        freeOldCmd.CommandText = "UPDATE ParkingSlots SET Status = 'Available', CurrentLicensePlate = NULL WHERE SlotCode = @oldSlot;";
                        freeOldCmd.Parameters.AddWithValue("@oldSlot", oldSlot);
                        freeOldCmd.ExecuteNonQuery();
                    }
                }
            }

            using (SqliteCommand cleanOrphanSlotCmd = connection.CreateCommand())
            {
                cleanOrphanSlotCmd.Transaction = transaction;
                cleanOrphanSlotCmd.CommandText = @"
                    UPDATE ParkingSlots
                    SET Status = 'Available',
                        CurrentLicensePlate = NULL
                    WHERE ZoneId = 'ZONE_B'
                      AND Status = 'Occupied'
                      AND NOT EXISTS (
                          SELECT 1 FROM ParkingSessions
                          WHERE ParkingSessions.SlotCode = ParkingSlots.SlotCode
                            AND ParkingSessions.Status = 'Parking'
                      );";
                cleanOrphanSlotCmd.ExecuteNonQuery();
            }

            using (SqliteCommand setFlagCmd = connection.CreateCommand())
            {
                setFlagCmd.Transaction = transaction;
                setFlagCmd.CommandText = "INSERT OR REPLACE INTO AppSettings (SettingKey, SettingValue) VALUES ('ParkingLotLayoutV2_96Slots', '1');";
                setFlagCmd.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    // Hàm khởi tạo cấu trúc CSDL và dữ liệu mẫu theo cơ chế bất đồng bộ cho tầng Service
    public static Task InitializeDatabaseAsync()
    {
        InitializeDatabase();
        return Task.CompletedTask;
    }

    // Hàm đọc câu lệnh SQL tạo cấu trúc 12 bảng từ file schema.sql (tách bỏ phần dữ liệu test mẫu) hoặc chuỗi SQL dự phòng
    private static string GetSchemaSql()
    {
        string schemaFilePath = Path.Combine(Path.GetDirectoryName(DatabaseFilePath) ?? string.Empty, "schema.sql");
        if (File.Exists(schemaFilePath))
        {
            string fullSchema = File.ReadAllText(schemaFilePath);
            int seedSectionIndex = fullSchema.IndexOf("-- PHẦN 2", StringComparison.OrdinalIgnoreCase);
            if (seedSectionIndex > 0)
            {
                return fullSchema.Substring(0, seedSectionIndex);
            }

            return fullSchema;
        }

        return @"
PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Users (
    UserId       INTEGER PRIMARY KEY AUTOINCREMENT,
    Username     TEXT    NOT NULL UNIQUE,
    PasswordHash TEXT    NOT NULL,
    FullName     TEXT    NOT NULL,
    Phone        TEXT    NOT NULL DEFAULT '',
    Email        TEXT    NOT NULL DEFAULT '',
    IdentityCard TEXT    NOT NULL DEFAULT '',
    EmployeeCode TEXT    NOT NULL DEFAULT '',
    IsActive     INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Customers (
    CustomerId   INTEGER PRIMARY KEY AUTOINCREMENT,
    FullName     TEXT    NOT NULL,
    Phone        TEXT    NOT NULL,
    IdentityCard TEXT    NOT NULL UNIQUE,
    CreatedAt    TEXT    NOT NULL
);

CREATE TABLE IF NOT EXISTS VehicleTypes (
    VehicleTypeId TEXT PRIMARY KEY,
    TypeName      TEXT NOT NULL,
    Description   TEXT NULL
);

CREATE TABLE IF NOT EXISTS PricingRules (
    RuleId           INTEGER PRIMARY KEY AUTOINCREMENT,
    VehicleTypeId    TEXT    NOT NULL UNIQUE,
    DayTurnPrice     NUMERIC NOT NULL DEFAULT 0,
    OvernightPrice   NUMERIC NOT NULL DEFAULT 0,
    HourlyPrice      NUMERIC NOT NULL DEFAULT 0,
    MonthlyPassPrice NUMERIC NOT NULL DEFAULT 0,
    LostCardPenalty  NUMERIC NOT NULL DEFAULT 0,
    UpdatedAt        TEXT    NOT NULL,
    FOREIGN KEY (VehicleTypeId) REFERENCES VehicleTypes(VehicleTypeId)
);

CREATE TABLE IF NOT EXISTS ParkingZones (
    ZoneId        TEXT    PRIMARY KEY,
    ZoneName      TEXT    NOT NULL,
    VehicleTypeId TEXT    NOT NULL,
    MaxCapacity   INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (VehicleTypeId) REFERENCES VehicleTypes(VehicleTypeId)
);

CREATE TABLE IF NOT EXISTS ParkingSlots (
    SlotCode            TEXT PRIMARY KEY,
    ZoneId              TEXT NOT NULL,
    Status              TEXT NOT NULL DEFAULT 'Available',
    CurrentLicensePlate TEXT NULL,
    FOREIGN KEY (ZoneId) REFERENCES ParkingZones(ZoneId)
);

CREATE TABLE IF NOT EXISTS ParkingCards (
    CardCode     TEXT PRIMARY KEY,
    LicensePlate TEXT NULL,
    Status       TEXT NOT NULL DEFAULT 'Available'
);

CREATE TABLE IF NOT EXISTS MonthlyPasses (
    PassId            INTEGER PRIMARY KEY AUTOINCREMENT,
    CustomerId        INTEGER NOT NULL,
    LicensePlate      TEXT    NOT NULL,
    VehicleTypeId     TEXT    NOT NULL,
    VehicleBrandColor TEXT    NULL,
    StartDate         TEXT    NOT NULL,
    EndDate           TEXT    NOT NULL,
    Status            TEXT    NOT NULL DEFAULT 'Active',
    FOREIGN KEY (CustomerId)    REFERENCES Customers(CustomerId),
    FOREIGN KEY (VehicleTypeId) REFERENCES VehicleTypes(VehicleTypeId)
);

CREATE TABLE IF NOT EXISTS WorkShifts (
    ShiftId          INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId           INTEGER NOT NULL,
    ShiftName        TEXT    NOT NULL,
    StartTime        TEXT    NOT NULL,
    EndTime          TEXT    NULL,
    TotalVehiclesIn  INTEGER NOT NULL DEFAULT 0,
    TotalVehiclesOut INTEGER NOT NULL DEFAULT 0,
    TotalRevenue     NUMERIC NOT NULL DEFAULT 0,
    Status           TEXT    NOT NULL DEFAULT 'Open',
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE TABLE IF NOT EXISTS ParkingSessions (
    SessionId         INTEGER PRIMARY KEY AUTOINCREMENT,
    CardCode          TEXT    NOT NULL,
    LicensePlate      TEXT    NOT NULL,
    VehicleTypeId     TEXT    NOT NULL,
    SlotCode          TEXT    NOT NULL,
    CheckInTime       TEXT    NOT NULL,
    CheckOutTime      TEXT    NULL,
    CheckInImagePath  TEXT    NULL,
    CheckOutImagePath TEXT    NULL,
    CheckInUserId     INTEGER NOT NULL,
    CheckOutUserId    INTEGER NULL,
    IsMonthlyPass     INTEGER NOT NULL DEFAULT 0,
    ParkingFee        NUMERIC NOT NULL DEFAULT 0,
    PenaltyFee        NUMERIC NOT NULL DEFAULT 0,
    TotalAmount       NUMERIC NOT NULL DEFAULT 0,
    Status            TEXT    NOT NULL DEFAULT 'Parking',
    FOREIGN KEY (CardCode)       REFERENCES ParkingCards(CardCode),
    FOREIGN KEY (VehicleTypeId)  REFERENCES VehicleTypes(VehicleTypeId),
    FOREIGN KEY (SlotCode)       REFERENCES ParkingSlots(SlotCode),
    FOREIGN KEY (CheckInUserId)  REFERENCES Users(UserId),
    FOREIGN KEY (CheckOutUserId) REFERENCES Users(UserId)
);

CREATE TABLE IF NOT EXISTS IncidentReports (
    IncidentId           INTEGER PRIMARY KEY AUTOINCREMENT,
    SessionId            INTEGER NULL,
    CardCode             TEXT    NOT NULL,
    LicensePlate         TEXT    NOT NULL,
    CustomerName         TEXT    NOT NULL,
    CustomerPhone        TEXT    NOT NULL,
    CustomerIdentityCard TEXT    NOT NULL,
    IncidentType         TEXT    NOT NULL DEFAULT 'LostCard',
    CompensationFee      NUMERIC NOT NULL DEFAULT 0,
    Note                 TEXT    NULL,
    HandledByUserId      INTEGER NOT NULL,
    CreatedAt            TEXT    NOT NULL,
    FOREIGN KEY (SessionId)       REFERENCES ParkingSessions(SessionId),
    FOREIGN KEY (CardCode)        REFERENCES ParkingCards(CardCode),
    FOREIGN KEY (HandledByUserId) REFERENCES Users(UserId)
);

CREATE TABLE IF NOT EXISTS PaymentTransactions (
    TransactionId     INTEGER PRIMARY KEY AUTOINCREMENT,
    TransactionType   TEXT    NOT NULL DEFAULT 'ParkingFee',
    ReferenceId       INTEGER NULL,
    Amount            NUMERIC NOT NULL DEFAULT 0,
    PaymentMethod     TEXT    NOT NULL DEFAULT 'Cash',
    CollectedByUserId INTEGER NOT NULL,
    ShiftId           INTEGER NULL,
    Note              TEXT    NULL,
    CreatedAt         TEXT    NOT NULL,
    FOREIGN KEY (CollectedByUserId) REFERENCES Users(UserId),
    FOREIGN KEY (ShiftId)           REFERENCES WorkShifts(ShiftId)
);";
    }
}

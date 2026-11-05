using Microsoft.Data.Sqlite;

namespace BAIDOXETHONGMINH.DAL.Database;

/// <summary>
/// Quản lý kết nối và khởi tạo cấu trúc 12 bảng cho CSDL SQLite (smart_parking.db)
/// </summary>
public static class SqliteDbContext
{
    /// <summary>
    /// Đường dẫn tới file vật lý DAL/Database/smart_parking.db
    /// </summary>
    public static string DatabaseFilePath { get; } = ResolveDatabasePath();

    public static string ConnectionString => $"Data Source={DatabaseFilePath}";

    private static string ResolveDatabasePath()
    {
        // Ưu tiên trỏ trực tiếp tới file DAL/Database/smart_parking.db trong thư mục Project khi chạy Debug/Release
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

        // Dự phòng nếu chạy bản đóng gói độc lập
        string fallbackDir = Path.Combine(baseDir, "DAL", "Database");
        Directory.CreateDirectory(fallbackDir);
        return Path.Combine(fallbackDir, "smart_parking.db");
    }

    /// <summary>
    /// Mở kết nối SQLite đã bật kiểm tra khóa ngoại (PRAGMA foreign_keys = ON)
    /// </summary>
    public static SqliteConnection GetConnection()
    {
        SqliteConnection connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using SqliteCommand pragmaCmd = connection.CreateCommand();
        pragmaCmd.CommandText = "PRAGMA foreign_keys = ON;";
        pragmaCmd.ExecuteNonQuery();

        return connection;
    }

    /// <summary>
    /// Đảm bảo file smart_parking.db có đủ cấu trúc 12 bảng (không tự nạp dữ liệu mẫu)
    /// </summary>
    public static void InitializeDatabase()
    {
        using SqliteConnection connection = GetConnection();
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = GetSchemaSql();
        cmd.ExecuteNonQuery();
    }

    private static string GetSchemaSql()
    {
        string schemaFilePath = Path.Combine(Path.GetDirectoryName(DatabaseFilePath) ?? string.Empty, "schema.sql");
        if (File.Exists(schemaFilePath))
        {
            return File.ReadAllText(schemaFilePath);
        }

        return @"
PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Users (
    UserId       INTEGER PRIMARY KEY AUTOINCREMENT,
    Username     TEXT    NOT NULL UNIQUE,
    PasswordHash TEXT    NOT NULL,
    FullName     TEXT    NOT NULL,
    Phone        TEXT    NOT NULL DEFAULT '',
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

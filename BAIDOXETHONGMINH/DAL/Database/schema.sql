-- ============================================================
-- CẤU TRÚC CƠ SỞ DỮ LIỆU SQLITE - BÃI ĐỖ XE THÔNG MINH (12 BẢNG)
-- Chỉ định nghĩa cấu trúc bảng (Schema), không nạp dữ liệu mẫu
-- ============================================================

PRAGMA foreign_keys = ON;

-- 1. Bảng Users: Tài khoản người quản lý / nhân viên trực ca
CREATE TABLE IF NOT EXISTS Users (
    UserId       INTEGER PRIMARY KEY AUTOINCREMENT,
    Username     TEXT    NOT NULL UNIQUE,
    PasswordHash TEXT    NOT NULL,
    FullName     TEXT    NOT NULL,
    Phone        TEXT    NOT NULL DEFAULT '',
    IsActive     INTEGER NOT NULL DEFAULT 1
);

-- 2. Bảng Customers: Hồ sơ khách hàng / chủ phương tiện
CREATE TABLE IF NOT EXISTS Customers (
    CustomerId   INTEGER PRIMARY KEY AUTOINCREMENT,
    FullName     TEXT    NOT NULL,
    Phone        TEXT    NOT NULL,
    IdentityCard TEXT    NOT NULL UNIQUE,
    CreatedAt    TEXT    NOT NULL
);

-- 3. Bảng VehicleTypes: Danh mục loại phương tiện (XEMAY, OTO...)
CREATE TABLE IF NOT EXISTS VehicleTypes (
    VehicleTypeId TEXT PRIMARY KEY,
    TypeName      TEXT NOT NULL,
    Description   TEXT NULL
);

-- 4. Bảng PricingRules: Cấu hình bảng giá đỗ xe theo loại phương tiện
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

-- 5. Bảng ParkingZones: Khu vực đỗ xe trong bãi (Khu A, Khu B...)
CREATE TABLE IF NOT EXISTS ParkingZones (
    ZoneId        TEXT    PRIMARY KEY,
    ZoneName      TEXT    NOT NULL,
    VehicleTypeId TEXT    NOT NULL,
    MaxCapacity   INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (VehicleTypeId) REFERENCES VehicleTypes(VehicleTypeId)
);

-- 6. Bảng ParkingSlots: Từng vị trí (ô đỗ) trên sơ đồ bãi xe
CREATE TABLE IF NOT EXISTS ParkingSlots (
    SlotCode            TEXT PRIMARY KEY,
    ZoneId              TEXT NOT NULL,
    Status              TEXT NOT NULL DEFAULT 'Available',
    CurrentLicensePlate TEXT NULL,
    FOREIGN KEY (ZoneId) REFERENCES ParkingZones(ZoneId)
);

-- 7. Bảng ParkingCards: Danh mục thẻ/vé xe
CREATE TABLE IF NOT EXISTS ParkingCards (
    CardCode     TEXT PRIMARY KEY,
    LicensePlate TEXT NULL,
    Status       TEXT NOT NULL DEFAULT 'Available'
);

-- 8. Bảng MonthlyPasses: Danh sách vé tháng đã đăng ký
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

-- 9. Bảng WorkShifts: Ca trực của nhân viên
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

-- 10. Bảng ParkingSessions: Lượt xe vào / ra bãi
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

-- 11. Bảng IncidentReports: Biên bản sự cố / mất thẻ xe
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

-- 12. Bảng PaymentTransactions: Lịch sử giao dịch thu tiền
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
);

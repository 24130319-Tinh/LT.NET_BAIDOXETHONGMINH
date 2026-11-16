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

-- ============================================================
-- PHẦN 2: DỮ LIỆU TEST MẪU (BẠN TỰ THỰC THI ĐỂ NẠP VÀO DB)
-- ============================================================

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
-- Trong đó A-01, B-01, B-02 đang có xe đỗ để test Làn Ra (Check-out)
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
-- Trong đó THE001, THE002, THE003 đang cấp cho 3 xe trong bãi
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

-- 8. Danh sách Vé tháng còn hạn (test biển số 51H-999.99 và 59P2-345.67)
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

-- 10. Ba lượt xe đang đỗ trong bãi (Status = 'Parking') để test ngay Làn Ra (Check-out):
--   • Session 1 (THE003 - 51H-999.99): Ô tô có Vé tháng hợp lệ -> Miễn phí gửi (0đ)
--   • Session 2 (THE002 - 51G-456.78): Ô tô Vé lượt gửi 3h05p -> Tính 4 giờ = 80.000đ
--   • Session 3 (THE001 - 59X3-112.23): Xe máy Vé lượt ban ngày -> 5.000đ
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
(3, 'THE001', '59X3-112.23', 'XEMAY', 'A-01', datetime('now', 'localtime', '-135 minutes'), NULL, NULL, NULL, 1, NULL, 0, 0, 0, 0, 'Parking');


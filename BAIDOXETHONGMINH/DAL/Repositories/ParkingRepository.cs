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
    ///
    // ham dung de xet xem vehicletype co dulieu trong khong? Neu trong thi nap vao du lieu mau, khác rỗng thì return
    public void SeedBaseCatalogIfEmpty()
    {
        using SqliteConnection connection = SqliteDbContext.GetConnection();

        using SqliteCommand checkCmd = connection.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM VehicleTypes;";
        object? scalarResult = checkCmd.ExecuteScalar();
        long existingCount = scalarResult != null ? Convert.ToInt64(scalarResult) : 0;

        if (existingCount > 0)
        {
            using SqliteCommand syncPenaltyCmd = connection.CreateCommand();
            syncPenaltyCmd.CommandText = "UPDATE PricingRules SET LostCardPenalty = 50000 WHERE LostCardPenalty <> 50000;";
            syncPenaltyCmd.ExecuteNonQuery();
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
                   ('OTO', 30000, 60000, 20000, 1200000, 50000, '{nowText}');"
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

    
    /// Kiểm tra xem biển số xe này có đang gửi trong bãi chưa lấy ra (Status = 'Parking') hay không
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

    /// <summary>
    /// Lưu lượt xe vào bãi trong 1 Transaction đồng bộ 3 bảng:
    /// 1. INSERT vào ParkingSessions (Status = 'Parking')
    /// 2. UPDATE ParkingCards sang Status = 'InUse'
    /// 3. UPDATE ParkingSlots sang Status = 'Occupied'
    /// Trả về true nếu cả 3 bước thành công, ngược lại Rollback và trả về false.
    /// </summary>
    public bool InsertCheckInSession(ParkingSession session)
    {
        using SqliteConnection connection = SqliteDbContext.GetConnection();
        using SqliteTransaction transaction = connection.BeginTransaction();

        try
        {
            // Bước 1: Thêm dòng mới vào bảng ParkingSessions
            using SqliteCommand insertSessionCmd = connection.CreateCommand();
            insertSessionCmd.Transaction = transaction;
            insertSessionCmd.CommandText = @"
                INSERT INTO ParkingSessions
                (
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
                (
                    @CardCode,
                    @LicensePlate,
                    @VehicleTypeId,
                    @SlotCode,
                    @CheckInTime,
                    NULL,
                    @CheckInImagePath,
                    NULL,
                    @CheckInUserId,
                    NULL,
                    @IsMonthlyPass,
                    0,
                    0,
                    0,
                    'Parking'
                );";

            insertSessionCmd.Parameters.AddWithValue("@CardCode", session.CardCode);
            insertSessionCmd.Parameters.AddWithValue("@LicensePlate", session.LicensePlate);
            insertSessionCmd.Parameters.AddWithValue("@VehicleTypeId", session.VehicleTypeId);
            insertSessionCmd.Parameters.AddWithValue("@SlotCode", session.SlotCode);
            insertSessionCmd.Parameters.AddWithValue("@CheckInTime", session.CheckInTime.ToString("yyyy-MM-dd HH:mm:ss"));
            insertSessionCmd.Parameters.AddWithValue("@CheckInImagePath", (object?)session.CheckInImagePath ?? DBNull.Value);
            insertSessionCmd.Parameters.AddWithValue("@CheckInUserId", session.CheckInUserId <= 0 ? 1 : session.CheckInUserId);
            insertSessionCmd.Parameters.AddWithValue("@IsMonthlyPass", session.IsMonthlyPass ? 1 : 0);

            int insertedRows = insertSessionCmd.ExecuteNonQuery();

            // Bước 2: Cập nhật trạng thái thẻ xe sang 'InUse'
            using SqliteCommand updateCardCmd = connection.CreateCommand();
            updateCardCmd.Transaction = transaction;
            updateCardCmd.CommandText = @"
                UPDATE ParkingCards
                SET Status = 'InUse',
                    LicensePlate = @LicensePlate
                WHERE CardCode = @CardCode
                  AND Status = 'Available';";
            updateCardCmd.Parameters.AddWithValue("@LicensePlate", session.LicensePlate);
            updateCardCmd.Parameters.AddWithValue("@CardCode", session.CardCode);

            int updatedCards = updateCardCmd.ExecuteNonQuery();

            // Bước 3: Cập nhật trạng thái ô đỗ sang 'Occupied'
            using SqliteCommand updateSlotCmd = connection.CreateCommand();
            updateSlotCmd.Transaction = transaction;
            updateSlotCmd.CommandText = @"
                UPDATE ParkingSlots
                SET Status = 'Occupied',
                    CurrentLicensePlate = @LicensePlate
                WHERE SlotCode = @SlotCode
                  AND Status = 'Available';";
            updateSlotCmd.Parameters.AddWithValue("@LicensePlate", session.LicensePlate);
            updateSlotCmd.Parameters.AddWithValue("@SlotCode", session.SlotCode);

            int updatedSlots = updateSlotCmd.ExecuteNonQuery();

            if (insertedRows == 1 && updatedCards == 1 && updatedSlots == 1)
            {
                transaction.Commit();
                return true;
            }

            transaction.Rollback();
            return false;
        }
        catch (Exception)
        {
            transaction.Rollback();
            return false;   
        }
    }

    /// <summary>
    /// Lấy cấu hình bảng giá theo mã loại phương tiện (XEMAY, OTO) từ bảng PricingRules
    /// </summary>
    public PricingRule? GetPricingRuleByVehicleType(string vehicleTypeId)
    {
        if (string.IsNullOrWhiteSpace(vehicleTypeId))
        {
            return null;
        }

        using SqliteConnection connection = SqliteDbContext.GetConnection();
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT RuleId, VehicleTypeId, DayTurnPrice, OvernightPrice, HourlyPrice, MonthlyPassPrice, LostCardPenalty, UpdatedAt
            FROM PricingRules
            WHERE UPPER(VehicleTypeId) = UPPER(@VehicleTypeId)
            LIMIT 1;";
        cmd.Parameters.AddWithValue("@VehicleTypeId", vehicleTypeId.Trim());

        using SqliteDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            PricingRule rule = new PricingRule
            {
                RuleId = reader.GetInt32(0),
                VehicleTypeId = reader.GetString(1),
                DayTurnPrice = reader.GetDecimal(2),
                OvernightPrice = reader.GetDecimal(3),
                HourlyPrice = reader.GetDecimal(4),
                MonthlyPassPrice = reader.GetDecimal(5),
                LostCardPenalty = reader.GetDecimal(6),
                UpdatedAt = reader.IsDBNull(7) ? DateTime.Now : ParseSqliteDateTime(reader.GetString(7))
            };
            return rule;
        }

        return null;
    }

    /// <summary>
    /// Lấy danh sách toàn bộ lượt xe đang gửi trong bãi (Status = 'Parking') để đổ lên thanh tìm kiếm Làn Ra
    /// </summary>
    public List<ParkingSession> GetAllActiveParkingSessions()
    {
        List<ParkingSession> sessions = new List<ParkingSession>();

        using SqliteConnection connection = SqliteDbContext.GetConnection();
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT
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
            FROM ParkingSessions
            WHERE Status = 'Parking'
            ORDER BY CheckInTime DESC, SessionId DESC;";

        using SqliteDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            ParkingSession session = MapParkingSession(reader);
            sessions.Add(session);
        }

        return sessions;
    }

    /// <summary>
    /// Tìm chính xác lượt gửi đang hoạt động (Status = 'Parking') theo Mã thẻ hoặc Biển số xe
    /// </summary>
    public ParkingSession? GetActiveSessionByCardOrPlate(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return null;
        }

        string rawKeyword = keyword.Trim().ToUpperInvariant();
        string normalizedKeyword = NormalizePlate(keyword);

        using SqliteConnection connection = SqliteDbContext.GetConnection();
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT
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
            FROM ParkingSessions
            WHERE Status = 'Parking'
              AND (
                  UPPER(CardCode) = @RawKeyword
                  OR UPPER(REPLACE(REPLACE(LicensePlate, ' ', ''), '-', '')) = @NormalizedKeyword
              )
            ORDER BY CheckInTime DESC, SessionId DESC
            LIMIT 1;";

        cmd.Parameters.AddWithValue("@RawKeyword", rawKeyword);
        cmd.Parameters.AddWithValue("@NormalizedKeyword", normalizedKeyword);

        using SqliteDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return MapParkingSession(reader);
        }

        return null;
    }

    /// <summary>
    /// Hoàn tất thủ tục Xe Ra Bãi (Nghiệp vụ 2 - Check-out) trong 1 Transaction:
    /// 1. UPDATE ParkingSessions sang 'Completed' cùng số tiền thu
    /// 2. Giải phóng ô đỗ về 'Available'
    /// 3. Giải phóng thẻ về 'Available' (hoặc khóa 'Lost' + lưu IncidentReports nếu mất thẻ)
    /// 4. Lưu hóa đơn thu tiền vào PaymentTransactions và cập nhật doanh thu ca trực (nếu có ca đang mở)
    /// </summary>
    public bool CompleteCheckOutSession(
        ParkingSession completedSession,
        bool isLostCard,
        IncidentReport? incidentReport,
        int checkOutUserId = 1)
    {
        using SqliteConnection connection = SqliteDbContext.GetConnection();
        using SqliteTransaction transaction = connection.BeginTransaction();

        try
        {
            int safeUserId = checkOutUserId <= 0 ? 1 : checkOutUserId;
            DateTime checkOutTime = completedSession.CheckOutTime ?? DateTime.Now;
            string checkOutTimeText = checkOutTime.ToString("yyyy-MM-dd HH:mm:ss");

            // Bước 1: Cập nhật lượt gửi trong ParkingSessions sang 'Completed'
            using SqliteCommand updateSessionCmd = connection.CreateCommand();
            updateSessionCmd.Transaction = transaction;
            updateSessionCmd.CommandText = @"
                UPDATE ParkingSessions
                SET CheckOutTime = @CheckOutTime,
                    CheckOutImagePath = @CheckOutImagePath,
                    CheckOutUserId = @CheckOutUserId,
                    IsMonthlyPass = @IsMonthlyPass,
                    ParkingFee = @ParkingFee,
                    PenaltyFee = @PenaltyFee,
                    TotalAmount = @TotalAmount,
                    Status = 'Completed'
                WHERE SessionId = @SessionId
                  AND Status = 'Parking';";

            updateSessionCmd.Parameters.AddWithValue("@CheckOutTime", checkOutTimeText);
            updateSessionCmd.Parameters.AddWithValue("@CheckOutImagePath", (object?)completedSession.CheckOutImagePath ?? DBNull.Value);
            updateSessionCmd.Parameters.AddWithValue("@CheckOutUserId", safeUserId);
            updateSessionCmd.Parameters.AddWithValue("@IsMonthlyPass", completedSession.IsMonthlyPass ? 1 : 0);
            updateSessionCmd.Parameters.AddWithValue("@ParkingFee", completedSession.ParkingFee);
            updateSessionCmd.Parameters.AddWithValue("@PenaltyFee", completedSession.PenaltyFee);
            updateSessionCmd.Parameters.AddWithValue("@TotalAmount", completedSession.TotalAmount);
            updateSessionCmd.Parameters.AddWithValue("@SessionId", completedSession.SessionId);

            int updatedSessionRows = updateSessionCmd.ExecuteNonQuery();
            if (updatedSessionRows != 1)
            {
                transaction.Rollback();
                return false;
            }

            // Bước 2: Giải phóng Ô đỗ về 'Available'
            using SqliteCommand releaseSlotCmd = connection.CreateCommand();
            releaseSlotCmd.Transaction = transaction;
            releaseSlotCmd.CommandText = @"
                UPDATE ParkingSlots
                SET Status = 'Available',
                    CurrentLicensePlate = NULL
                WHERE SlotCode = @SlotCode;";
            releaseSlotCmd.Parameters.AddWithValue("@SlotCode", completedSession.SlotCode);
            releaseSlotCmd.ExecuteNonQuery();

            // Bước 3: Giải phóng Thẻ xe hoặc khóa thẻ bị mất + lưu biên bản sự cố
            string targetCardStatus = isLostCard ? "Lost" : "Available";
            using SqliteCommand updateCardCmd = connection.CreateCommand();
            updateCardCmd.Transaction = transaction;
            updateCardCmd.CommandText = @"
                UPDATE ParkingCards
                SET Status = @TargetStatus,
                    LicensePlate = NULL
                WHERE CardCode = @CardCode;";
            updateCardCmd.Parameters.AddWithValue("@TargetStatus", targetCardStatus);
            updateCardCmd.Parameters.AddWithValue("@CardCode", completedSession.CardCode);
            updateCardCmd.ExecuteNonQuery();

            if (isLostCard && incidentReport != null)
            {
                using SqliteCommand insertIncidentCmd = connection.CreateCommand();
                insertIncidentCmd.Transaction = transaction;
                insertIncidentCmd.CommandText = @"
                    INSERT INTO IncidentReports
                    (
                        SessionId,
                        CardCode,
                        LicensePlate,
                        CustomerName,
                        CustomerPhone,
                        CustomerIdentityCard,
                        IncidentType,
                        CompensationFee,
                        Note,
                        HandledByUserId,
                        CreatedAt
                    )
                    VALUES
                    (
                        @SessionId,
                        @CardCode,
                        @LicensePlate,
                        @CustomerName,
                        @CustomerPhone,
                        @CustomerIdentityCard,
                        @IncidentType,
                        @CompensationFee,
                        @Note,
                        @HandledByUserId,
                        @CreatedAt
                    );";

                insertIncidentCmd.Parameters.AddWithValue("@SessionId", completedSession.SessionId);
                insertIncidentCmd.Parameters.AddWithValue("@CardCode", completedSession.CardCode);
                insertIncidentCmd.Parameters.AddWithValue("@LicensePlate", completedSession.LicensePlate);
                insertIncidentCmd.Parameters.AddWithValue("@CustomerName", incidentReport.CustomerName);
                insertIncidentCmd.Parameters.AddWithValue("@CustomerPhone", incidentReport.CustomerPhone);
                insertIncidentCmd.Parameters.AddWithValue("@CustomerIdentityCard", incidentReport.CustomerIdentityCard);
                insertIncidentCmd.Parameters.AddWithValue("@IncidentType", string.IsNullOrWhiteSpace(incidentReport.IncidentType) ? "LostCard" : incidentReport.IncidentType);
                insertIncidentCmd.Parameters.AddWithValue("@CompensationFee", incidentReport.CompensationFee);
                insertIncidentCmd.Parameters.AddWithValue("@Note", (object?)incidentReport.Note ?? DBNull.Value);
                insertIncidentCmd.Parameters.AddWithValue("@HandledByUserId", safeUserId);
                insertIncidentCmd.Parameters.AddWithValue("@CreatedAt", checkOutTimeText);
                insertIncidentCmd.ExecuteNonQuery();
            }

            // Bước 4: Ghi nhận hóa đơn thu tiền vào PaymentTransactions & cập nhật ca trực (nếu có)
            int? openShiftId = GetOpenShiftId(connection, transaction);

            if (completedSession.ParkingFee > 0m)
            {
                InsertPaymentTransaction(
                    connection,
                    transaction,
                    "ParkingFee",
                    completedSession.SessionId,
                    completedSession.ParkingFee,
                    safeUserId,
                    openShiftId,
                    $"Thu phí gửi xe {completedSession.LicensePlate} (Thẻ {completedSession.CardCode})",
                    checkOutTimeText
                );
            }

            if (completedSession.PenaltyFee > 0m)
            {
                InsertPaymentTransaction(
                    connection,
                    transaction,
                    "LostCardPenalty",
                    completedSession.SessionId,
                    completedSession.PenaltyFee,
                    safeUserId,
                    openShiftId,
                    $"Phạt mất thẻ {completedSession.CardCode} - BS: {completedSession.LicensePlate}",
                    checkOutTimeText
                );
            }

            if (openShiftId.HasValue)
            {
                using SqliteCommand updateShiftCmd = connection.CreateCommand();
                updateShiftCmd.Transaction = transaction;
                updateShiftCmd.CommandText = @"
                    UPDATE WorkShifts
                    SET TotalVehiclesOut = TotalVehiclesOut + 1,
                        TotalRevenue = TotalRevenue + @TotalAmount
                    WHERE ShiftId = @ShiftId;";
                updateShiftCmd.Parameters.AddWithValue("@TotalAmount", completedSession.TotalAmount);
                updateShiftCmd.Parameters.AddWithValue("@ShiftId", openShiftId.Value);
                updateShiftCmd.ExecuteNonQuery();
            }

            transaction.Commit();
            return true;
        }
        catch (Exception)
        {
            transaction.Rollback();
            return false;
        }
    }

    private static ParkingSession MapParkingSession(SqliteDataReader reader)
    {
        ParkingSession session = new ParkingSession
        {
            SessionId = reader.GetInt32(0),
            CardCode = reader.GetString(1),
            LicensePlate = reader.GetString(2),
            VehicleTypeId = reader.GetString(3),
            SlotCode = reader.GetString(4),
            CheckInTime = ParseSqliteDateTime(reader.GetString(5)),
            CheckOutTime = reader.IsDBNull(6) ? null : ParseSqliteDateTime(reader.GetString(6)),
            CheckInImagePath = reader.IsDBNull(7) ? null : reader.GetString(7),
            CheckOutImagePath = reader.IsDBNull(8) ? null : reader.GetString(8),
            CheckInUserId = reader.GetInt32(9),
            CheckOutUserId = reader.IsDBNull(10) ? null : reader.GetInt32(10),
            IsMonthlyPass = reader.GetInt32(11) == 1,
            ParkingFee = reader.GetDecimal(12),
            PenaltyFee = reader.GetDecimal(13),
            TotalAmount = reader.GetDecimal(14),
            Status = reader.GetString(15)
        };
        return session;
    }

    private static int? GetOpenShiftId(SqliteConnection connection, SqliteTransaction transaction)
    {
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = @"
            SELECT ShiftId
            FROM WorkShifts
            WHERE Status = 'Open'
            ORDER BY ShiftId DESC
            LIMIT 1;";

        object? scalar = cmd.ExecuteScalar();
        if (scalar == null || scalar == DBNull.Value)
        {
            return null;
        }

        return Convert.ToInt32(scalar);
    }

    private static void InsertPaymentTransaction(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string transactionType,
        int referenceId,
        decimal amount,
        int collectedByUserId,
        int? shiftId,
        string note,
        string createdAtText)
    {
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = @"
            INSERT INTO PaymentTransactions
            (
                TransactionType,
                ReferenceId,
                Amount,
                PaymentMethod,
                CollectedByUserId,
                ShiftId,
                Note,
                CreatedAt
            )
            VALUES
            (
                @TransactionType,
                @ReferenceId,
                @Amount,
                'Cash',
                @CollectedByUserId,
                @ShiftId,
                @Note,
                @CreatedAt
            );";

        cmd.Parameters.AddWithValue("@TransactionType", transactionType);
        cmd.Parameters.AddWithValue("@ReferenceId", referenceId);
        cmd.Parameters.AddWithValue("@Amount", amount);
        cmd.Parameters.AddWithValue("@CollectedByUserId", collectedByUserId);
        cmd.Parameters.AddWithValue("@ShiftId", shiftId.HasValue ? (object)shiftId.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@Note", note);
        cmd.Parameters.AddWithValue("@CreatedAt", createdAtText);
        cmd.ExecuteNonQuery();
    }

    //Ham lam cho bien số xe nhất quán và đẹp mắt hơn
    private static string NormalizePlate(string rawPlate)
    {
        if (string.IsNullOrWhiteSpace(rawPlate))
        {
            return string.Empty;
        }

        return rawPlate.Trim().Replace(" ", string.Empty).Replace("-", string.Empty).ToUpperInvariant();
    }

    //Ham chuyen doi string khi lay tu sqlite sang DateTime và kiểm tra null
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

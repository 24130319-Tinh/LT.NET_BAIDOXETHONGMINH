using System.Globalization;
using BAIDOXETHONGMINH.DAL.Database;
using BAIDOXETHONGMINH.DTO.Entities;
using BAIDOXETHONGMINH.GUI;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;

namespace BAIDOXETHONGMINH.DAL.Repositories;

public class ParkingRepository
{
    // Hàm kiểm tra bảng VehicleTypes có rỗng không, nếu rỗng thì nạp danh mục mẫu, nếu đã có thì đồng bộ mức phạt 50.000đ
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

            ExecuteNonQuery(
                connection,
                transaction,
                @"INSERT OR IGNORE INTO Users (Username, PasswordHash, FullName, Phone, IsActive)
                  VALUES ('admin', '123456', 'Admin Tính', '0901234567', 1);"
            );

            ExecuteNonQuery(
                connection,
                transaction,
                @"INSERT OR IGNORE INTO VehicleTypes (VehicleTypeId, TypeName, Description) VALUES
                  ('XEMAY', 'Xe máy', 'Xe máy 2 bánh / Xe tay ga'),
                  ('OTO', 'Ô tô', 'Ô tô con 4 - 7 chỗ');"
            );

            ExecuteNonQuery(
                connection,
                transaction,
                $@"INSERT OR IGNORE INTO PricingRules
                   (VehicleTypeId, DayTurnPrice, OvernightPrice, HourlyPrice, MonthlyPassPrice, LostCardPenalty, UpdatedAt)
                   VALUES
                   ('XEMAY', 5000, 10000, 3000, 150000, 50000, '{nowText}'),
                   ('OTO', 30000, 60000, 20000, 1200000, 50000, '{nowText}');"
            );

            ExecuteNonQuery(
                connection,
                transaction,
                @"INSERT OR IGNORE INTO ParkingZones (ZoneId, ZoneName, VehicleTypeId, MaxCapacity) VALUES
                  ('ZONE_A', 'Khu A - Xe máy', 'XEMAY', 20),
                  ('ZONE_B', 'Khu B - Xe máy', 'XEMAY', 20),
                  ('ZONE_C', 'Khu C - Xe máy', 'XEMAY', 20),
                  ('ZONE_D', 'Khu D - Xe máy', 'XEMAY', 20),
                  ('ZONE_E', 'Khu E - Ô tô', 'OTO', 8),
                  ('ZONE_F', 'Khu F - Ô tô', 'OTO', 8);"
            );

            SeedSlotsForZone(connection, transaction, "ZONE_A", "A", 20);
            SeedSlotsForZone(connection, transaction, "ZONE_B", "B", 20);
            SeedSlotsForZone(connection, transaction, "ZONE_C", "C", 20);
            SeedSlotsForZone(connection, transaction, "ZONE_D", "D", 20);
            SeedSlotsForZone(connection, transaction, "ZONE_E", "E", 8);
            SeedSlotsForZone(connection, transaction, "ZONE_F", "F", 8);

            for (int i = 1; i <= 96; i++)
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

    // Hàm thêm nhanh các ô đỗ mặc định cho một khu vực trong cùng Transaction
    private static void SeedSlotsForZone(SqliteConnection connection, SqliteTransaction transaction, string zoneId, string prefix, int capacity)
    {
        for (int i = 1; i <= capacity; i++)
        {
            string slotCode = $"{prefix}-{i:D2}";
            ExecuteNonQuery(
                connection,
                transaction,
                $"INSERT OR IGNORE INTO ParkingSlots (SlotCode, ZoneId, Status, CurrentLicensePlate) VALUES ('{slotCode}', '{zoneId}', 'Available', NULL);"
            );
        }
    }

    // Hàm lấy danh sách tất cả các loại phương tiện trong cơ sở dữ liệu
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

    // Hàm lấy danh sách các thẻ xe đang ở trạng thái sẵn sàng cấp phát (Available)
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

    // Hàm lấy danh sách các ô đỗ còn trống tương ứng với mã loại phương tiện
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

    // Hàm đếm số lượng ô đỗ còn trống trên tổng số ô đỗ của toàn bãi xe
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

    // Hàm kiểm tra biển số xe hiện có đang gửi trong bãi chưa lấy ra hay không
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
            WHERE UPPER(REPLACE(REPLACE(REPLACE(LicensePlate, ' ', ''), '-', ''), '.', '')) = @NormalizedPlate
              AND Status = 'Parking';";
        cmd.Parameters.AddWithValue("@NormalizedPlate", normalizedPlate);

        object? scalar = cmd.ExecuteScalar();
        long count = scalar != null ? Convert.ToInt64(scalar) : 0;
        return count > 0;
    }

    // Hàm tìm kiếm Vé tháng đang hoạt động và còn thời hạn theo biển số xe
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
            WHERE UPPER(REPLACE(REPLACE(REPLACE(LicensePlate, ' ', ''), '-', ''), '.', '')) = @NormalizedPlate
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

    // Hàm lưu lượt xe vào bãi và đồng bộ trạng thái thẻ xe, ô đỗ, ca trực trong một Transaction
    public bool InsertCheckInSession(ParkingSession session)
    {
        using SqliteConnection connection = SqliteDbContext.GetConnection();
        using SqliteTransaction transaction = connection.BeginTransaction();

        try
        {
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
                int? openShiftId = GetOpenShiftId(connection, transaction);
                if (openShiftId.HasValue)
                {
                    using SqliteCommand updateShiftCmd = connection.CreateCommand();
                    updateShiftCmd.Transaction = transaction;
                    updateShiftCmd.CommandText = @"
                        UPDATE WorkShifts
                        SET TotalVehiclesIn = TotalVehiclesIn + 1
                        WHERE ShiftId = @ShiftId;";
                    updateShiftCmd.Parameters.AddWithValue("@ShiftId", openShiftId.Value);
                    updateShiftCmd.ExecuteNonQuery();
                }

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

    // Hàm lấy cấu hình bảng giá đỗ xe theo mã loại phương tiện
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

    // Hàm lấy danh sách toàn bộ lượt xe đang gửi trong bãi (Status = 'Parking')
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

    // Hàm tìm lượt gửi xe đang hoạt động theo Mã thẻ hoặc Biển số xe
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
                  OR UPPER(REPLACE(REPLACE(REPLACE(LicensePlate, ' ', ''), '-', ''), '.', '')) = @NormalizedKeyword
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

    // Hàm hoàn tất thủ tục cho xe ra bãi, ghi nhận thanh toán, biên bản mất thẻ và giải phóng chỗ đỗ trong một Transaction
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

            using SqliteCommand releaseSlotCmd = connection.CreateCommand();
            releaseSlotCmd.Transaction = transaction;
            releaseSlotCmd.CommandText = @"
                UPDATE ParkingSlots
                SET Status = 'Available',
                    CurrentLicensePlate = NULL
                WHERE SlotCode = @SlotCode;";
            releaseSlotCmd.Parameters.AddWithValue("@SlotCode", completedSession.SlotCode);
            releaseSlotCmd.ExecuteNonQuery();

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

    // Hàm ánh xạ một dòng dữ liệu từ SqliteDataReader sang đối tượng ParkingSession
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

    // Hàm lấy mã ca trực đang mở (Status = 'Open') hiện tại trong bảng WorkShifts
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

    // Hàm thêm một bản ghi giao dịch thu tiền vào bảng PaymentTransactions
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

    // Hàm làm cho biển số xe nhất quán (bỏ khoảng trắng, dấu gạch ngang, dấu chấm và viết hoa)
    private static string NormalizePlate(string rawPlate)
    {
        if (string.IsNullOrWhiteSpace(rawPlate))
        {
            return string.Empty;
        }

        return rawPlate.Trim().Replace(" ", string.Empty).Replace("-", string.Empty).Replace(".", string.Empty).ToUpperInvariant();
    }

    // Hàm chuyển đổi chuỗi ngày giờ lấy từ SQLite sang kiểu DateTime
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

    // Hàm thực thi nhanh một câu lệnh SQL không trả về kết quả trong cùng Transaction
    private static void ExecuteNonQuery(SqliteConnection connection, SqliteTransaction transaction, string sql)
    {
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    // Hàm ánh xạ một dòng dữ liệu SqliteDataReader sang đối tượng VehicleType
    private static VehicleType MapType(SqliteDataReader r)
    {
        return new VehicleType
        {
            VehicleTypeId = r.GetString(0),
            TypeName = r.GetString(1),
            Description = SqliteHelper.GetNullableString(r, 2)
        };
    }

    // Hàm ánh xạ một dòng dữ liệu SqliteDataReader sang đối tượng PricingRule
    private static PricingRule MapRule(SqliteDataReader r)
    {
        return new PricingRule
        {
            RuleId = r.GetInt32(0),
            VehicleTypeId = r.GetString(1),
            DayTurnPrice = r.GetDecimal(2),
            OvernightPrice = r.GetDecimal(3),
            HourlyPrice = r.GetDecimal(4),
            MonthlyPassPrice = r.GetDecimal(5),
            LostCardPenalty = r.GetDecimal(6),
            UpdatedAt = r.IsDBNull(7) ? DateTime.Now : ParseSqliteDateTime(r.GetString(7))
        };
    }

    // Hàm lấy bất đồng bộ danh sách tất cả loại phương tiện
    public Task<List<VehicleType>> GetVehicleTypesAsync()
    {
        return SqliteHelper.QueryAsync(
            "SELECT VehicleTypeId, TypeName, Description FROM VehicleTypes ORDER BY VehicleTypeId",
            MapType
        );
    }

    // Hàm lấy bất đồng bộ danh sách cấu hình bảng giá của tất cả loại phương tiện
    public Task<List<PricingRule>> GetPricingRulesAsync()
    {
        return SqliteHelper.QueryAsync(
            "SELECT RuleId, VehicleTypeId, DayTurnPrice, OvernightPrice, HourlyPrice, MonthlyPassPrice, LostCardPenalty, UpdatedAt FROM PricingRules ORDER BY VehicleTypeId",
            MapRule
        );
    }

    // Hàm lấy bất đồng bộ cấu hình bảng giá theo mã loại phương tiện
    public Task<PricingRule?> GetPricingRuleAsync(string vehicleTypeId)
    {
        return SqliteHelper.QueryFirstAsync(
            "SELECT RuleId, VehicleTypeId, DayTurnPrice, OvernightPrice, HourlyPrice, MonthlyPassPrice, LostCardPenalty, UpdatedAt FROM PricingRules WHERE VehicleTypeId = @type",
            MapRule,
            ("@type", vehicleTypeId)
        );
    }

    // Hàm thêm mới hoặc cập nhật bất đồng bộ cấu hình bảng giá theo loại phương tiện
    public Task SavePricingRuleAsync(PricingRule rule)
    {
        return SqliteHelper.ExecuteAsync(
            @"INSERT INTO PricingRules (VehicleTypeId, DayTurnPrice, OvernightPrice, HourlyPrice, MonthlyPassPrice, LostCardPenalty, UpdatedAt)
              VALUES (@type, @day, @night, @hour, @month, @penalty, @updated)
              ON CONFLICT(VehicleTypeId) DO UPDATE SET
                  DayTurnPrice = excluded.DayTurnPrice,
                  OvernightPrice = excluded.OvernightPrice,
                  HourlyPrice = excluded.HourlyPrice,
                  MonthlyPassPrice = excluded.MonthlyPassPrice,
                  LostCardPenalty = excluded.LostCardPenalty,
                  UpdatedAt = excluded.UpdatedAt",
            ("@type", rule.VehicleTypeId),
            ("@day", rule.DayTurnPrice),
            ("@night", rule.OvernightPrice),
            ("@hour", rule.HourlyPrice),
            ("@month", rule.MonthlyPassPrice),
            ("@penalty", rule.LostCardPenalty),
            ("@updated", rule.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
        );
    }

    // Hàm ánh xạ một dòng dữ liệu SqliteDataReader sang đối tượng ParkingZone
    private static ParkingZone MapZone(SqliteDataReader r)
    {
        return new ParkingZone
        {
            ZoneId = r.GetString(0),
            ZoneName = r.GetString(1),
            VehicleTypeId = r.GetString(2),
            MaxCapacity = r.GetInt32(3)
        };
    }

    // Hàm lấy bất đồng bộ danh sách tất cả khu vực đỗ xe
    public Task<List<ParkingZone>> GetZonesAsync()
    {
        return SqliteHelper.QueryAsync(
            "SELECT ZoneId, ZoneName, VehicleTypeId, MaxCapacity FROM ParkingZones ORDER BY ZoneId",
            MapZone
        );
    }

    // Hàm thêm mới bất đồng bộ một khu vực đỗ xe vào CSDL
    public Task AddZoneAsync(ParkingZone zone)
    {
        return SqliteHelper.ExecuteAsync(
            "INSERT INTO ParkingZones (ZoneId, ZoneName, VehicleTypeId, MaxCapacity) VALUES (@id, @name, @type, @capacity)",
            ("@id", zone.ZoneId),
            ("@name", zone.ZoneName),
            ("@type", zone.VehicleTypeId),
            ("@capacity", zone.MaxCapacity)
        );
    }

    // Hàm xóa bất đồng bộ một khu vực đỗ xe theo mã khu
    public async Task DeleteZoneAsync(string zoneId)
    {
        try
        {
            await SqliteHelper.ExecuteAsync("DELETE FROM ParkingZones WHERE ZoneId = @id", ("@id", zoneId));
        }
        catch (SqliteException ex) when (SqliteHelper.IsConstraintError(ex))
        {
            throw new ServiceException("Khu đang được dùng ở dữ liệu khác, không xóa được.");
        }
    }

    // Hàm ánh xạ một dòng dữ liệu SqliteDataReader sang đối tượng ParkingSlot
    private static ParkingSlot MapSlot(SqliteDataReader r)
    {
        return new ParkingSlot
        {
            SlotCode = r.GetString(0),
            ZoneId = r.GetString(1),
            Status = r.GetString(2),
            CurrentLicensePlate = SqliteHelper.GetNullableString(r, 3)
        };
    }

    // Hàm lấy bất đồng bộ danh sách tất cả các ô đỗ xe
    public Task<List<ParkingSlot>> GetSlotsAsync()
    {
        return SqliteHelper.QueryAsync(
            "SELECT SlotCode, ZoneId, Status, CurrentLicensePlate FROM ParkingSlots ORDER BY SlotCode",
            MapSlot
        );
    }

    // Hàm lấy bất đồng bộ thông tin một ô đỗ xe theo mã ô
    public Task<ParkingSlot?> GetSlotAsync(string slotCode)
    {
        return SqliteHelper.QueryFirstAsync(
            "SELECT SlotCode, ZoneId, Status, CurrentLicensePlate FROM ParkingSlots WHERE SlotCode = @code",
            MapSlot,
            ("@code", slotCode)
        );
    }

    // Hàm thêm bất đồng bộ danh sách các ô đỗ mới trong một Transaction
    public Task AddSlotsAsync(List<ParkingSlot> slots)
    {
        return SqliteHelper.InTransactionAsync(async (connection, tx) =>
        {
            foreach (ParkingSlot slot in slots)
            {
                await SqliteHelper.ExecuteAsync(
                    connection,
                    tx,
                    "INSERT INTO ParkingSlots (SlotCode, ZoneId, Status, CurrentLicensePlate) VALUES (@code, @zone, @status, @plate)",
                    ("@code", slot.SlotCode),
                    ("@zone", slot.ZoneId),
                    ("@status", slot.Status),
                    ("@plate", slot.CurrentLicensePlate)
                );
            }
        });
    }

    // Hàm cập nhật bất đồng bộ trạng thái và biển số hiện tại của một ô đỗ
    public Task UpdateSlotAsync(ParkingSlot slot)
    {
        return SqliteHelper.ExecuteAsync(
            "UPDATE ParkingSlots SET Status = @status, CurrentLicensePlate = @plate WHERE SlotCode = @code",
            ("@status", slot.Status),
            ("@plate", slot.CurrentLicensePlate),
            ("@code", slot.SlotCode)
        );
    }

    // Hàm xóa bất đồng bộ một ô đỗ xe theo mã ô nếu chưa phát sinh lịch sử gửi xe
    public async Task DeleteSlotAsync(string slotCode)
    {
        try
        {
            await SqliteHelper.ExecuteAsync("DELETE FROM ParkingSlots WHERE SlotCode = @code", ("@code", slotCode));
        }
        catch (SqliteException ex) when (SqliteHelper.IsConstraintError(ex))
        {
            throw new ServiceException("Ô đã có lịch sử gửi xe, không xóa được. Hãy đặt ô sang Bảo trì.");
        }
    }

    // Hàm ánh xạ một dòng dữ liệu SqliteDataReader sang đối tượng ParkingCard
    private static ParkingCard MapCard(SqliteDataReader r)
    {
        return new ParkingCard
        {
            CardCode = r.GetString(0),
            LicensePlate = SqliteHelper.GetNullableString(r, 1),
            Status = r.GetString(2)
        };
    }

    // Hàm lấy bất đồng bộ danh sách toàn bộ thẻ xe
    public Task<List<ParkingCard>> GetCardsAsync()
    {
        return SqliteHelper.QueryAsync(
            "SELECT CardCode, LicensePlate, Status FROM ParkingCards ORDER BY CardCode",
            MapCard
        );
    }

    // Hàm lấy bất đồng bộ thông tin một thẻ xe theo mã thẻ
    public Task<ParkingCard?> GetCardAsync(string cardCode)
    {
        return SqliteHelper.QueryFirstAsync(
            "SELECT CardCode, LicensePlate, Status FROM ParkingCards WHERE CardCode = @code",
            MapCard,
            ("@code", cardCode)
        );
    }

    // Hàm thêm mới bất đồng bộ một thẻ xe vào danh mục
    public Task AddCardAsync(ParkingCard card)
    {
        return SqliteHelper.ExecuteAsync(
            "INSERT INTO ParkingCards (CardCode, LicensePlate, Status) VALUES (@code, @plate, @status)",
            ("@code", card.CardCode),
            ("@plate", card.LicensePlate),
            ("@status", card.Status)
        );
    }

    // Hàm cập nhật bất đồng bộ trạng thái và biển số đang gắn của thẻ xe
    public Task UpdateCardAsync(ParkingCard card)
    {
        return SqliteHelper.ExecuteAsync(
            "UPDATE ParkingCards SET LicensePlate = @plate, Status = @status WHERE CardCode = @code",
            ("@plate", card.LicensePlate),
            ("@status", card.Status),
            ("@code", card.CardCode)
        );
    }

    // Hàm kiểm tra bất đồng bộ xem thẻ xe hiện có đang gắn với lượt xe chưa ra bãi hay không
    public async Task<bool> IsCardCurrentlyInActiveSessionAsync(string cardCode)
    {
        long activeCount = await SqliteHelper.ScalarAsync(
            "SELECT COUNT(*) FROM ParkingSessions WHERE UPPER(CardCode) = UPPER(@code) AND Status = 'Parking';",
            ("@code", cardCode)
        );
        return activeCount > 0;
    }

    // Hàm xóa bất đồng bộ một thẻ xe nếu chưa từng phát sinh lịch sử gửi xe
    public async Task DeleteCardAsync(string cardCode)
    {
        try
        {
            await SqliteHelper.ExecuteAsync("DELETE FROM ParkingCards WHERE CardCode = @code", ("@code", cardCode));
        }
        catch (SqliteException ex) when (SqliteHelper.IsConstraintError(ex))
        {
            throw new ServiceException("Thẻ đã có lịch sử gửi xe, không xóa được. Hãy dùng chức năng Khóa thẻ.");
        }
    }
  
  //NV5: tra cứu lịch sử ra vào
    public List<ParkingSession> getParkingHistory(string keyword,string status , string tuNgay, string denNgay){
        
            var list = new List<ParkingSession>();
            using (var conn = SqliteDbContext.GetConnection())
            {
               string query = @"
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
    WHERE
        (@Keyword IS NULL
            OR LicensePlate LIKE '%' || @Keyword || '%'
            OR CardCode LIKE '%' || @Keyword || '%')
        AND (@Status IS NULL OR Status = @Status)
        AND (@TuNgay IS NULL OR date(CheckInTime) >= date(@TuNgay))
        AND (@DenNgay IS NULL OR date(CheckInTime) <= date(@DenNgay))
    ORDER BY CheckInTime DESC;";
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = query;
                        cmd.Parameters.AddWithValue("@Keyword", string.IsNullOrEmpty(keyword) ? (object)DBNull.Value : keyword);
                        cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(status) ? (object)DBNull.Value : status);
                        cmd.Parameters.AddWithValue("@TuNgay", string.IsNullOrEmpty(tuNgay) ? (object)DBNull.Value : tuNgay);
                        cmd.Parameters.AddWithValue("@DenNgay", string.IsNullOrEmpty(denNgay) ? (object)DBNull.Value : denNgay);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                
                                   list.Add(new ParkingSession
{
                                    SessionId = reader.GetInt32(0),
                                    CardCode = reader.GetString(1),
                                    LicensePlate = reader.GetString(2),
                                    VehicleTypeId = Convert.ToString(reader.GetValue(3)) ?? String.Empty,
                                    SlotCode = reader.GetString(4),
                                    CheckInTime = DateTime.Parse(reader.GetString(5)),
                                    CheckOutTime = reader.IsDBNull(6) ? null : DateTime.Parse(reader.GetString(6)),
                                    CheckInImagePath = reader.IsDBNull(7) ? null : reader.GetString(7),
                                    CheckOutImagePath = reader.IsDBNull(8) ? null : reader.GetString(8),
                                    CheckInUserId = reader.GetInt32(9),
                                    CheckOutUserId = reader.IsDBNull(10) ? (int?)null : reader.GetInt32(10),
                                    IsMonthlyPass = reader.GetInt32(11) == 1,
                                    ParkingFee = reader.GetDecimal(12),
                                    PenaltyFee = reader.GetDecimal(13),
                                    TotalAmount = reader.GetDecimal(14),
                                    Status = reader.GetString(15)
                                    });
                            }
                        }
                    }   
            }
            return list;         
    }
    //NV5: tra cứu biên bản sự cố / mất thẻ

    //user = 
    public bool HandleLostCardReport(long sessionId, string cardCode, string licensePlate, string customerName, string phoneNumber, string customerIdCode, decimal parkingFee, decimal penaltyFee, int userId, long shiftId, string note)
    {
        using (var conn = SqliteDbContext.GetConnection()) 
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    decimal totalAmount = parkingFee + penaltyFee;
                    //1. Cập nhật trạng thái lịch sử ra vào thành "Mất thẻ" (ParkingSessions thành LostCard) - NV5
                    string updateSession = @" UPDATE ParkingSessions SET CheckOutTime = datetime('now', 'localtime'), ParkingFee = @ParkingFee, PenaltyFee = @PenaltyFee,
                    TotalAmount = @TotalAmount, Status = 'LostCard', CheckOutUserId = @UserId WHERE SessionId = @SessionId;";
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;

                        cmd.CommandText = updateSession;
                        cmd.Parameters.AddWithValue("@ParkingFee", parkingFee);
                        cmd.Parameters.AddWithValue("@PenaltyFee", penaltyFee);
                        cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@SessionId", sessionId);
                        
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                    {
                        trans.Rollback();
                        return false; // Không tìm thấy sessionId
                    }
                    } 
                    //2. Khoá thẻ(ParkingCards) - NV5
                    string updateCard = "UPDATE ParkingCards SET Status = 'Locked' WHERE CardCode = @CardCode;";
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        cmd.CommandText = updateCard;
                        cmd.Parameters.AddWithValue("@CardCode", cardCode);
                        cmd.ExecuteNonQuery();
                    }
                    //3. Thêm/Lưu thông tin sự cố mất thẻ(IncidentReports) - NV5
                   string insertIncident = @"
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
                            'LostCard',
                            @CompensationFee,
                            @Note,
                            @UserId,
                            datetime('now', 'localtime')
                        );";
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;

                        cmd.CommandText = insertIncident;
                        cmd.Parameters.AddWithValue("@SessionId", sessionId);
                        cmd.Parameters.AddWithValue("@CardCode", cardCode);
                        cmd.Parameters.AddWithValue("@LicensePlate", licensePlate);
                        cmd.Parameters.AddWithValue("@CustomerName", customerName);
                        cmd.Parameters.AddWithValue("@CustomerPhone", phoneNumber);
                        cmd.Parameters.AddWithValue("@CustomerIdentityCard", customerIdCode);
                        cmd.Parameters.AddWithValue("@CompensationFee", penaltyFee);
                        cmd.Parameters.AddWithValue("@Note", String.IsNullOrEmpty(note) ? (object)DBNull.Value : note);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.ExecuteNonQuery();
                    }
                    //4. Thêm giao dịch thu tiền (PaymentTransactions) - NV5
                    string insertPayment = @"
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
                            'LostCardPenalty',
                            @SessionId,
                            @TotalAmount,
                            'Cash',
                            @UserId,
                            @ShiftId,
                            @Note,
                            datetime('now', 'localtime')
                        );";
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        cmd.CommandText = insertPayment;
                        cmd.Parameters.AddWithValue("@SessionId", sessionId);
                        cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@ShiftId", shiftId == 0 ? (object)DBNull.Value : shiftId);
                        cmd.Parameters.AddWithValue("@Note", "Thu phí giữ xe và phạt mất thẻ");
                        cmd.ExecuteNonQuery();

                    }

                    trans.Commit();
                    return true;
                           
                }
                catch
                {
                       trans.Rollback();
                          return false;
                }
            }
                     
    }   
            

    //NV6: Thống kê doanh thu theo ca làm việc, Quản lý ca trực..

       //1. Thống kê doanh thu theo ca trực..(work Shift) - NV6
    public Dictionary<string, object> GetShiftRevenueReport(long shiftId)
    {
        var result = new Dictionary<string, object>();
        using (var conn = SqliteDbContext.GetConnection())
        {
                string query = @"
                    SELECT
                        s.ShiftId,
                        s.ShiftName,
                        s.StartTime,
                        s.EndTime,
                        s.Status,
                        s.TotalVehiclesOut,

                        SUM(
                            CASE
                                WHEN p.TransactionType = 'ParkingFee'
                                THEN p.Amount
                                ELSE 0
                            END
                        ) AS RevenueNormal,

                        SUM(
                            CASE
                                WHEN p.TransactionType = 'LostCardPenalty'
                                THEN p.Amount
                                ELSE 0
                            END
                        ) AS RevenueLostCard,

                        SUM(p.Amount) AS TotalRevenue

                    FROM WorkShifts s
                    LEFT JOIN PaymentTransactions p
                        ON s.ShiftId = p.ShiftId

                    WHERE s.ShiftId = @ShiftId

                    GROUP BY
                        s.ShiftId,
                        s.ShiftName,
                        s.StartTime,
                        s.EndTime,
                        s.Status,
                        s.TotalVehiclesOut;";


            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@ShiftId", shiftId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())  
                    {
                        result["ShiftId"] = reader.GetInt64(0);
                        result["ShiftName"] = reader.GetString(1);
                        result["StartTime"] = reader.GetString(2);
                        result["EndTime"] = reader.IsDBNull(3) ? "Đang Mở" : reader.GetString(3);
                        result["Status"] = reader.GetString(4);
                        result["TotalVehiclesOut"] = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                        result["RevenueNormal"] = reader.IsDBNull(6) ? 0m : reader.GetDecimal(6);
                        result["RevenueLostCard"] = reader.IsDBNull(7) ? 0m : reader.GetDecimal(7);
                        result["TotalRevenue"] = reader.IsDBNull(8) ? 0m : reader.GetDecimal(8);
                    }
                }
            }
        }
        return result;
    }
    //2. Chốt ca trực (Close Shift) - NV6
    public bool CloseShift(long shiftId, decimal finalRevenue)
    {
        using var conn = SqliteDbContext.GetConnection();
        {
            string query = @"
                    UPDATE WorkShifts
                    SET EndTime = datetime('now', 'localtime'),
                        TotalRevenue = @FinalRevenue,
                        Status = 'Closed'
                    WHERE ShiftId = @ShiftId
                      AND Status = 'Open';";
                using (var cmd = conn.CreateCommand())
            {
             cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@FinalRevenue", finalRevenue);
                cmd.Parameters.AddWithValue("@ShiftId", shiftId);
                int rowsAffected = cmd.ExecuteNonQuery();  
                return rowsAffected > 0;
            }  
        }  
    }
}

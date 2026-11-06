using BAIDOXETHONGMINH.DAL.Database;
using BAIDOXETHONGMINH.DTO.Entities;
using Microsoft.Data.Sqlite;

namespace BAIDOXETHONGMINH.GUI;

/// <summary>Truy vấn các bảng VehicleTypes, PricingRules, ParkingZones, ParkingSlots, ParkingCards bằng ADO.NET (SQLite).</summary>
public class ParkingRepository
{
    // ---------- Loại xe & bảng giá ----------
    private static VehicleType MapType(SqliteDataReader r) => new()
    {
        VehicleTypeId = r.GetString(0),
        TypeName = r.GetString(1),
        Description = SqliteHelper.GetNullableString(r, 2)
    };

    private const string SelectRule =
        "SELECT RuleId, VehicleTypeId, DayTurnPrice, OvernightPrice, HourlyPrice, MonthlyPassPrice, LostCardPenalty, UpdatedAt FROM PricingRules";

    private static PricingRule MapRule(SqliteDataReader r) => new()
    {
        RuleId = r.GetInt32(0),
        VehicleTypeId = r.GetString(1),
        DayTurnPrice = r.GetDecimal(2),
        OvernightPrice = r.GetDecimal(3),
        HourlyPrice = r.GetDecimal(4),
        MonthlyPassPrice = r.GetDecimal(5),
        LostCardPenalty = r.GetDecimal(6),
        UpdatedAt = r.GetDateTime(7)
    };

    public Task<List<VehicleType>> GetVehicleTypesAsync() =>
        SqliteHelper.QueryAsync("SELECT VehicleTypeId, TypeName, Description FROM VehicleTypes ORDER BY VehicleTypeId", MapType);

    public Task<List<PricingRule>> GetPricingRulesAsync() =>
        SqliteHelper.QueryAsync(SelectRule + " ORDER BY VehicleTypeId", MapRule);

    public Task<PricingRule?> GetPricingRuleAsync(string vehicleTypeId) =>
        SqliteHelper.QueryFirstAsync(SelectRule + " WHERE VehicleTypeId = @type", MapRule, ("@type", vehicleTypeId));

    /// <summary>Loại xe đã có bảng giá thì cập nhật, chưa có thì thêm mới (VehicleTypeId là cột UNIQUE).</summary>
    public Task SavePricingRuleAsync(PricingRule rule) =>
        SqliteHelper.ExecuteAsync(
            @"INSERT INTO PricingRules (VehicleTypeId, DayTurnPrice, OvernightPrice, HourlyPrice, MonthlyPassPrice, LostCardPenalty, UpdatedAt)
              VALUES (@type, @day, @night, @hour, @month, @penalty, @updated)
              ON CONFLICT(VehicleTypeId) DO UPDATE SET
                  DayTurnPrice = excluded.DayTurnPrice,
                  OvernightPrice = excluded.OvernightPrice,
                  HourlyPrice = excluded.HourlyPrice,
                  MonthlyPassPrice = excluded.MonthlyPassPrice,
                  LostCardPenalty = excluded.LostCardPenalty,
                  UpdatedAt = excluded.UpdatedAt",
            ("@type", rule.VehicleTypeId), ("@day", rule.DayTurnPrice), ("@night", rule.OvernightPrice),
            ("@hour", rule.HourlyPrice), ("@month", rule.MonthlyPassPrice), ("@penalty", rule.LostCardPenalty),
            ("@updated", rule.UpdatedAt));

    // ---------- Khu ----------
    private static ParkingZone MapZone(SqliteDataReader r) => new()
    {
        ZoneId = r.GetString(0),
        ZoneName = r.GetString(1),
        VehicleTypeId = r.GetString(2),
        MaxCapacity = r.GetInt32(3)
    };

    public Task<List<ParkingZone>> GetZonesAsync() =>
        SqliteHelper.QueryAsync("SELECT ZoneId, ZoneName, VehicleTypeId, MaxCapacity FROM ParkingZones ORDER BY ZoneId", MapZone);

    public Task AddZoneAsync(ParkingZone zone) =>
        SqliteHelper.ExecuteAsync(
            "INSERT INTO ParkingZones (ZoneId, ZoneName, VehicleTypeId, MaxCapacity) VALUES (@id, @name, @type, @capacity)",
            ("@id", zone.ZoneId), ("@name", zone.ZoneName), ("@type", zone.VehicleTypeId), ("@capacity", zone.MaxCapacity));

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

    // ---------- Ô đỗ ----------
    private const string SelectSlot = "SELECT SlotCode, ZoneId, Status, CurrentLicensePlate FROM ParkingSlots";

    private static ParkingSlot MapSlot(SqliteDataReader r) => new()
    {
        SlotCode = r.GetString(0),
        ZoneId = r.GetString(1),
        Status = r.GetString(2),
        CurrentLicensePlate = SqliteHelper.GetNullableString(r, 3)
    };

    public Task<List<ParkingSlot>> GetSlotsAsync() =>
        SqliteHelper.QueryAsync(SelectSlot + " ORDER BY SlotCode", MapSlot);

    public Task<ParkingSlot?> GetSlotAsync(string slotCode) =>
        SqliteHelper.QueryFirstAsync(SelectSlot + " WHERE SlotCode = @code", MapSlot, ("@code", slotCode));

    /// <summary>Thêm nhiều ô trong một Transaction (hoặc thêm hết, hoặc không thêm ô nào).</summary>
    public Task AddSlotsAsync(List<ParkingSlot> slots) =>
        SqliteHelper.InTransactionAsync(async (connection, tx) =>
        {
            foreach (var slot in slots)
            {
                await SqliteHelper.ExecuteAsync(connection, tx,
                    "INSERT INTO ParkingSlots (SlotCode, ZoneId, Status, CurrentLicensePlate) VALUES (@code, @zone, @status, @plate)",
                    ("@code", slot.SlotCode), ("@zone", slot.ZoneId), ("@status", slot.Status),
                    ("@plate", slot.CurrentLicensePlate));
            }
        });

    public Task UpdateSlotAsync(ParkingSlot slot) =>
        SqliteHelper.ExecuteAsync(
            "UPDATE ParkingSlots SET Status = @status, CurrentLicensePlate = @plate WHERE SlotCode = @code",
            ("@status", slot.Status), ("@plate", slot.CurrentLicensePlate), ("@code", slot.SlotCode));

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

    // ---------- Thẻ xe ----------
    private const string SelectCard = "SELECT CardCode, LicensePlate, Status FROM ParkingCards";

    private static ParkingCard MapCard(SqliteDataReader r) => new()
    {
        CardCode = r.GetString(0),
        LicensePlate = SqliteHelper.GetNullableString(r, 1),
        Status = r.GetString(2)
    };

    public Task<List<ParkingCard>> GetCardsAsync() =>
        SqliteHelper.QueryAsync(SelectCard + " ORDER BY CardCode", MapCard);

    public Task<ParkingCard?> GetCardAsync(string cardCode) =>
        SqliteHelper.QueryFirstAsync(SelectCard + " WHERE CardCode = @code", MapCard, ("@code", cardCode));

    public Task AddCardAsync(ParkingCard card) =>
        SqliteHelper.ExecuteAsync(
            "INSERT INTO ParkingCards (CardCode, LicensePlate, Status) VALUES (@code, @plate, @status)",
            ("@code", card.CardCode), ("@plate", card.LicensePlate), ("@status", card.Status));

    public Task UpdateCardAsync(ParkingCard card) =>
        SqliteHelper.ExecuteAsync(
            "UPDATE ParkingCards SET LicensePlate = @plate, Status = @status WHERE CardCode = @code",
            ("@plate", card.LicensePlate), ("@status", card.Status), ("@code", card.CardCode));

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
}

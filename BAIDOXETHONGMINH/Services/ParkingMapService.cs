using BAIDOXETHONGMINH.DTO.Entities;
using BAIDOXETHONGMINH.Models;

namespace BAIDOXETHONGMINH.GUI;

/// <summary>
/// NV3: quản lý khu, ô đỗ, dữ liệu sơ đồ 2D.
/// NV1, NV2 gọi FindAvailableSlotAsync / OccupyAsync / ReleaseAsync / ReleaseByPlateAsync.
/// </summary>
public class ParkingMapService
{
    private readonly ParkingRepository _repo = new();

    /// <summary>Form sơ đồ đăng ký sự kiện này (+=) để tự vẽ lại khi ô đỗ thay đổi.</summary>
    public event Action? MapChanged;

    // ---------- Dữ liệu cho sơ đồ 2D ----------
    /// <summary>Lấy khu và ô bằng 2 câu truy vấn rồi ghép trong RAM.</summary>
    public async Task<List<ZoneView>> GetMapAsync()
    {
        var zones = await _repo.GetZonesAsync();
        var slots = await _repo.GetSlotsAsync();

        var views = new List<ZoneView>();
        foreach (var zone in zones)
        {
            var zoneSlots = slots.Where(s => s.ZoneId == zone.ZoneId).OrderBy(s => s.SlotCode).ToList();
            views.Add(new ZoneView
            {
                Zone = zone,
                Slots = zoneSlots,
                Total = zoneSlots.Count,
                Available = zoneSlots.Count(s => s.Status == SlotStatus.Available),
                Occupied = zoneSlots.Count(s => s.Status == SlotStatus.Occupied),
                Maintenance = zoneSlots.Count(s => s.Status == SlotStatus.Maintenance)
            });
        }
        return views;
    }

    // ---------- Khu ----------
    public async Task AddZoneAsync(string zoneId, string zoneName, string vehicleTypeId, int maxCapacity)
    {
        zoneId = (zoneId ?? "").Trim().ToUpperInvariant();
        zoneName = (zoneName ?? "").Trim();
        vehicleTypeId = (vehicleTypeId ?? "").Trim().ToUpperInvariant();

        if (zoneId.Length == 0) throw new ServiceException("Mã khu không được để trống.");
        if (zoneName.Length == 0) throw new ServiceException("Tên khu không được để trống.");
        if (maxCapacity <= 0) throw new ServiceException("Sức chứa phải lớn hơn 0.");

        var types = await _repo.GetVehicleTypesAsync();
        if (!types.Any(t => t.VehicleTypeId == vehicleTypeId))
            throw new ServiceException($"Loại xe '{vehicleTypeId}' không tồn tại.");

        var zones = await _repo.GetZonesAsync();
        if (zones.Any(z => z.ZoneId == zoneId)) throw new ServiceException($"Khu '{zoneId}' đã tồn tại.");

        await _repo.AddZoneAsync(new ParkingZone
        {
            ZoneId = zoneId,
            ZoneName = zoneName,
            VehicleTypeId = vehicleTypeId,
            MaxCapacity = maxCapacity
        });
        MapChanged?.Invoke();
    }

    public async Task DeleteZoneAsync(string zoneId)
    {
        var slots = await _repo.GetSlotsAsync();
        if (slots.Any(s => s.ZoneId == zoneId)) throw new ServiceException("Khu còn ô đỗ, hãy xóa ô trước.");

        await _repo.DeleteZoneAsync(zoneId);
        MapChanged?.Invoke();
    }

    // ---------- Ô đỗ ----------
    /// <summary>Thêm hàng loạt: mã ô = tiền tố + số thứ tự (A- + 01 = A-01). Kiểm tra không vượt sức chứa khu.</summary>
    public async Task AddSlotsAsync(string zoneId, string prefix, int count)
    {
        zoneId = (zoneId ?? "").Trim().ToUpperInvariant();
        prefix = (prefix ?? "").Trim().ToUpperInvariant();
        if (prefix.Length == 0) throw new ServiceException("Tiền tố mã ô không được để trống.");
        if (count < 1) throw new ServiceException("Số lượng ô phải lớn hơn 0.");

        var zones = await _repo.GetZonesAsync();
        var zone = zones.FirstOrDefault(z => z.ZoneId == zoneId);
        if (zone == null) throw new ServiceException("Không tìm thấy khu.");

        var slots = await _repo.GetSlotsAsync();
        int current = slots.Count(s => s.ZoneId == zoneId);
        if (current + count > zone.MaxCapacity)
            throw new ServiceException($"{zone.ZoneName} chỉ còn chỗ cho {zone.MaxCapacity - current} ô (sức chứa {zone.MaxCapacity}).");

        var existingCodes = new HashSet<string>(slots.Select(s => s.SlotCode));
        var newSlots = new List<ParkingSlot>();
        int number = current + 1;
        while (newSlots.Count < count)
        {
            string code = $"{prefix}{number:00}";
            number++;
            if (existingCodes.Contains(code)) continue;
            newSlots.Add(new ParkingSlot { SlotCode = code, ZoneId = zoneId, Status = SlotStatus.Available });
        }

        await _repo.AddSlotsAsync(newSlots);
        MapChanged?.Invoke();
    }

    public async Task DeleteSlotAsync(string slotCode)
    {
        var slot = await GetSlotRequiredAsync(slotCode);
        if (slot.Status == SlotStatus.Occupied) throw new ServiceException("Ô đang có xe, không xóa được.");

        await _repo.DeleteSlotAsync(slot.SlotCode);
        MapChanged?.Invoke();
    }

    /// <summary>Đổi qua lại: Trống &lt;-&gt; Bảo trì. Ô đang có xe thì không đổi được.</summary>
    public async Task ToggleMaintenanceAsync(string slotCode)
    {
        var slot = await GetSlotRequiredAsync(slotCode);
        if (slot.Status == SlotStatus.Occupied)
            throw new ServiceException("Ô đỗ này đang có xe, không thể chuyển sang Bảo trì!");

        slot.Status = slot.Status == SlotStatus.Maintenance ? SlotStatus.Available : SlotStatus.Maintenance;
        await _repo.UpdateSlotAsync(slot);
        MapChanged?.Invoke();
    }

    // ---------- NV1, NV2 gọi ----------
    /// <summary>Gợi ý ô trống đầu tiên cho loại xe, null nếu hết chỗ.</summary>
    public async Task<ParkingSlot?> FindAvailableSlotAsync(string vehicleTypeId)
    {
        var zones = await _repo.GetZonesAsync();
        var slots = await _repo.GetSlotsAsync();
        var zoneIds = zones.Where(z => z.VehicleTypeId == vehicleTypeId).Select(z => z.ZoneId).ToList();

        return slots.Where(s => zoneIds.Contains(s.ZoneId) && s.Status == SlotStatus.Available)
                    .OrderBy(s => s.ZoneId).ThenBy(s => s.SlotCode)
                    .FirstOrDefault();
    }

    /// <summary>Xe vào đỗ: Available -> Occupied.</summary>
    public async Task OccupyAsync(string slotCode, string licensePlate)
    {
        var slot = await GetSlotRequiredAsync(slotCode);
        if (slot.Status != SlotStatus.Available)
            throw new ServiceException($"Ô '{slot.SlotCode}' không trống.");

        slot.Status = SlotStatus.Occupied;
        slot.CurrentLicensePlate = PlateHelper.Clean(licensePlate);
        await _repo.UpdateSlotAsync(slot);
        MapChanged?.Invoke();
    }

    /// <summary>Xe ra: Occupied -> Available.</summary>
    public async Task ReleaseAsync(string slotCode)
    {
        var slot = await GetSlotRequiredAsync(slotCode);
        if (slot.Status != SlotStatus.Occupied) throw new ServiceException("Ô không có xe.");

        slot.Status = SlotStatus.Available;
        slot.CurrentLicensePlate = null;
        await _repo.UpdateSlotAsync(slot);
        MapChanged?.Invoke();
    }

    /// <summary>Giải phóng ô theo biển số (lúc xe ra chỉ biết biển số). Trả về false nếu không thấy xe trong bãi.</summary>
    public async Task<bool> ReleaseByPlateAsync(string licensePlate)
    {
        var slots = await _repo.GetSlotsAsync();
        var slot = slots.FirstOrDefault(s =>
            s.Status == SlotStatus.Occupied && PlateHelper.Same(s.CurrentLicensePlate, licensePlate));
        if (slot == null) return false;

        await ReleaseAsync(slot.SlotCode);
        return true;
    }

    private async Task<ParkingSlot> GetSlotRequiredAsync(string slotCode)
    {
        string code = (slotCode ?? "").Trim().ToUpperInvariant();
        var slot = await _repo.GetSlotAsync(code);
        if (slot == null) throw new ServiceException($"Không tìm thấy ô '{slotCode}'.");
        return slot;
    }
}

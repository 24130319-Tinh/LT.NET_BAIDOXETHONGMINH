using BAIDOXETHONGMINH.DTO.Entities;
using BAIDOXETHONGMINH.Models;
using BAIDOXETHONGMINH.DTO.Entities;
using BAIDOXETHONGMINH.GUI;
using BAIDOXETHONGMINH.Models;


namespace BAIDOXETHONGMINH.Services;
public class ParkingMapService
{
    private readonly ParkingRepository _repo = new();
    public event Action? MapChanged;

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

    public async Task ToggleMaintenanceAsync(string slotCode)
    {
        var slot = await GetSlotRequiredAsync(slotCode);
        if (slot.Status == SlotStatus.Occupied)
            throw new ServiceException("Ô này đang có xe đỗ, không thể chuyển sang Bảo trì!");

        slot.Status = slot.Status == SlotStatus.Maintenance ? SlotStatus.Available : SlotStatus.Maintenance;
        await _repo.UpdateSlotAsync(slot);
        MapChanged?.Invoke();
    }


    private async Task<ParkingSlot> GetSlotRequiredAsync(string slotCode)
    {
        string code = (slotCode ?? "").Trim().ToUpperInvariant();

        var slot = await _repo.GetSlotAsync(code);

        if (slot == null)
            throw new ServiceException($"Không tìm thấy ô '{slotCode}'");

        return slot;
    }
}
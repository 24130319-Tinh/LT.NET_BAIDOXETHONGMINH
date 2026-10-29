namespace BAIDOXETHONGMINH.DTO.Entities;

/// <summary>
/// Bảng ParkingSlots: Từng vị trí (ô đỗ) trên sơ đồ bãi xe
/// Status: "Available" (Trống), "Occupied" (Đang có xe), "Maintenance" (Bảo trì)
/// </summary>
public class ParkingSlot
{
    public string SlotCode { get; set; } = string.Empty;
    public string ZoneId { get; set; } = string.Empty;
    public string Status { get; set; } = "Available";
    public string? CurrentLicensePlate { get; set; }
}

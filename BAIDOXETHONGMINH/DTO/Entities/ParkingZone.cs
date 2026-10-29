namespace BAIDOXETHONGMINH.DTO.Entities;

/// <summary>
/// Bảng ParkingZones: Khu vực đỗ xe trong bãi (VD: Khu A - Xe máy, Khu B - Ô tô)
/// </summary>
public class ParkingZone
{
    public string ZoneId { get; set; } = string.Empty;
    public string ZoneName { get; set; } = string.Empty;
    public string VehicleTypeId { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }
}

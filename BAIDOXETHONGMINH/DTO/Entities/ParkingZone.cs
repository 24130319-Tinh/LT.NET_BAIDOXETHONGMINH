namespace BAIDOXETHONGMINH.DTO.Entities;

public class ParkingZone
{
    public string ZoneId { get; set; } = string.Empty;
    public string ZoneName { get; set; } = string.Empty;
    public string VehicleTypeId { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }
}

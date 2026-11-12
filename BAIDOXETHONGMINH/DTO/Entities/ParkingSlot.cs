namespace BAIDOXETHONGMINH.DTO.Entities;

public class ParkingSlot
{
    public string SlotCode { get; set; } = string.Empty;
    public string ZoneId { get; set; } = string.Empty;
    public string Status { get; set; } = "Available";
    public string? CurrentLicensePlate { get; set; }
}

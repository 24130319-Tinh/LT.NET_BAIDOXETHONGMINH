namespace BAIDOXETHONGMINH.DTO.Entities;

public class ParkingCard
{
    public string CardCode { get; set; } = string.Empty;
    public string? LicensePlate { get; set; }
    public string Status { get; set; } = "Available";
}

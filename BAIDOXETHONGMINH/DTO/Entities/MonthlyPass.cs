namespace BAIDOXETHONGMINH.DTO.Entities;

public class MonthlyPass
{
    public int PassId { get; set; }
    public int CustomerId { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string VehicleTypeId { get; set; } = string.Empty;
    public string? VehicleBrandColor { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Now;
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Active";
}

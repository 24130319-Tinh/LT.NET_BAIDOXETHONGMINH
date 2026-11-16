namespace BAIDOXETHONGMINH.DTO.Entities;

public class WorkShift
{
    public int ShiftId { get; set; }
    public int UserId { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime? EndTime { get; set; }
    public int TotalVehiclesIn { get; set; }
    public int TotalVehiclesOut { get; set; }
    public decimal TotalRevenue { get; set; }
    public string Status { get; set; } = "Open";
}

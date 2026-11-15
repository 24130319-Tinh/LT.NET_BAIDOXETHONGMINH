namespace BAIDOXETHONGMINH.DTO.Models;

public class FeeCalculationResult
{
    public string DurationText { get; set; } = string.Empty;
    public string TicketTypeDisplay { get; set; } = string.Empty;
    public bool IsMonthlyPass { get; set; }
    public int RemainingPassDays { get; set; }
    public decimal ParkingFee { get; set; }
    public decimal PenaltyFee { get; set; }
    public decimal TotalAmount { get; set; }
}

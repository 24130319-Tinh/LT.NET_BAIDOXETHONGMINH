namespace BAIDOXETHONGMINH.DTO.Entities;

/// <summary>
/// Bảng PricingRules: Cấu hình bảng giá đỗ xe theo từng loại phương tiện
/// </summary>
public class PricingRule
{
    public int RuleId { get; set; }
    public string VehicleTypeId { get; set; } = string.Empty;
    public decimal DayTurnPrice { get; set; }
    public decimal OvernightPrice { get; set; }
    public decimal HourlyPrice { get; set; }
    public decimal MonthlyPassPrice { get; set; }
    public decimal LostCardPenalty { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

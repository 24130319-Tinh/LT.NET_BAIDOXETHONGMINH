using BAIDOXETHONGMINH.DTO.Entities;
using BAIDOXETHONGMINH.DTO.Models;

namespace BAIDOXETHONGMINH.Services.Strategies;

/// <summary>
/// Giao diện Chiến lược tính phí gửi xe (Strategy Pattern)
/// </summary>
public interface IFeeCalculationStrategy
{
    FeeCalculationResult Calculate(
        DateTime checkInTime,
        DateTime checkOutTime,
        string baseDurationText,
        PricingRule? pricingRule,
        MonthlyPass? activePass,
        decimal penaltyFee);
}

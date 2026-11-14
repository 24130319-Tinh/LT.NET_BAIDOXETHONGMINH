using BAIDOXETHONGMINH.DTO.Entities;
using BAIDOXETHONGMINH.DTO.Models;

namespace BAIDOXETHONGMINH.Services.Strategies;

/// <summary>
/// Chiến lược 1: Tính phí cho Khách có Vé tháng hợp lệ và còn thời hạn (Miễn phí gửi lượt)
/// </summary>
public class MonthlyPassFeeStrategy : IFeeCalculationStrategy
{
    public FeeCalculationResult Calculate(
        DateTime checkInTime,
        DateTime checkOutTime,
        string baseDurationText,
        PricingRule? pricingRule,
        MonthlyPass? activePass,
        decimal penaltyFee)
    {
        int remainingDays = activePass != null
            ? Math.Max(0, (activePass.EndDate.Date - checkOutTime.Date).Days)
            : 0;

        return new FeeCalculationResult
        {
            DurationText = baseDurationText,
            TicketTypeDisplay = $"★ Vé tháng (0đ • Còn {remainingDays} ngày)",
            IsMonthlyPass = true,
            RemainingPassDays = remainingDays,
            ParkingFee = 0m,
            PenaltyFee = penaltyFee,
            TotalAmount = penaltyFee
        };
    }
}

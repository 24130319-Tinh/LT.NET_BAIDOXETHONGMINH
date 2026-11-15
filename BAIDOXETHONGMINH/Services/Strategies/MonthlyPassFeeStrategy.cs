using BAIDOXETHONGMINH.DTO.Entities;
using BAIDOXETHONGMINH.DTO.Models;

namespace BAIDOXETHONGMINH.Services.Strategies;

public class MonthlyPassFeeStrategy : IFeeCalculationStrategy
{
    // Hàm tính phí cho khách có Vé tháng hợp lệ còn hạn (miễn phí gửi lượt, chỉ cộng tiền phạt nếu mất thẻ)
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

using BAIDOXETHONGMINH.DTO.Entities;
using BAIDOXETHONGMINH.DTO.Models;

namespace BAIDOXETHONGMINH.Services.Strategies;

/// <summary>
/// Chiến lược 2: Tính phí cho Ô tô gửi Vé lượt (Tính theo giờ làm tròn lên + Phụ thu qua đêm)
/// </summary>
public class CarTurnFeeStrategy : IFeeCalculationStrategy
{
    public FeeCalculationResult Calculate(
        DateTime checkInTime,
        DateTime checkOutTime,
        string baseDurationText,
        PricingRule? pricingRule,
        MonthlyPass? activePass,
        decimal penaltyFee)
    {
        TimeSpan duration = checkOutTime - checkInTime;

        decimal hourlyPrice = pricingRule != null && pricingRule.HourlyPrice > 0m
            ? pricingRule.HourlyPrice
            : 20000m;

        decimal overnightPrice = pricingRule != null && pricingRule.OvernightPrice > 0m
            ? pricingRule.OvernightPrice
            : 60000m;

        int roundedHours = Math.Max(1, (int)Math.Ceiling(duration.TotalHours));
        int nightsCrossed = Math.Max(0, (checkOutTime.Date - checkInTime.Date).Days);

        decimal hourlyFee = roundedHours * hourlyPrice;
        decimal overnightSurcharge = nightsCrossed * overnightPrice;
        decimal carParkingFee = hourlyFee + overnightSurcharge;

        string carTicketDisplay = nightsCrossed > 0
            ? $"Vé lượt ({hourlyPrice:N0}đ/h + {nightsCrossed} đêm)"
            : $"Vé lượt ({hourlyPrice:N0}đ/h)";

        return new FeeCalculationResult
        {
            DurationText = $"{baseDurationText} (Tính {roundedHours}h)",
            TicketTypeDisplay = carTicketDisplay,
            IsMonthlyPass = false,
            RemainingPassDays = 0,
            ParkingFee = carParkingFee,
            PenaltyFee = penaltyFee,
            TotalAmount = carParkingFee + penaltyFee
        };
    }
}

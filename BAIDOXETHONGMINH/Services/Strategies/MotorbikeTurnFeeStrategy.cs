using BAIDOXETHONGMINH.DTO.Entities;
using BAIDOXETHONGMINH.DTO.Models;

namespace BAIDOXETHONGMINH.Services.Strategies;

public class MotorbikeTurnFeeStrategy : IFeeCalculationStrategy
{
    // Hàm tính phí cho Xe máy gửi vé lượt (tính theo lượt ban ngày hoặc lượt qua đêm)
    public FeeCalculationResult Calculate(
        DateTime checkInTime,
        DateTime checkOutTime,
        string baseDurationText,
        PricingRule? pricingRule,
        MonthlyPass? activePass,
        decimal penaltyFee)
    {
        decimal dayTurnPrice = pricingRule != null && pricingRule.DayTurnPrice > 0m
            ? pricingRule.DayTurnPrice
            : 5000m;

        decimal overnightPrice = pricingRule != null && pricingRule.OvernightPrice > 0m
            ? pricingRule.OvernightPrice
            : 10000m;

        int nightsCrossed = Math.Max(0, (checkOutTime.Date - checkInTime.Date).Days);
        bool isOvernightTurn = nightsCrossed > 0 || checkInTime.Hour >= 22 || checkOutTime.Hour >= 22 || checkOutTime.Hour < 6;

        decimal motorbikeParkingFee;
        string motorbikeTicketDisplay;

        if (isOvernightTurn)
        {
            int overnightCount = Math.Max(1, nightsCrossed);
            motorbikeParkingFee = overnightCount * overnightPrice;
            motorbikeTicketDisplay = overnightCount > 1
                ? $"Vé qua đêm ({overnightCount} đêm)"
                : $"Vé qua đêm ({overnightPrice:N0}đ)";
        }
        else
        {
            motorbikeParkingFee = dayTurnPrice;
            motorbikeTicketDisplay = $"Vé lượt ngày ({dayTurnPrice:N0}đ)";
        }

        return new FeeCalculationResult
        {
            DurationText = baseDurationText,
            TicketTypeDisplay = motorbikeTicketDisplay,
            IsMonthlyPass = false,
            RemainingPassDays = 0,
            ParkingFee = motorbikeParkingFee,
            PenaltyFee = penaltyFee,
            TotalAmount = motorbikeParkingFee + penaltyFee
        };
    }
}

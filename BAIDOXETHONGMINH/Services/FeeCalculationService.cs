using BAIDOXETHONGMINH.DTO.Entities;
using BAIDOXETHONGMINH.DTO.Models;
using BAIDOXETHONGMINH.Services.Strategies;

namespace BAIDOXETHONGMINH.Services;

/// <summary>
/// Context điều phối thuật toán tính phí gửi xe và tiền phạt mất thẻ theo Strategy Pattern
/// </summary>
public class FeeCalculationService
{
    /// <summary>
    /// Mức phạt mất thẻ thống nhất áp dụng chung cho mọi loại thẻ / loại xe (50.000 VNĐ)
    /// </summary>
    public const decimal UnifiedLostCardPenalty = 50000m;

    private readonly IFeeCalculationStrategy _monthlyPassStrategy = new MonthlyPassFeeStrategy();
    private readonly IFeeCalculationStrategy _carTurnStrategy = new CarTurnFeeStrategy();
    private readonly IFeeCalculationStrategy _motorbikeTurnStrategy = new MotorbikeTurnFeeStrategy();

    /// <summary>
    /// Tính toán chi tiết phí đỗ xe bằng cách lựa chọn Strategy phù hợp.
    /// </summary>
    public FeeCalculationResult CalculateFee(
        DateTime checkInTime,
        DateTime checkOutTime,
        string vehicleTypeId,
        PricingRule? pricingRule,
        MonthlyPass? activePass,
        bool isLostCard)
    {
        DateTime safeCheckOutTime = checkOutTime < checkInTime ? checkInTime : checkOutTime;
        TimeSpan duration = safeCheckOutTime - checkInTime;

        string baseDurationText = FormatDurationText(duration);
        decimal penaltyFee = isLostCard ? UnifiedLostCardPenalty : 0m;

        IFeeCalculationStrategy strategy = ResolveStrategy(vehicleTypeId, activePass, safeCheckOutTime);
        return strategy.Calculate(
            checkInTime,
            safeCheckOutTime,
            baseDurationText,
            pricingRule,
            activePass,
            penaltyFee
        );
    }

    /// <summary>
    /// Lựa chọn chiến lược tính phí dựa trên trạng thái Vé tháng và Loại phương tiện
    /// </summary>
    public IFeeCalculationStrategy ResolveStrategy(
        string vehicleTypeId,
        MonthlyPass? activePass,
        DateTime checkOutTime)
    {
        if (IsMonthlyPassValidAtCheckOut(activePass, checkOutTime))
        {
            return _monthlyPassStrategy;
        }

        bool isCar = string.Equals(vehicleTypeId.Trim(), "OTO", StringComparison.OrdinalIgnoreCase);
        if (isCar)
        {
            return _carTurnStrategy;
        }

        return _motorbikeTurnStrategy;
    }

    private static bool IsMonthlyPassValidAtCheckOut(MonthlyPass? activePass, DateTime checkOutTime)
    {
        if (activePass == null)
        {
            return false;
        }

        bool isStatusActive = string.Equals(activePass.Status, "Active", StringComparison.OrdinalIgnoreCase);
        bool isWithinDateRange = checkOutTime.Date >= activePass.StartDate.Date
                              && checkOutTime.Date <= activePass.EndDate.Date;

        return isStatusActive && isWithinDateRange;
    }

    private static string FormatDurationText(TimeSpan duration)
    {
        int totalMinutes = Math.Max(1, (int)Math.Ceiling(duration.TotalMinutes));
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;

        if (hours > 0)
        {
            return $"{hours}h {minutes:D2}p";
        }

        return $"{minutes}p";
    }
}

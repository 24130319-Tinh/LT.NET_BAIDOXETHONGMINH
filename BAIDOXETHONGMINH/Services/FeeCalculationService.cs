using BAIDOXETHONGMINH.DTO.Entities;
using BAIDOXETHONGMINH.DTO.Models;
using BAIDOXETHONGMINH.Services.Strategies;

namespace BAIDOXETHONGMINH.Services;

public class FeeCalculationService
{
    public const decimal UnifiedLostCardPenalty = 50000m;

    private readonly IFeeCalculationStrategy _monthlyPassStrategy = new MonthlyPassFeeStrategy();
    private readonly IFeeCalculationStrategy _carTurnStrategy = new CarTurnFeeStrategy();
    private readonly IFeeCalculationStrategy _motorbikeTurnStrategy = new MotorbikeTurnFeeStrategy();

    // Hàm điều phối tính toán chi tiết phí đỗ xe và tiền phạt mất thẻ theo chiến lược phù hợp
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

    // Hàm lựa chọn chiến lược tính phí dựa trên trạng thái Vé tháng và Loại phương tiện
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

    // Hàm kiểm tra Vé tháng có đang hoạt động và còn hiệu lực tại thời điểm xe ra hay không
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

    // Hàm định dạng khoảng thời gian đỗ xe sang chuỗi hiển thị giờ và phút
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

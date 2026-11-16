using BAIDOXETHONGMINH.DTO.Entities;
using BAIDOXETHONGMINH.DTO.Models;

namespace BAIDOXETHONGMINH.Services.Strategies;

public interface IFeeCalculationStrategy
{
    // Hàm định nghĩa giao diện tính phí gửi xe và tiền phạt cho từng chiến lược
    FeeCalculationResult Calculate(
        DateTime checkInTime,
        DateTime checkOutTime,
        string baseDurationText,
        PricingRule? pricingRule,
        MonthlyPass? activePass,
        decimal penaltyFee);
}

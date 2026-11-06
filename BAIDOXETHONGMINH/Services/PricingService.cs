using BAIDOXETHONGMINH.DTO.Entities;

namespace BAIDOXETHONGMINH.GUI;

/// <summary>NV3: cấu hình bảng giá theo loại xe.</summary>
public class PricingService
{
    private readonly ParkingRepository _repo = new();

    public Task<List<VehicleType>> GetVehicleTypesAsync() => _repo.GetVehicleTypesAsync();

    public Task<List<PricingRule>> GetAllAsync() => _repo.GetPricingRulesAsync();

    public Task<PricingRule?> GetByVehicleTypeAsync(string vehicleTypeId) => _repo.GetPricingRuleAsync(vehicleTypeId);

    /// <summary>NV khác gọi để lấy giá; ném lỗi nếu loại xe chưa có bảng giá.</summary>
    public async Task<PricingRule> GetRequiredAsync(string vehicleTypeId)
    {
        var rule = await _repo.GetPricingRuleAsync(vehicleTypeId);
        if (rule == null) throw new ServiceException($"Chưa cấu hình bảng giá cho loại xe '{vehicleTypeId}'.");
        return rule;
    }

    /// <summary>Lưu bảng giá (chưa có thì thêm, có rồi thì cập nhật).</summary>
    public async Task SaveAsync(PricingRule rule)
    {
        if (string.IsNullOrWhiteSpace(rule.VehicleTypeId)) throw new ServiceException("Phải chọn loại xe.");
        if (rule.DayTurnPrice < 0 || rule.OvernightPrice < 0 || rule.HourlyPrice < 0 ||
            rule.MonthlyPassPrice < 0 || rule.LostCardPenalty < 0)
            throw new ServiceException("Giá không được âm.");

        var types = await _repo.GetVehicleTypesAsync();
        if (!types.Any(t => t.VehicleTypeId == rule.VehicleTypeId))
            throw new ServiceException($"Loại xe '{rule.VehicleTypeId}' không tồn tại.");

        rule.UpdatedAt = DateTime.Now;
        await _repo.SavePricingRuleAsync(rule);
    }
}

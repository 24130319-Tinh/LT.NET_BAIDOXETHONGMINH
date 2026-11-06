using BAIDOXETHONGMINH.DTO.Entities;
using BAIDOXETHONGMINH.Models;

namespace BAIDOXETHONGMINH.GUI;

/// <summary>
/// NV4: đăng ký, gia hạn, cập nhật, hủy vé tháng.
/// NV1, NV2 gọi FindValidPassAsync / IsValidAsync để miễn thu tiền xe có vé tháng.
/// Quy ước: vé hiệu lực từ ngày StartDate đến hết ngày EndDate.
/// </summary>
public class MonthlyPassService(PricingService pricing)
{
    private readonly MonthlyPassRepository _repo = new();
    private readonly CustomerService _customers = new();

    /// <summary>Danh sách vé kèm chủ xe. filter: ALL, ACTIVE (còn hạn), EXPIRING (sắp / đã hết hạn / đã hủy).</summary>
    public async Task<List<MonthlyPassView>> GetViewsAsync(string keyword = "", string filter = "ALL")
    {
        await _repo.ExpireOverdueAsync(DateTime.Today);
        var views = await _repo.GetViewsAsync();
        var types = await pricing.GetVehicleTypesAsync();
        foreach (var v in views)
            v.VehicleTypeName = types.FirstOrDefault(t => t.VehicleTypeId == v.VehicleTypeId)?.TypeName ?? v.VehicleTypeId;

        keyword = (keyword ?? "").Trim();
        string plateKey = PlateHelper.Key(keyword);

        return views.Where(v =>
        {
            bool matchKeyword = keyword.Length == 0
                || v.OwnerName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || (plateKey.Length > 0 && PlateHelper.Key(v.LicensePlate).Contains(plateKey));
            if (!matchKeyword) return false;

            if (filter == "ACTIVE") return v.State == "ACTIVE";
            if (filter == "EXPIRING") return v.State != "ACTIVE";
            return true;
        }).ToList();
    }

    /// <summary>Số tiền phải thu cho N tháng vé của loại xe.</summary>
    public async Task<decimal> GetFeeAsync(string vehicleTypeId, int months)
    {
        CheckMonths(months);
        var rule = await pricing.GetRequiredAsync(vehicleTypeId);
        return rule.MonthlyPassPrice * months;
    }

    /// <summary>Ngày hết hạn mới khi gia hạn: cộng từ ngày hết hạn nếu còn hạn, từ hôm nay nếu đã hết hạn.</summary>
    public static DateTime GetRenewEndDate(DateTime currentEnd, int months)
    {
        DateTime baseDate = currentEnd.Date >= DateTime.Today ? currentEnd.Date : DateTime.Today;
        return baseDate.AddMonths(months);
    }

    /// <summary>Đăng ký vé mới (tạo hoặc dùng lại khách theo CCCD). Ngày bắt đầu là hôm nay.</summary>
    public async Task<MonthlyPass> RegisterAsync(string fullName, string phone, string identityCard,
        string licensePlate, string vehicleTypeId, string? brand, int months)
    {
        CheckMonths(months);
        string plate = PlateHelper.Clean(licensePlate);
        if (plate.Length == 0) throw new ServiceException("Biển số xe không được để trống.");
        if (string.IsNullOrWhiteSpace(vehicleTypeId)) throw new ServiceException("Phải chọn loại xe.");
        var rule = await pricing.GetRequiredAsync(vehicleTypeId); // loại xe phải có bảng giá

        await _repo.ExpireOverdueAsync(DateTime.Today);
        var actives = await _repo.GetActiveAsync();
        if (actives.Any(p => PlateHelper.Same(p.LicensePlate, plate)))
            throw new ServiceException($"Biển số {plate} đã có vé tháng còn hiệu lực. Hãy dùng chức năng gia hạn.");

        var customer = await _customers.GetOrCreateAsync(fullName, phone, identityCard);

        var today = DateTime.Today;
        var pass = new MonthlyPass
        {
            CustomerId = customer.CustomerId,
            LicensePlate = plate,
            VehicleTypeId = vehicleTypeId,
            VehicleBrandColor = string.IsNullOrWhiteSpace(brand) ? null : brand.Trim(),
            StartDate = today,
            EndDate = today.AddMonths(months),
            Status = PassStatus.Active
        };
        // Có người đang đăng nhập (AppService.CurrentUserId) thì ghi giao dịch thu tiền cùng lúc với vé (một Transaction)
        var payment = NewPayment(PaymentType.MonthlyPassNew, rule.MonthlyPassPrice * months,
            $"Đăng ký vé tháng {months} tháng, xe {plate}");
        await _repo.AddAsync(pass, payment);
        return pass;
    }

    public async Task<MonthlyPass> RenewAsync(int passId, int months)
    {
        CheckMonths(months);
        var pass = await GetRequiredAsync(passId);
        if (pass.Status == PassStatus.Cancelled) throw new ServiceException("Vé đã hủy, không gia hạn được.");
        var rule = await pricing.GetRequiredAsync(pass.VehicleTypeId); // không có bảng giá thì không thu được tiền

        if (pass.EndDate.Date < DateTime.Today) // vé đã hết hạn: tính lại từ hôm nay
        {
            var actives = await _repo.GetActiveAsync();
            if (actives.Any(p => p.PassId != passId && PlateHelper.Same(p.LicensePlate, pass.LicensePlate)))
                throw new ServiceException("Biển số này đã có vé tháng khác còn hiệu lực.");
            pass.StartDate = DateTime.Today;
        }

        pass.EndDate = GetRenewEndDate(pass.EndDate, months);
        pass.Status = PassStatus.Active;
        var payment = NewPayment(PaymentType.MonthlyPassRenew, rule.MonthlyPassPrice * months,
            $"Gia hạn vé tháng {months} tháng, xe {pass.LicensePlate}");
        await _repo.UpdateAsync(pass, payment);
        return pass;
    }

    /// <summary>Cập nhật SĐT chủ xe và mô tả xe (hãng, màu).</summary>
    public async Task UpdateInfoAsync(int passId, string phone, string? brand)
    {
        string newPhone = CustomerService.CheckPhone(phone);
        var pass = await GetRequiredAsync(passId);

        var customer = (await _customers.GetAllAsync()).FirstOrDefault(c => c.CustomerId == pass.CustomerId);
        if (customer == null) throw new ServiceException("Không tìm thấy chủ xe của vé này.");

        customer.Phone = newPhone;
        await _customers.UpdateAsync(customer);

        pass.VehicleBrandColor = string.IsNullOrWhiteSpace(brand) ? null : brand.Trim();
        await _repo.UpdateAsync(pass);
    }

    public async Task CancelAsync(int passId)
    {
        var pass = await GetRequiredAsync(passId);
        if (pass.Status == PassStatus.Cancelled) throw new ServiceException("Vé đã được hủy từ trước.");

        pass.Status = PassStatus.Cancelled;
        await _repo.UpdateAsync(pass);
    }

    // ---------- NV1, NV2 gọi ----------
    /// <summary>Tìm vé tháng hợp lệ hôm nay của biển số, null nếu không có.</summary>
    public async Task<MonthlyPass?> FindValidPassAsync(string licensePlate)
    {
        if (PlateHelper.Key(licensePlate).Length == 0) return null;

        var today = DateTime.Today;
        var actives = await _repo.GetActiveAsync();
        return actives.FirstOrDefault(p =>
            PlateHelper.Same(p.LicensePlate, licensePlate) && p.StartDate.Date <= today && today <= p.EndDate.Date);
    }

    public async Task<bool> IsValidAsync(string licensePlate) => await FindValidPassAsync(licensePlate) != null;

    /// <summary>Tạo giao dịch thu tiền; trả null nếu chưa biết ai đang đăng nhập (cột CollectedByUserId bắt buộc có).</summary>
    private static PaymentTransaction? NewPayment(string type, decimal amount, string note)
    {
        int? userId = AppService.CurrentUserId;
        if (userId == null) return null;

        return new PaymentTransaction
        {
            TransactionType = type,
            Amount = amount,
            PaymentMethod = "Cash",
            CollectedByUserId = userId.Value,
            Note = note,
            CreatedAt = DateTime.Now
        };
    }

    private async Task<MonthlyPass> GetRequiredAsync(int passId)
    {
        var pass = await _repo.GetByIdAsync(passId);
        if (pass == null) throw new ServiceException("Không tìm thấy vé tháng.");
        return pass;
    }

    private static void CheckMonths(int months)
    {
        if (months < 1 || months > 36) throw new ServiceException("Số tháng phải từ 1 đến 36.");
    }
}

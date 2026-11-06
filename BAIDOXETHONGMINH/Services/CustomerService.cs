using System.Text.RegularExpressions;
using BAIDOXETHONGMINH.DTO.Entities;

namespace BAIDOXETHONGMINH.GUI;

/// <summary>NV4: quản lý khách hàng (chủ xe đăng ký vé tháng).</summary>
public class CustomerService
{
    private readonly CustomerRepository _repo = new();
    private readonly MonthlyPassRepository _passRepo = new();

    public Task<List<Customer>> GetAllAsync() => _repo.GetAllAsync();

    /// <summary>Tìm theo họ tên, số điện thoại hoặc CCCD (để trống = lấy tất cả).</summary>
    public Task<List<Customer>> SearchAsync(string keyword)
    {
        keyword = (keyword ?? "").Trim();
        return keyword.Length == 0 ? _repo.GetAllAsync() : _repo.SearchAsync(keyword);
    }

    public async Task AddAsync(Customer customer)
    {
        Validate(customer);
        if (await _repo.GetByIdentityCardAsync(customer.IdentityCard) != null)
            throw new ServiceException("CCCD này đã có trong danh sách khách hàng.");

        customer.CreatedAt = DateTime.Now;
        await _repo.AddAsync(customer);
    }

    public async Task UpdateAsync(Customer customer)
    {
        Validate(customer);
        var existing = await _repo.GetByIdAsync(customer.CustomerId);
        if (existing == null) throw new ServiceException("Không tìm thấy khách hàng.");

        var sameCard = await _repo.GetByIdentityCardAsync(customer.IdentityCard);
        if (sameCard != null && sameCard.CustomerId != customer.CustomerId)
            throw new ServiceException("CCCD này đã được dùng cho khách khác.");

        existing.FullName = customer.FullName;
        existing.Phone = customer.Phone;
        existing.IdentityCard = customer.IdentityCard;
        await _repo.UpdateAsync(existing);
    }

    public async Task DeleteAsync(int customerId)
    {
        if (await _passRepo.HasAnyPassAsync(customerId))
            throw new ServiceException("Khách đã có vé tháng, không xóa được.");
        await _repo.DeleteAsync(customerId);
    }

    /// <summary>Dùng khi đăng ký vé: đã có CCCD thì cập nhật tên, SĐT; chưa có thì tạo khách mới.</summary>
    public async Task<Customer> GetOrCreateAsync(string fullName, string phone, string identityCard)
    {
        var input = new Customer { FullName = fullName, Phone = phone, IdentityCard = identityCard };
        Validate(input);

        var existing = await _repo.GetByIdentityCardAsync(input.IdentityCard);
        if (existing == null)
        {
            await _repo.AddAsync(input);
            return input;
        }

        existing.FullName = input.FullName;
        existing.Phone = input.Phone;
        await _repo.UpdateAsync(existing);
        return existing;
    }

    /// <summary>Kiểm tra số điện thoại, trả về số đã cắt khoảng trắng.</summary>
    public static string CheckPhone(string phone)
    {
        phone = (phone ?? "").Trim();
        if (!Regex.IsMatch(phone, @"^0\d{9}$"))
            throw new ServiceException("Số điện thoại phải gồm 10 số, bắt đầu bằng 0.");
        return phone;
    }

    private static void Validate(Customer c)
    {
        c.FullName = (c.FullName ?? "").Trim();
        c.IdentityCard = (c.IdentityCard ?? "").Trim();

        if (c.FullName.Length == 0) throw new ServiceException("Họ tên không được để trống.");
        c.Phone = CheckPhone(c.Phone);
        if (!Regex.IsMatch(c.IdentityCard, @"^(\d{9}|\d{12})$"))
            throw new ServiceException("CCCD/CMND phải gồm 9 hoặc 12 số.");
    }
}

namespace BAIDOXETHONGMINH.DTO.Entities;

/// <summary>
/// Bảng Customers: Hồ sơ khách hàng / chủ phương tiện
/// </summary>
public class Customer
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string IdentityCard { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

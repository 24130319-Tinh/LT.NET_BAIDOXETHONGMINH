namespace BAIDOXETHONGMINH.DTO.Entities;

/// <summary>
/// Bảng Users: Thông tin tài khoản người quản lý / trực ca
/// </summary>
public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

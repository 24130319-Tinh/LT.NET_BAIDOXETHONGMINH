namespace BAIDOXETHONGMINH.DTO.Entities;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string IdentityCard { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

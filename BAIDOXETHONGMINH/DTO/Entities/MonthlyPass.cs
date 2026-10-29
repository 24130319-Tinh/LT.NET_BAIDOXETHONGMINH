namespace BAIDOXETHONGMINH.DTO.Entities;

/// <summary>
/// Bảng MonthlyPasses: Danh sách đăng ký vé tháng theo biển số xe
/// Status: "Active" (Còn hạn), "Expired" (Hết hạn), "Cancelled" (Đã hủy)
/// </summary>
public class MonthlyPass
{
    public int PassId { get; set; }
    public int CustomerId { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string VehicleTypeId { get; set; } = string.Empty;
    public string? VehicleBrandColor { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Now;
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Active";
}

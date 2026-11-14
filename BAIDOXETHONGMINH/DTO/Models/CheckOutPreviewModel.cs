namespace BAIDOXETHONGMINH.DTO.Models;

/// <summary>
/// Mô hình dữ liệu tổng hợp thông tin lượt xe đang đỗ và kết quả tính tiền để hiển thị ở Làn Ra
/// </summary>
public class CheckOutPreviewModel
{
    public int SessionId { get; set; }
    public string CardCode { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string VehicleTypeId { get; set; } = string.Empty;
    public string VehicleTypeName { get; set; } = string.Empty;
    public string SlotCode { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public DateTime CheckOutTime { get; set; }
    public string? CheckInImagePath { get; set; }
    public string DisplaySearchText { get; set; } = string.Empty;
    public FeeCalculationResult FeeResult { get; set; } = new FeeCalculationResult();
}

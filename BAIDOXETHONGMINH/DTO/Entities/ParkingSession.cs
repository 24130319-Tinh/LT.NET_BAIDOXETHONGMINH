namespace BAIDOXETHONGMINH.DTO.Entities;

/// <summary>
/// Bảng ParkingSessions: Lưu thông tin mỗi lượt xe vào / ra bãi
/// Status: "Parking" (Đang gửi trong bãi), "Completed" (Đã lấy xe ra)
/// </summary>
public class ParkingSession
{
    public int SessionId { get; set; }
    public string CardCode { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string VehicleTypeId { get; set; } = string.Empty;
    public string SlotCode { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; } = DateTime.Now;
    public DateTime? CheckOutTime { get; set; }
    public string? CheckInImagePath { get; set; }
    public string? CheckOutImagePath { get; set; }
    public int CheckInUserId { get; set; }
    public int? CheckOutUserId { get; set; }
    public bool IsMonthlyPass { get; set; }
    public decimal ParkingFee { get; set; }
    public decimal PenaltyFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Parking";
}

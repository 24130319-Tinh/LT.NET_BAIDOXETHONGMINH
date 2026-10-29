namespace BAIDOXETHONGMINH.DTO.Entities;

/// <summary>
/// Bảng ParkingCards: Danh mục thẻ/vé xe
/// Status: "Available" (Sẵn sàng), "InUse" (Đang cấp cho xe), "Lost" (Bị mất), "Locked" (Khóa)
/// </summary>
public class ParkingCard
{
    public string CardCode { get; set; } = string.Empty;
    public string? LicensePlate { get; set; }
    public string Status { get; set; } = "Available";
}

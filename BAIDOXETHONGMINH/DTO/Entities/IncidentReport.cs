namespace BAIDOXETHONGMINH.DTO.Entities;

/// <summary>
/// Bảng IncidentReports: Biên bản sự cố / mất thẻ xe
/// IncidentType: "LostCard" (Mất thẻ), "Damage" (Va quệt/Hư hỏng), "Other" (Khác)
/// </summary>
public class IncidentReport
{
    public int IncidentId { get; set; }
    public int? SessionId { get; set; }
    public string CardCode { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerIdentityCard { get; set; } = string.Empty;
    public string IncidentType { get; set; } = "LostCard";
    public decimal CompensationFee { get; set; }
    public string? Note { get; set; }
    public int HandledByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

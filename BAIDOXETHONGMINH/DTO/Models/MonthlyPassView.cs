namespace BAIDOXETHONGMINH.Models;

/// <summary>Một dòng vé tháng kèm tên chủ xe, dùng để hiển thị trên DataGridView (không phải bảng CSDL).</summary>
public class MonthlyPassView
{
    public int PassId { get; set; }
    public int CustomerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string VehicleTypeId { get; set; } = string.Empty;
    public string VehicleTypeName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Active";

    // ----- Phần hiển thị -----
    public string Code => $"MP-{PassId:00}";
    public string OwnerText => $"{OwnerName} ({Phone})";
    public string VehicleText => string.IsNullOrWhiteSpace(Brand) ? VehicleTypeName : $"{VehicleTypeName} • {Brand}";
    public string PeriodText => $"{StartDate:dd/MM/yyyy} → {EndDate:dd/MM/yyyy}";
    public string EndDateText => EndDate.ToString("dd/MM/yyyy");

    /// <summary>ACTIVE (còn hạn), EXPIRING (còn dưới 7 ngày), EXPIRED (hết hạn), CANCELLED (đã hủy).</summary>
    public string State
    {
        get
        {
            if (Status == "Cancelled") return "CANCELLED";
            int daysLeft = (EndDate.Date - DateTime.Today).Days;
            if (Status == "Expired" || daysLeft < 0) return "EXPIRED";
            if (daysLeft <= 7) return "EXPIRING";
            return "ACTIVE";
        }
    }

    public string StatusText
    {
        get
        {
            if (State == "CANCELLED") return "Đã hủy vé";
            if (State == "EXPIRED") return "Đã hết hạn";
            if (State == "EXPIRING") return $"Sắp hết hạn ({(EndDate.Date - DateTime.Today).Days} ngày)";
            return "Còn hạn";
        }
    }
}

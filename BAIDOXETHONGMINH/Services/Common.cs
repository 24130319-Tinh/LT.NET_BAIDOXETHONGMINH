using System.Globalization;

namespace BAIDOXETHONGMINH.GUI;

/// <summary>Lỗi nghiệp vụ tự định nghĩa. Form bắt và hiện ex.Message cho người dùng.</summary>
public class ServiceException : Exception
{
    public ServiceException(string message) : base(message) { }
}

public static class PassStatus
{
    public const string Active = "Active";
    public const string Expired = "Expired";
    public const string Cancelled = "Cancelled";
}

public static class CardStatus
{
    public const string Available = "Available";
    public const string InUse = "InUse";
    public const string Lost = "Lost";
    public const string Locked = "Locked";
}

public static class SlotStatus
{
    public const string Available = "Available";
    public const string Occupied = "Occupied";
    public const string Maintenance = "Maintenance";
}

/// <summary>Giá trị cột TransactionType của bảng PaymentTransactions (theo mô tả trong entity PaymentTransaction).</summary>
public static class PaymentType
{
    public const string MonthlyPassNew = "MonthlyPassNew";
    public const string MonthlyPassRenew = "MonthlyPassRenew";
}

public static class PlateHelper
{
    /// <summary>Biển số lưu vào CSDL: bỏ khoảng trắng hai đầu, viết hoa (giữ dấu - và . để hiển thị đẹp).</summary>
    public static string Clean(string? plate) => (plate ?? "").Trim().ToUpperInvariant();

    /// <summary>Khóa so sánh: chỉ giữ chữ và số. "59k1-234.56" và "59K1 23456" cho cùng một khóa.</summary>
    public static string Key(string? plate) =>
        new string(Clean(plate).Where(c => char.IsLetterOrDigit(c)).ToArray());

    public static bool Same(string? a, string? b)
    {
        string keyA = Key(a);
        return keyA.Length > 0 && keyA == Key(b);
    }
}

/// <summary>Định dạng hiển thị tiền, ngày, trạng thái thẻ.</summary>
public static class Fmt
{
    private static readonly CultureInfo Vi = new("vi-VN");

    public static string Money(decimal value) => value.ToString("N0", Vi) + "đ";

    /// <summary>5.000 (số có dấu chấm ngăn cách hàng nghìn, không có đơn vị).</summary>
    public static string Number(decimal value) => value.ToString("N0", Vi);

    /// <summary>150.000 VNĐ (dùng cho ô hiển thị phí).</summary>
    public static string Vnd(decimal value) => value.ToString("N0", Vi) + " VNĐ";

    /// <summary>150k, 1.200k (dùng cho bảng giá rút gọn).</summary>
    public static string MoneyK(decimal value) => (value / 1000).ToString("N0", Vi) + "k";

    public static string Date(DateTime value) => value.ToString("dd/MM/yyyy");

    public static string CardText(string status)
    {
        if (status == CardStatus.Available) return "Sẵn sàng";
        if (status == CardStatus.InUse) return "Đang cấp cho xe";
        if (status == CardStatus.Lost) return "Bị mất";
        if (status == CardStatus.Locked) return "Đã khóa";
        return status;
    }
}

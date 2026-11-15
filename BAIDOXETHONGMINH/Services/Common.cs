using System.Globalization;

namespace BAIDOXETHONGMINH.GUI;

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

public static class PaymentType
{
    public const string MonthlyPassNew = "MonthlyPassNew";
    public const string MonthlyPassRenew = "MonthlyPassRenew";
}

public static class PlateHelper
{
    public static string Clean(string? plate) => (plate ?? "").Trim().ToUpperInvariant();

    public static string Key(string? plate) =>
        new string(Clean(plate).Where(c => char.IsLetterOrDigit(c)).ToArray());

    public static bool Same(string? a, string? b)
    {
        string keyA = Key(a);
        return keyA.Length > 0 && keyA == Key(b);
    }
}

public static class Fmt
{
    private static readonly CultureInfo Vi = new("vi-VN");

    public static string Money(decimal value) => value.ToString("N0", Vi) + "đ";

    public static string Number(decimal value) => value.ToString("N0", Vi);

    public static string Vnd(decimal value) => value.ToString("N0", Vi) + " VNĐ";

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

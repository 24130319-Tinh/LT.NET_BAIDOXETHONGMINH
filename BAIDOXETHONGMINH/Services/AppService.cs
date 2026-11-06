using BAIDOXETHONGMINH.DAL.Database;

namespace BAIDOXETHONGMINH.GUI;

public class AppService
{
    public static PricingService Pricing { get;}=new ();
    public static ParkingCardService Card { get; } = new();
    public static ParkingMapService Map { get;}=new ();
    public static CustomerService Customers { get;}=new ();
    public static MonthlyPassService Monthly { get;}=new (Pricing);

    /// <summary>Id người đang đăng nhập. Form đăng nhập gán sau khi đăng nhập thành công (AppService.CurrentUserId = user.UserId).
    /// Khi có giá trị, đăng ký/gia hạn vé tháng sẽ ghi thêm một dòng vào PaymentTransactions.</summary>
    public static int? CurrentUserId { get; set; }

    /// <summary>true: lần chạy đầu tự nạp danh mục mẫu cho NV3, NV4 (loại xe, bảng giá, khu A/B, ô, thẻ). Đặt false để tắt.</summary>
    public static bool SeedSampleData { get; set; } = true;

    private static bool _ready;

    /// <summary>Gọi ở sự kiện Load của mỗi Form (gọi nhiều lần vẫn an toàn). Cấu trúc 12 bảng đã do SqliteDbContext.InitializeDatabase() tạo.</summary>
    public static async Task InitializeAsync()
    {
        if (_ready) return;
        if (SeedSampleData) await SampleData.SeedIfEmptyAsync();
        _ready = true;
    }
}

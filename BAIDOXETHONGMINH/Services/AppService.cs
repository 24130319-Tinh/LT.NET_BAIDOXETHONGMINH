using BAIDOXETHONGMINH.DAL.Database;

namespace BAIDOXETHONGMINH.GUI;

public class AppService
{
    public static PricingService Pricing { get; } = new PricingService();
    public static ParkingCardService Card { get; } = new ParkingCardService();
    public static ParkingMapService Map { get; } = new ParkingMapService();
    public static CustomerService Customers { get; } = new CustomerService();
    public static MonthlyPassService Monthly { get; } = new MonthlyPassService(Pricing);

    public static int? CurrentUserId { get; set; }

    public static bool SeedSampleData { get; set; } = true;

    private static bool _ready;
    private static readonly SemaphoreSlim InitializationLock = new SemaphoreSlim(1, 1);

    // Hàm khởi tạo CSDL và xác định mã nhân viên đang hoạt động mặc định cho toàn bộ phiên làm việc
    public static async Task InitializeAsync()
    {
        await InitializationLock.WaitAsync();
        try
        {
            if (_ready) return;
            await SqliteDbContext.InitializeDatabaseAsync();
            if (!CurrentUserId.HasValue || CurrentUserId.Value <= 0)
            {
                long resolvedUserId = await SqliteHelper.ScalarAsync(
                    "SELECT COALESCE((SELECT UserId FROM Users WHERE IsActive = 1 ORDER BY CASE WHEN Username = 'admin' THEN 0 ELSE 1 END, UserId LIMIT 1), 1);"
                );
                CurrentUserId = resolvedUserId > 0 ? (int)resolvedUserId : 1;
            }
            _ready = true;
        }
        finally
        {
            InitializationLock.Release();
        }
    }
}

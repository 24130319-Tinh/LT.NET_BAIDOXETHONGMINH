using BAIDOXETHONGMINH.DAL.Database;

namespace BAIDOXETHONGMINH.GUI;

public class AppService
{
    public static PricingService Pricing { get;}=new ();
    public static ParkingCardService Card { get; } = new();
    public static ParkingMapService Map { get;}=new ();
    public static CustomerService Customers { get;}=new ();
    public static MonthlyPassService Monthly { get;}=new (Pricing);

    public static int? CurrentUserId { get; set; }

    public static bool SeedSampleData { get; set; } = true;

    private static bool _ready;
    private static readonly SemaphoreSlim InitializationLock = new(1, 1);

    public static async Task InitializeAsync()
    {
        await InitializationLock.WaitAsync();
        try
        {
            if (_ready) return;
            await SqliteDbContext.InitializeDatabaseAsync();
            if (SeedSampleData) await SampleData.SeedIfEmptyAsync();
            CurrentUserId ??= (int)await SqliteHelper.ScalarAsync("SELECT UserId FROM Users WHERE Username = 'local'");
            _ready = true;
        }
        finally { InitializationLock.Release(); }
    }
}

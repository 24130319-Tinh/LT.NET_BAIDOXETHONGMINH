using BAIDOXETHONGMINH.GUI;

namespace BAIDOXETHONGMINH;

static class Program
{
    // Hàm khởi chạy chính của ứng dụng, khởi tạo CSDL và mở màn hình MainLayout
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        SqliteDbContext.InitializeDatabase();

        bool isConnected = DatabaseConnectionTester.IsConnectionAvailable();
        if (!isConnected)
        {
            AppService.InitializeAsync().GetAwaiter().GetResult();
            Application.Run(new MainLayout());
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không khởi tạo được dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

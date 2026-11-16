using BAIDOXETHONGMINH.DAL.Database;
using BAIDOXETHONGMINH.GUI;

namespace BAIDOXETHONGMINH;

static class Program
{
    // Hàm khởi chạy chính của ứng dụng, khởi tạo CSDL, phiên người dùng mặc định và mở màn hình MainLayout
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        SqliteDbContext.InitializeDatabase();

        bool isConnected = DatabaseConnectionTester.IsConnectionAvailable();
        if (!isConnected)
        {
            MessageBox.Show(
                "Không thể kết nối tới cơ sở dữ liệu smart_parking.db!",
                "Lỗi Kết nối CSDL",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        try
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

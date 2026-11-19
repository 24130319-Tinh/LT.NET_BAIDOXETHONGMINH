using BAIDOXETHONGMINH.DAL.Database;
using BAIDOXETHONGMINH.GUI;

namespace BAIDOXETHONGMINH;

static class Program
{
    // Hàm khởi chạy chính của ứng dụng, khởi tạo CSDL và điều phối vòng lặp Đăng nhập -> MainLayout -> Đăng xuất
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

            while (true)
            {
                using Form1 loginForm = new Form1();
                DialogResult loginResult = loginForm.ShowDialog();
                if (loginResult != DialogResult.OK)
                {
                    break;
                }

                using MainLayout mainLayout = new MainLayout();
                Application.Run(mainLayout);
                if (!mainLayout.IsLogoutRequested)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không khởi tạo được dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

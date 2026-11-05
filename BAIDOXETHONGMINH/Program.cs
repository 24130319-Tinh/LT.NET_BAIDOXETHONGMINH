using BAIDOXETHONGMINH.DAL.Database;
using BAIDOXETHONGMINH.GUI;

namespace BAIDOXETHONGMINH;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Đảm bảo cấu trúc 12 bảng trong DAL/Database/smart_parking.db
        SqliteDbContext.InitializeDatabase();

        // Kiểm tra kết nối tồn tại trước khi mở ứng dụng
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

        Application.Run(new MainLayout());
    }
}

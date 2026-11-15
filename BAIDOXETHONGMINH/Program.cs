using BAIDOXETHONGMINH.GUI;

namespace BAIDOXETHONGMINH;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
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

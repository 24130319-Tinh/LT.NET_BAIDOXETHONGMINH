namespace BAIDOXETHONGMINH.GUI;
using BAIDOXETHONGMINH.Services;
//class nay để dùng chạy thông báo

public static class Dialogs
{
    public static async Task<bool> RunAsync(Func<Task> action)
    {
        try
        {
            await action();
            return true;
        }
        catch (ServiceException ex)
        {
            MessageBox.Show(ex.Message, "Không thực hiện được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi hệ thống hoặc Database: " + ex.Message, "Lỗi", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        return false;

    } 
}
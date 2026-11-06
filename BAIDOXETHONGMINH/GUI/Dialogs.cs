namespace BAIDOXETHONGMINH.GUI;

/// <summary>Hàm dùng chung cho các Form: chạy thao tác nghiệp vụ có bắt lỗi và hộp thoại nhập liệu.</summary>
public static class Dialogs
{
    /// <summary>Chạy thao tác; lỗi nghiệp vụ hiện thành hộp thoại thay vì làm chương trình sập. Trả về true nếu thành công.</summary>
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
            MessageBox.Show("Lỗi hệ thống hoặc CSDL: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        return false;
    }

    /// <summary>Giống RunAsync nhưng trả về kết quả; trả null nếu có lỗi.</summary>
    public static async Task<T?> TryAsync<T>(Func<Task<T>> action) where T : class
    {
        try
        {
            return await action();
        }
        catch (ServiceException ex)
        {
            MessageBox.Show(ex.Message, "Không thực hiện được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi hệ thống hoặc CSDL: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        return null;
    }

    /// <summary>Hộp thoại nhập nhiều trường (nhãn, giá trị mặc định). Trả về null nếu bấm Hủy.</summary>
    public static string[]? Prompt(IWin32Window owner, string title, params (string Label, string Value)[] fields)
    {
        using var dialog = new Form
        {
            Text = title,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            ClientSize = new Size(360, 24 + fields.Length * 34 + 56)
        };

        var boxes = new TextBox[fields.Length];
        for (int i = 0; i < fields.Length; i++)
        {
            dialog.Controls.Add(new Label { Text = fields[i].Label, Left = 12, Top = 24 + i * 34 + 3, Width = 170 });
            boxes[i] = new TextBox { Left = 185, Top = 24 + i * 34, Width = 160, Text = fields[i].Value };
            dialog.Controls.Add(boxes[i]);
        }

        int y = 24 + fields.Length * 34 + 8;
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 180, Top = y, Width = 80 };
        var cancel = new Button { Text = "Hủy", DialogResult = DialogResult.Cancel, Left = 265, Top = y, Width = 80 };
        dialog.Controls.Add(ok);
        dialog.Controls.Add(cancel);
        dialog.AcceptButton = ok;
        dialog.CancelButton = cancel;

        if (dialog.ShowDialog(owner) != DialogResult.OK) return null;
        return boxes.Select(b => b.Text).ToArray();
    }
}

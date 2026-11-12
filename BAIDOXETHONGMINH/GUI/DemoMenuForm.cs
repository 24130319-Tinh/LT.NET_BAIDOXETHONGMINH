namespace BAIDOXETHONGMINH.GUI;

public class DemoMenuForm : Form
{
    public DemoMenuForm()
    {
        Text = "Bãi đỗ xe thông minh - Nghiệp vụ 3 và 4";
        ClientSize = new Size(420, 300);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        Controls.Add(new Label
        {
            Text = "NGHIỆP VỤ 3 VÀ 4",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            AutoSize = true,
            Left = 24,
            Top = 18
        });

        AddButton("Sơ đồ bãi 2D, bảng giá, thẻ xe  (NV3)", 62, () => new ParkingMapForm().Show());
        AddButton("Đăng ký / Gia hạn vé tháng  (NV4)", 112, () => new MonthlyPassForm(0).Show());
        AddButton("Quản lý khách hàng  (NV4)", 162, () => new CustomerForm().Show());
        AddButton("Gia hạn & cập nhật vé  (NV4, tab 3.2)", 212, () => new MonthlyPassForm(1).Show());
    }

    private void AddButton(string text, int top, Action open)
    {
        var button = new Button { Text = text, Left = 24, Top = top, Width = 372, Height = 40 };
        button.Click += (_, _) => open();
        Controls.Add(button);
    }
}

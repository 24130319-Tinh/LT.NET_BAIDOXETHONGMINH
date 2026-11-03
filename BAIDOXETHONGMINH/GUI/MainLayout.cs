namespace BAIDOXETHONGMINH.GUI;

/// <summary>
/// MainLayout đóng vai trò khung bố cục chính:
/// - Bên trái (pnlLeftMenu): Nhúng component MenuForm
/// - Bên phải (pnlRightContent): Hiển thị Form tương ứng với mục được chọn trên Menu (Mặc định mục 1 là CheckInOutForm)
/// </summary>
public partial class MainLayout : Form
{
    private readonly MenuForm _menuForm;
    private Form? _currentRightForm;

    public MainLayout()
    {
        InitializeComponent();

        // 1. Khởi tạo và nhúng component MenuForm vào khung bên trái
        _menuForm = new MenuForm();
        EmbedFormIntoPanel(_menuForm, pnlLeftMenu);

        // 2. Lắng nghe sự kiện khi người dùng bấm chọn mục trên MenuForm
        _menuForm.MenuItemSelected += OnMenuItemSelected;

        // 3. Đầu tiên khi mở app: chọn Mục 1 và hiển thị CheckInOutForm
        OpenRightComponent(new CheckInOutForm());
    }

    private void OnMenuItemSelected(string menuKey)
    {
        Form targetForm = menuKey switch
        {
            "CheckInOut"      => new CheckInOutForm(),
            "ParkingMap"      => new ParkingMapForm(),
            "MonthlyRegister" => new MonthlyPassForm(),
            "Customer"        => new CustomerForm(),
            "MonthlyRenew"    => CreateEmptyPlaceholderForm("3.2 GIA HẠN & CẬP NHẬT VÉ THÁNG (Đang phát triển)"),
            "SearchReport"    => CreateEmptyPlaceholderForm("5. TRA CỨU & THỐNG KÊ DOANH THU (Đang phát triển)"),
            _                 => CreateEmptyPlaceholderForm(menuKey)
        };

        OpenRightComponent(targetForm);
    }

    /// <summary>
    /// Nhúng một Form component vào khung nội dung bên phải (pnlRightContent)
    /// </summary>
    public void OpenRightComponent(Form childForm)
    {
        if (_currentRightForm != null)
        {
            _currentRightForm.Close();
            _currentRightForm.Dispose();
            _currentRightForm = null;
        }

        _currentRightForm = childForm;
        EmbedFormIntoPanel(childForm, pnlRightContent);
    }

    /// <summary>
    /// Hàm dùng chung để tái sử dụng (nhúng) bất kỳ Form nào vào bên trong một Panel
    /// </summary>
    private static void EmbedFormIntoPanel(Form formComponent, Panel containerPanel)
    {
        containerPanel.Controls.Clear();

        formComponent.TopLevel = false;
        formComponent.FormBorderStyle = FormBorderStyle.None;
        formComponent.Dock = DockStyle.Fill;

        containerPanel.Controls.Add(formComponent);
        containerPanel.Tag = formComponent;
        formComponent.Show();
    }

    private static Form CreateEmptyPlaceholderForm(string title)
    {
        var form = new Form { BackColor = Color.White };
        var lbl = new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 116, 139)
        };
        form.Controls.Add(lbl);
        return form;
    }
}

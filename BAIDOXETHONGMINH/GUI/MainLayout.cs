namespace BAIDOXETHONGMINH.GUI;

/// <summary>
/// MainLayout đóng vai trò khung bố cục chính:
/// - Bên trái (pnlLeftMenu): Nhúng component MenuForm
/// - Bên phải (pnlRightContent): Hiển thị Form tương ứng với mục được chọn trên Menu
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
            "MonthlyRenew"    => new MonthlyRenewForm(),
            "Customer"        => new CustomerForm(),
            "SearchReport"    => new SearchReportForm(),
            _                 => new CheckInOutForm()
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
}

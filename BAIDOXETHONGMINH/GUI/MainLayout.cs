namespace BAIDOXETHONGMINH.GUI;

public partial class MainLayout : Form
{
    private readonly MenuForm _menuForm;
    private Form? _currentRightForm;

    // Hàm khởi tạo khung bố cục chính MainLayout, nhúng Menu bên trái và mở CheckInOutForm bên phải
    public MainLayout()
    {
        InitializeComponent();

        _menuForm = new MenuForm();
        EmbedFormIntoPanel(_menuForm, pnlLeftMenu);

        _menuForm.MenuItemSelected += OnMenuItemSelected;

        OpenRightComponent(new CheckInOutForm());
    }

    // Hàm điều hướng mở Form chức năng tương ứng khi người dùng chọn mục trên Menu
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

    // Hàm đóng Form cũ và nhúng Form chức năng mới vào vùng nội dung bên phải
    public void OpenRightComponent(Form childForm)
    {
        if (_currentRightForm != null)
        {
            if (_currentRightForm is CheckInOutForm oldCheckInForm)
            {
                oldCheckInForm.SlotCapacityChanged -= OnSlotCapacityChanged;
            }

            _currentRightForm.Close();
            _currentRightForm.Dispose();
            _currentRightForm = null;
        }

        if (childForm is CheckInOutForm newCheckInForm)
        {
            newCheckInForm.SlotCapacityChanged += OnSlotCapacityChanged;
        }

        _currentRightForm = childForm;
        EmbedFormIntoPanel(childForm, pnlRightContent);
    }

    // Hàm nhận sự kiện thay đổi số chỗ trống từ CheckInOutForm và cập nhật lên Sidebar
    private void OnSlotCapacityChanged(int availableSlots, int totalSlots)
    {
        _menuForm.UpdateSlotCapacity(availableSlots, totalSlots);
    }

    // Hàm nhúng một Form con vào bên trong một Panel chứa
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

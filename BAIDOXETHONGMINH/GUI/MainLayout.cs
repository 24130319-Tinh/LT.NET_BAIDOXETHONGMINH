namespace BAIDOXETHONGMINH.GUI;

public partial class MainLayout : Form
{
    private readonly MenuForm _menuForm;
    private Form? _currentRightForm;

    public MainLayout()
    {
        InitializeComponent();

        _menuForm = new MenuForm();
        EmbedFormIntoPanel(_menuForm, pnlLeftMenu);

        _menuForm.MenuItemSelected += OnMenuItemSelected;

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

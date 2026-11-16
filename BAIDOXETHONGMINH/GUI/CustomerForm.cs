using BAIDOXETHONGMINH.DTO.Entities;
namespace BAIDOXETHONGMINH.GUI;

public partial class CustomerForm : Form
{
    public CustomerForm()
    {
        InitializeComponent();

        RoundedUiHelper.AttachRoundedFrame(pnlLeftForm, 16, RoundedUiHelper.CardBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlRightGrid, 16, RoundedUiHelper.CardBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlFullNameBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlPhoneBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlCccdBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlSearchBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(btnAddCustomer, 10, Color.FromArgb(37, 99, 235));
        RoundedUiHelper.AttachRoundedFrame(btnUpdateCustomer, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(btnDeleteCustomer, 10, Color.FromArgb(248, 113, 113));

        SetupCustomerGrid();
    }

    private readonly CustomerService _service = AppService.Customers;
    private bool _saving;
    private int _searchVersion;

    private void SetupCustomerGrid()
    {
        dgvCustomers.Columns.Add("CustomerId", "Mã KH");
        dgvCustomers.Columns.Add("FullName", "Họ và tên");
        dgvCustomers.Columns.Add("Phone", "Số điện thoại");
        dgvCustomers.Columns.Add("IdentityCard", "Số CCCD");
        dgvCustomers.Columns.Add("CreatedAt", "Ngày đăng ký");
        Load += async (_, _) => await Dialogs.RunAsync(async () =>
        {
            await AppService.InitializeAsync();
            await RefreshCustomersAsync();
        });
        txtSearchCustomer.TextChanged += async (_, _) => await Dialogs.RunAsync(RefreshCustomersAsync);
    }

    private Customer? SelectedCustomer => dgvCustomers.CurrentRow?.Tag as Customer;

    private async Task RefreshCustomersAsync()
    {
        int version = ++_searchVersion;
        int? selectedId = SelectedCustomer?.CustomerId;
        var customers = await _service.SearchAsync(txtSearchCustomer.Text);
        if (IsDisposed || version != _searchVersion) return;
        dgvCustomers.Rows.Clear();
        foreach (var customer in customers)
        {
            int index = dgvCustomers.Rows.Add($"KH-{customer.CustomerId:00}", customer.FullName,
                customer.Phone, customer.IdentityCard, Fmt.Date(customer.CreatedAt));
            dgvCustomers.Rows[index].Tag = customer;
        }
        var row = dgvCustomers.Rows.Cast<DataGridViewRow>().FirstOrDefault(r =>
            (r.Tag as Customer)?.CustomerId == selectedId) ?? dgvCustomers.Rows.Cast<DataGridViewRow>().FirstOrDefault();
        if (row != null) dgvCustomers.CurrentCell = row.Cells[0];
        DgvCustomers_SelectionChanged(null, EventArgs.Empty);
    }

    private void DgvCustomers_SelectionChanged(object? sender, EventArgs e)
    {
        var customer = SelectedCustomer;
        if (customer == null) return;
        txtFullName.Text = customer.FullName;
        txtPhone.Text = customer.Phone;
        txtCccd.Text = customer.IdentityCard;
    }

    private async Task SaveAsync(Func<Task> action)
    {
        if (_saving) return;
        _saving = true;
        btnAddCustomer.Enabled = btnUpdateCustomer.Enabled = btnDeleteCustomer.Enabled = false;
        try { await Dialogs.RunAsync(async () => { await action(); await RefreshCustomersAsync(); }); }
        finally
        {
            _saving = false;
            if (!IsDisposed) btnAddCustomer.Enabled = btnUpdateCustomer.Enabled = btnDeleteCustomer.Enabled = true;
        }
    }

    private async void BtnAddCustomer_Click(object? sender, EventArgs e) => await SaveAsync(() =>
        _service.AddAsync(new Customer { FullName = txtFullName.Text, Phone = txtPhone.Text, IdentityCard = txtCccd.Text }));

    private async void BtnUpdateCustomer_Click(object? sender, EventArgs e)
    {
        var customer = SelectedCustomer;
        if (customer == null) return;
        await SaveAsync(() => _service.UpdateAsync(new Customer
        {
            CustomerId = customer.CustomerId, FullName = txtFullName.Text,
            Phone = txtPhone.Text, IdentityCard = txtCccd.Text
        }));
    }

    private async void BtnDeleteCustomer_Click(object? sender, EventArgs e)
    {
        var customer = SelectedCustomer;
        if (customer == null) return;
        await SaveAsync(() => _service.DeleteAsync(customer.CustomerId));
    }
}
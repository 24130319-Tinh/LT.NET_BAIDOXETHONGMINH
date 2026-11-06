using BAIDOXETHONGMINH.DTO.Entities;

namespace BAIDOXETHONGMINH.GUI;

/// <summary>NV4: quản lý khách hàng (thêm, sửa, xóa, tìm kiếm).</summary>
public class CustomerForm : Form
{
    private readonly CustomerService _service = AppService.Customers;

    private readonly TextBox _txtSearch = new() { Dock = DockStyle.Top, PlaceholderText = "Tìm theo họ tên, SĐT hoặc CCCD..." };
    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        RowHeadersVisible = false,
        MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoGenerateColumns = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        BackgroundColor = SystemColors.Window
    };
    private readonly TextBox _txtName = new();
    private readonly TextBox _txtPhone = new();
    private readonly TextBox _txtCccd = new();
    private int _selectedId;
    private bool _loading;

    public CustomerForm()
    {
        Text = "Quản lý khách hàng";
        Size = new Size(960, 560);
        MinimumSize = new Size(760, 440);
        StartPosition = FormStartPosition.CenterScreen;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã", DataPropertyName = "CustomerId", FillWeight = 20 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Họ tên", DataPropertyName = "FullName", FillWeight = 60 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Số điện thoại", DataPropertyName = "Phone", FillWeight = 40 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CCCD", DataPropertyName = "IdentityCard", FillWeight = 45 });

        // Bên trái: tìm kiếm + bảng. Bên phải: form nhập
        var left = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        left.Controls.Add(_grid);
        left.Controls.Add(_txtSearch);

        var right = new Panel { Dock = DockStyle.Right, Width = 320, Padding = new Padding(10) };
        var group = new GroupBox { Text = "Thông tin khách hàng", Dock = DockStyle.Fill };
        int y = 28;
        AddField(group, "Họ tên (*)", _txtName, ref y);
        AddField(group, "Số điện thoại (*)", _txtPhone, ref y);
        AddField(group, "Số CCCD (*)", _txtCccd, ref y);
        AddButton(group, "Thêm mới", 16, y + 6, async () => await AddAsync());
        AddButton(group, "Lưu sửa", 160, y + 6, async () => await UpdateAsync());
        AddButton(group, "Xóa", 16, y + 46, async () => await DeleteAsync());
        AddButton(group, "Làm mới ô nhập", 160, y + 46, () => { ClearInput(); return Task.CompletedTask; });
        right.Controls.Add(group);

        Controls.Add(left);
        Controls.Add(right);

        _txtSearch.TextChanged += async (_, _) => await LoadAsync();
        _grid.SelectionChanged += (_, _) => ShowSelected();
        Load += async (_, _) =>
        {
            if (await Dialogs.RunAsync(AppService.InitializeAsync)) await LoadAsync();
        };
    }

    private static void AddField(Control parent, string label, TextBox box, ref int y)
    {
        parent.Controls.Add(new Label { Text = label, Left = 16, Top = y, Width = 270 });
        box.Left = 16;
        box.Top = y + 20;
        box.Width = 270;
        parent.Controls.Add(box);
        y += 56;
    }

    private static void AddButton(Control parent, string text, int left, int top, Func<Task> onClick)
    {
        var button = new Button { Text = text, Left = left, Top = top, Width = 130, Height = 32 };
        button.Click += async (_, _) => await onClick();
        parent.Controls.Add(button);
    }

    private async Task LoadAsync()
    {
        await Dialogs.RunAsync(async () =>
        {
            var customers = await _service.SearchAsync(_txtSearch.Text);
            _loading = true;
            _grid.DataSource = customers;
            _loading = false;
        });
    }

    private void ShowSelected()
    {
        if (_loading) return;
        if (_grid.CurrentRow?.DataBoundItem is not Customer c) return;
        _selectedId = c.CustomerId;
        _txtName.Text = c.FullName;
        _txtPhone.Text = c.Phone;
        _txtCccd.Text = c.IdentityCard;
    }

    private void ClearInput()
    {
        _selectedId = 0;
        _txtName.Clear();
        _txtPhone.Clear();
        _txtCccd.Clear();
        _loading = true;            // tránh SelectionChanged điền lại ô nhập
        _grid.ClearSelection();
        _loading = false;
    }

    private Customer ReadInput() => new()
    {
        CustomerId = _selectedId,
        FullName = _txtName.Text,
        Phone = _txtPhone.Text,
        IdentityCard = _txtCccd.Text
    };

    private async Task AddAsync()
    {
        var customer = ReadInput();
        customer.CustomerId = 0;
        if (await Dialogs.RunAsync(() => _service.AddAsync(customer)))
        {
            ClearInput();
            await LoadAsync();
        }
    }

    private async Task UpdateAsync()
    {
        if (_selectedId == 0)
        {
            MessageBox.Show("Hãy chọn một khách hàng trong bảng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var customer = ReadInput();
        if (await Dialogs.RunAsync(() => _service.UpdateAsync(customer)))
            await LoadAsync();
    }

    private async Task DeleteAsync()
    {
        if (_selectedId == 0)
        {
            MessageBox.Show("Hãy chọn một khách hàng trong bảng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var answer = MessageBox.Show("Xóa khách hàng đang chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer != DialogResult.Yes) return;

        int id = _selectedId;
        if (await Dialogs.RunAsync(() => _service.DeleteAsync(id)))
        {
            ClearInput();
            await LoadAsync();
        }
    }
}

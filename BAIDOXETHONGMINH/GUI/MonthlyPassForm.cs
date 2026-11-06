using BAIDOXETHONGMINH.Models;

namespace BAIDOXETHONGMINH.GUI;

/// <summary>
/// NV4: quản lý vé tháng gồm 2 tab theo bản thiết kế UI:
/// 3.1 Đăng ký vé tháng mới, 3.2 Gia hạn &amp; cập nhật vé. tabIndex = 0 mở tab 3.1, 1 mở tab 3.2.
/// </summary>
public class MonthlyPassForm : Form
{
    private record MonthOption(int Months, string Text);
    private record FilterOption(string Value, string Text);

    private readonly MonthlyPassService _service = AppService.Monthly;
    private readonly PricingService _pricing = AppService.Pricing;
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };

    // ----- Tab 3.1 Đăng ký mới -----
    private readonly TextBox _regName = new();
    private readonly TextBox _regPhone = new();
    private readonly TextBox _regCccd = new();
    private readonly TextBox _regPlate = new();
    private readonly ComboBox _regType = NewCombo();
    private readonly TextBox _regBrand = new();
    private readonly TextBox _regStart = new() { ReadOnly = true };
    private readonly ComboBox _regMonths = NewCombo();
    private readonly Label _regFee = NewBoldLabel();
    private readonly Label _regToast = new() { AutoSize = false, Height = 44, ForeColor = Color.SeaGreen };
    private readonly DataGridView _gridAll = NewGrid();

    // ----- Tab 3.2 Gia hạn & cập nhật -----
    private readonly TextBox _search = new() { PlaceholderText = "Gõ biển số hoặc tên khách hàng...", Width = 280 };
    private readonly ComboBox _filter = NewCombo();
    private readonly DataGridView _gridRenew = NewGrid();
    private readonly Label _renCode = NewBoldLabel();
    private readonly Label _renOwner = NewBoldLabel();
    private readonly Label _renOldEnd = NewBoldLabel();
    private readonly Label _renNewEnd = NewBoldLabel();
    private readonly Label _renFee = NewBoldLabel();
    private readonly ComboBox _renMonths = NewCombo();
    private readonly TextBox _updPhone = new();
    private readonly TextBox _updBrand = new();
    private readonly Label _renToast = new() { AutoSize = false, Height = 44, ForeColor = Color.SeaGreen };

    private MonthlyPassView? _selected;
    private bool _loading;

    public MonthlyPassForm(int tabIndex = 0)
    {
        Text = "Quản lý Vé tháng";
        Size = new Size(1000, 680);
        MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;

        var tabRegister = new TabPage("3.1 Đăng ký Vé tháng mới");
        var tabRenew = new TabPage("3.2 Gia hạn && Cập nhật vé");
        BuildRegisterTab(tabRegister);
        BuildRenewTab(tabRenew);
        _tabs.TabPages.Add(tabRegister);
        _tabs.TabPages.Add(tabRenew);
        _tabs.SelectedIndex = tabIndex;
        Controls.Add(_tabs);

        _gridAll.CellFormatting += ColorStatus;
        _gridRenew.CellFormatting += ColorStatus;
        _gridRenew.SelectionChanged += async (_, _) =>
        {
            if (!_loading) await ShowSelectedAsync();
        };
        _regType.SelectedIndexChanged += async (_, _) => await UpdateRegFeeAsync();
        _regMonths.SelectedIndexChanged += async (_, _) => await UpdateRegFeeAsync();
        _renMonths.SelectedIndexChanged += async (_, _) =>
        {
            if (!_loading) await UpdateRenewPreviewAsync();
        };
        _search.TextChanged += async (_, _) => await Dialogs.RunAsync(RefreshGridsAsync);
        _filter.SelectedIndexChanged += async (_, _) =>
        {
            if (!_loading) await Dialogs.RunAsync(RefreshGridsAsync);
        };

        Load += async (_, _) => await LoadAllAsync();
    }

    // =====================================================================
    // Dựng giao diện
    // =====================================================================
    private static ComboBox NewCombo() => new() { DropDownStyle = ComboBoxStyle.DropDownList };

    private static Label NewBoldLabel() => new()
    {
        AutoSize = false,
        Height = 22,
        Font = new Font("Segoe UI", 9F, FontStyle.Bold)
    };

    private static DataGridView NewGrid() => new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        RowHeadersVisible = false,
        MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoGenerateColumns = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        BackgroundColor = SystemColors.Window
    };

    private static void AddColumns(DataGridView grid, params (string Header, string Property, float Weight)[] columns)
    {
        foreach (var (header, property, weight) in columns)
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = header, DataPropertyName = property, FillWeight = weight });
    }

    /// <summary>Nhãn ở trên, ô nhập ở dưới (như bản thiết kế UI).</summary>
    private static void AddField(Control parent, string label, Control input, ref int y)
    {
        parent.Controls.Add(new Label { Text = label, Left = 16, Top = y, Width = 310, Height = 18, UseMnemonic = false });
        input.Left = 16;
        input.Top = y + 19;
        input.Width = 310;
        parent.Controls.Add(input);
        y += 52;
    }

    /// <summary>Một dòng "Tên mục: giá trị".</summary>
    private static void AddInfoRow(Control parent, string caption, Label value, ref int y)
    {
        parent.Controls.Add(new Label { Text = caption, Left = 14, Top = y + 2, Width = 140, Height = 20, UseMnemonic = false });
        value.Left = 158;
        value.Top = y;
        value.Width = 215;
        parent.Controls.Add(value);
        y += 28;
    }

    private static Button NewButton(string text, int left, int top, int width, Func<Task> onClick)
    {
        var button = new Button { Text = text, Left = left, Top = top, Width = width, Height = 34, UseMnemonic = false };
        button.Click += async (_, _) => await onClick();
        return button;
    }

    private void BuildRegisterTab(TabPage page)
    {
        _regStart.Text = Fmt.Date(DateTime.Today);

        var left = new Panel { Dock = DockStyle.Left, Width = 350, AutoScroll = true, Padding = new Padding(8) };
        int y = 12;
        AddField(left, "Họ tên chủ xe (*)", _regName, ref y);
        AddField(left, "Số điện thoại (*)", _regPhone, ref y);
        AddField(left, "Số CCCD (*)", _regCccd, ref y);
        AddField(left, "Biển số xe (*)", _regPlate, ref y);
        AddField(left, "Loại xe (*)", _regType, ref y);
        AddField(left, "Hãng xe & Màu sơn", _regBrand, ref y);
        AddField(left, "Ngày bắt đầu", _regStart, ref y);
        AddField(left, "Gói đăng ký ban đầu", _regMonths, ref y);

        left.Controls.Add(new Label { Text = "Phí đăng ký mới:", Left = 16, Top = y + 3, Width = 130 });
        _regFee.Left = 150;
        _regFee.Top = y;
        _regFee.Width = 180;
        left.Controls.Add(_regFee);
        y += 36;

        left.Controls.Add(NewButton("Đăng Ký Vé Tháng Mới", 16, y, 200, async () => await RegisterAsync()));
        left.Controls.Add(NewButton("Làm mới", 224, y, 102, () => { ClearRegisterForm(); return Task.CompletedTask; }));
        _regToast.Left = 16;
        _regToast.Top = y + 44;
        _regToast.Width = 310;
        left.Controls.Add(_regToast);

        AddColumns(_gridAll,
            ("Mã Vé", "Code", 14),
            ("Biển số", "LicensePlate", 18),
            ("Chủ xe / SĐT", "OwnerText", 30),
            ("Phương tiện", "VehicleText", 28),
            ("Thời hạn", "PeriodText", 34),
            ("Trạng thái", "StatusText", 24));

        page.Controls.Add(_gridAll); // Fill thêm trước, Left thêm sau
        page.Controls.Add(left);
    }

    private void BuildRenewTab(TabPage page)
    {
        // ----- Bên trái: tìm kiếm + bảng chọn vé -----
        _search.Left = 8;
        _search.Top = 8;
        _filter.Left = 296;
        _filter.Top = 8;
        _filter.Width = 170;
        var bar = new Panel { Dock = DockStyle.Top, Height = 40 };
        bar.Controls.Add(_search);
        bar.Controls.Add(_filter);

        AddColumns(_gridRenew,
            ("Mã Vé", "Code", 16),
            ("Biển số", "LicensePlate", 20),
            ("Chủ xe", "OwnerName", 30),
            ("Hạn hiện tại", "EndDateText", 22),
            ("Trạng thái", "StatusText", 26));

        var leftPanel = new Panel { Dock = DockStyle.Fill };
        leftPanel.Controls.Add(_gridRenew);
        leftPanel.Controls.Add(bar);

        // ----- Bên phải: gia hạn + cập nhật / hủy -----
        var groupRenew = new GroupBox { Text = "Gia hạn Vé tháng đang chọn", Dock = DockStyle.Top, Height = 292 };
        int y = 26;
        AddInfoRow(groupRenew, "Mã vé & Biển số:", _renCode, ref y);
        AddInfoRow(groupRenew, "Chủ xe:", _renOwner, ref y);
        AddInfoRow(groupRenew, "Ngày hết hạn cũ:", _renOldEnd, ref y);
        groupRenew.Controls.Add(new Label { Text = "Số tháng muốn gia hạn thêm:", Left = 14, Top = y, Width = 340, Height = 18 });
        _renMonths.Left = 14;
        _renMonths.Top = y + 20;
        _renMonths.Width = 359;
        groupRenew.Controls.Add(_renMonths);
        y += 56;
        AddInfoRow(groupRenew, "Ngày hết hạn mới:", _renNewEnd, ref y);
        AddInfoRow(groupRenew, "Số tiền thu gia hạn:", _renFee, ref y);
        groupRenew.Controls.Add(NewButton("Xác Nhận Gia Hạn & Thu Phí", 14, y + 6, 359, async () => await RenewAsync()));

        var groupUpdate = new GroupBox { Text = "Cập nhật thông tin / Hủy vé", Dock = DockStyle.Fill };
        int y2 = 26;
        AddField(groupUpdate, "SĐT liên hệ", _updPhone, ref y2);
        AddField(groupUpdate, "Mô tả xe (hãng, màu)", _updBrand, ref y2);
        groupUpdate.Controls.Add(NewButton("Lưu thay đổi", 16, y2, 150, async () => await UpdateInfoAsync()));
        groupUpdate.Controls.Add(NewButton("Hủy vé", 176, y2, 150, async () => await CancelAsync()));
        _renToast.Left = 16;
        _renToast.Top = y2 + 44;
        _renToast.Width = 340;
        groupUpdate.Controls.Add(_renToast);

        var right = new Panel { Dock = DockStyle.Right, Width = 400, Padding = new Padding(8) };
        right.Controls.Add(groupUpdate);
        right.Controls.Add(groupRenew);

        page.Controls.Add(leftPanel);
        page.Controls.Add(right);
    }

    /// <summary>Tô màu ô Trạng thái: xanh còn hạn, cam sắp hết hạn, đỏ hết hạn / đã hủy.</summary>
    private static void ColorStatus(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (sender is not DataGridView grid || e.RowIndex < 0) return;
        if (grid.Columns[e.ColumnIndex].DataPropertyName != "StatusText") return;
        if (grid.Rows[e.RowIndex].DataBoundItem is not MonthlyPassView view) return;

        if (view.State == "ACTIVE") e.CellStyle.ForeColor = Color.SeaGreen;
        else if (view.State == "EXPIRING") e.CellStyle.ForeColor = Color.DarkOrange;
        else e.CellStyle.ForeColor = Color.Firebrick;
    }

    // =====================================================================
    // Nạp dữ liệu
    // =====================================================================
    private async Task LoadAllAsync()
    {
        await Dialogs.RunAsync(async () =>
        {
            await AppService.InitializeAsync();
            var types = await _pricing.GetVehicleTypesAsync();

            _loading = true;
            _regType.DataSource = types;
            _regType.DisplayMember = "TypeName";
            _regType.ValueMember = "VehicleTypeId";

            _regMonths.DataSource = new[] { 1, 3, 6 }
                .Select(m => new MonthOption(m, $"{m} tháng (→ {Fmt.Date(DateTime.Today.AddMonths(m))})")).ToList();
            _regMonths.DisplayMember = "Text";
            _regMonths.ValueMember = "Months";

            _renMonths.DataSource = new[] { 1, 3, 6 }
                .Select(m => new MonthOption(m, $"Gia hạn thêm +{m} tháng")).ToList();
            _renMonths.DisplayMember = "Text";
            _renMonths.ValueMember = "Months";

            _filter.DataSource = new List<FilterOption>
            {
                new("ALL", "Tất cả vé"),
                new("EXPIRING", "Sắp / Đã hết hạn"),
                new("ACTIVE", "Đang còn hạn")
            };
            _filter.DisplayMember = "Text";
            _filter.ValueMember = "Value";
            _loading = false;

            await UpdateRegFeeAsync();
            await RefreshGridsAsync();
        });
    }

    private async Task RefreshGridsAsync()
    {
        string filter = _filter.SelectedValue as string ?? "ALL";
        var all = await _service.GetViewsAsync();
        var filtered = await _service.GetViewsAsync(_search.Text, filter);
        int? keepId = _selected?.PassId;

        _loading = true;
        _gridAll.DataSource = all;
        _gridRenew.DataSource = filtered;
        _loading = false;

        // giữ lại vé đang chọn sau khi nạp lại
        foreach (DataGridViewRow row in _gridRenew.Rows)
        {
            if (row.DataBoundItem is MonthlyPassView v && v.PassId == keepId)
            {
                _gridRenew.CurrentCell = row.Cells[0];
                break;
            }
        }
        await ShowSelectedAsync();
    }

    private async Task ShowSelectedAsync()
    {
        _selected = _gridRenew.CurrentRow?.DataBoundItem as MonthlyPassView;
        if (_selected == null)
        {
            _renCode.Text = "—";
            _renOwner.Text = "—";
            _renOldEnd.Text = "—";
            _renNewEnd.Text = "—";
            _renFee.Text = "—";
            _updPhone.Clear();
            _updBrand.Clear();
            return;
        }

        _renCode.Text = $"{_selected.Code} ({_selected.LicensePlate})";
        _renOwner.Text = _selected.OwnerName;
        _renOldEnd.Text = _selected.EndDateText;
        _updPhone.Text = _selected.Phone;
        _updBrand.Text = _selected.Brand ?? "";
        await UpdateRenewPreviewAsync();
    }

    // =====================================================================
    // Tab 3.1: đăng ký mới
    // =====================================================================
    private async Task UpdateRegFeeAsync()
    {
        if (_loading || _regType.SelectedValue == null || _regMonths.SelectedValue == null) return;
        try
        {
            decimal fee = await _service.GetFeeAsync((string)_regType.SelectedValue, Convert.ToInt32(_regMonths.SelectedValue));
            _regFee.Text = Fmt.Money(fee);
        }
        catch (Exception)
        {
            _regFee.Text = "Chưa có bảng giá";
        }
    }

    private async Task RegisterAsync()
    {
        string? typeId = _regType.SelectedValue as string;
        if (typeId == null || _regMonths.SelectedValue == null)
        {
            MessageBox.Show("Hãy chọn loại xe và gói đăng ký.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        int months = Convert.ToInt32(_regMonths.SelectedValue);

        var pass = await Dialogs.TryAsync(() => _service.RegisterAsync(
            _regName.Text, _regPhone.Text, _regCccd.Text, _regPlate.Text, typeId, _regBrand.Text, months));
        if (pass == null) return;

        string message = $"✔ Đã đăng ký Vé tháng mới MP-{pass.PassId:000} cho xe {pass.LicensePlate}!";
        ClearRegisterForm();
        _regToast.Text = message;
        await Dialogs.RunAsync(RefreshGridsAsync);
    }

    private void ClearRegisterForm()
    {
        _regName.Clear();
        _regPhone.Clear();
        _regCccd.Clear();
        _regPlate.Clear();
        _regBrand.Clear();
        _regToast.Text = "";
    }

    // =====================================================================
    // Tab 3.2: gia hạn, cập nhật, hủy
    // =====================================================================
    private async Task UpdateRenewPreviewAsync()
    {
        if (_selected == null || _renMonths.SelectedValue == null) return;
        int months = Convert.ToInt32(_renMonths.SelectedValue);

        _renNewEnd.Text = Fmt.Date(MonthlyPassService.GetRenewEndDate(_selected.EndDate, months));
        try
        {
            decimal fee = await _service.GetFeeAsync(_selected.VehicleTypeId, months);
            _renFee.Text = Fmt.Money(fee);
        }
        catch (Exception)
        {
            _renFee.Text = "Chưa có bảng giá";
        }
    }

    private async Task RenewAsync()
    {
        if (_selected == null || _renMonths.SelectedValue == null)
        {
            MessageBox.Show("Hãy chọn một vé tháng trong bảng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        int passId = _selected.PassId;
        int months = Convert.ToInt32(_renMonths.SelectedValue);

        var pass = await Dialogs.TryAsync(() => _service.RenewAsync(passId, months));
        if (pass == null) return;

        _renToast.Text = $"✔ Đã gia hạn vé MP-{pass.PassId:000} đến {Fmt.Date(pass.EndDate)}!";
        await Dialogs.RunAsync(RefreshGridsAsync);
    }

    private async Task UpdateInfoAsync()
    {
        if (_selected == null)
        {
            MessageBox.Show("Hãy chọn một vé tháng trong bảng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        int passId = _selected.PassId;
        string phone = _updPhone.Text;
        string brand = _updBrand.Text;

        if (!await Dialogs.RunAsync(() => _service.UpdateInfoAsync(passId, phone, brand))) return;

        _renToast.Text = $"✔ Đã cập nhật thông tin vé MP-{passId:000}!";
        await Dialogs.RunAsync(RefreshGridsAsync);
    }

    private async Task CancelAsync()
    {
        if (_selected == null)
        {
            MessageBox.Show("Hãy chọn một vé tháng trong bảng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        int passId = _selected.PassId;

        var answer = MessageBox.Show($"Hủy vé tháng MP-{passId:000}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer != DialogResult.Yes) return;

        if (!await Dialogs.RunAsync(() => _service.CancelAsync(passId))) return;

        _renToast.Text = $"✔ Đã hủy vé tháng MP-{passId:000}!";
        await Dialogs.RunAsync(RefreshGridsAsync);
    }
}

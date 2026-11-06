using BAIDOXETHONGMINH.DTO.Entities;
using BAIDOXETHONGMINH.Models;

namespace BAIDOXETHONGMINH.GUI;

/// <summary>
/// NV3: sơ đồ vị trí đỗ xe 2D và bảng giá (frmParkingMap).
/// Bấm vào một ô để xem thông tin, đổi Bảo trì / Trống. Tự vẽ lại khi ParkingMapService báo MapChanged.
/// </summary>
public class ParkingMapForm : Form
{
    private static readonly Color AvailableColor = Color.FromArgb(165, 214, 167);
    private static readonly Color OccupiedColor = Color.FromArgb(239, 154, 154);
    private static readonly Color MaintenanceColor = Color.FromArgb(255, 224, 130);

    private readonly ParkingMapService _map = AppService.Map;
    private readonly PricingService _pricing = AppService.Pricing;

    private readonly FlowLayoutPanel _zonesPanel = new()
    {
        Dock = DockStyle.Fill,
        AutoScroll = true,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        Padding = new Padding(10)
    };
    private readonly Label _lblSubtitle = new() { AutoSize = true, ForeColor = Color.Gray, Left = 12, Top = 34 };
    private readonly Label _lblSlotCode = new() { AutoSize = true, Left = 14, Top = 26 };
    private readonly Label _lblSlotStatus = new() { AutoSize = true, Left = 14, Top = 52 };
    private readonly Label _lblSlotPlate = new() { AutoSize = true, Left = 14, Top = 78 };
    private readonly Button _btnMaintenance = new() { Text = "Đổi trạng thái Bảo trì / Mở trống", Left = 14, Top = 108, Width = 262, Height = 32 };
    private readonly Button _btnDeleteSlot = new() { Text = "Xóa ô này", Left = 14, Top = 146, Width = 262, Height = 28 };
    private readonly Label _lblPricing = new() { AutoSize = true, Left = 4, Top = 4 };

    private readonly Dictionary<string, Button> _slotButtons = new();
    private List<ZoneView> _zones = new();
    private List<VehicleType> _types = new();
    private string? _selectedCode;

    public ParkingMapForm()
    {
        Text = "Sơ đồ bãi đỗ xe & Bảng giá";
        Size = new Size(1000, 680);
        MinimumSize = new Size(760, 480);
        StartPosition = FormStartPosition.CenterScreen;

        BuildLayout();

        _btnMaintenance.Click += async (_, _) => await ToggleMaintenanceAsync();
        _btnDeleteSlot.Click += async (_, _) => await DeleteSlotAsync();

        _map.MapChanged += OnMapChanged;                    // đăng ký sự kiện (multicast delegate)
        FormClosed += (_, _) => _map.MapChanged -= OnMapChanged;
        ResizeEnd += async (_, _) => await RenderAsync();
        Load += async (_, _) =>
        {
            if (await Dialogs.RunAsync(AppService.InitializeAsync)) await RenderAsync();
        };
    }

    // ---------- Giao diện ----------
    private void BuildLayout()
    {
        // Cột phải: thông tin ô đang chọn + bảng giá
        var groupSlot = new GroupBox { Text = "Thông tin Ô đỗ đang chọn", Dock = DockStyle.Top, Height = 186 };
        groupSlot.Controls.AddRange(new Control[] { _lblSlotCode, _lblSlotStatus, _lblSlotPlate, _btnMaintenance, _btnDeleteSlot });

        var pricingPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(6) };
        pricingPanel.Controls.Add(_lblPricing);
        var groupPricing = new GroupBox { Text = "Cấu hình Bảng giá (PricingRule)", Dock = DockStyle.Fill };
        groupPricing.Controls.Add(pricingPanel);

        var right = new Panel { Dock = DockStyle.Right, Width = 300, Padding = new Padding(8) };
        right.Controls.Add(groupPricing);
        right.Controls.Add(groupSlot);

        // Thanh nút chức năng
        var tools = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, Padding = new Padding(8, 5, 8, 0) };
        tools.Controls.Add(MakeButton("Thêm khu", async (_, _) => await AddZoneAsync()));
        tools.Controls.Add(MakeButton("Thêm ô", async (_, _) => await AddSlotsAsync()));
        tools.Controls.Add(MakeButton("Sửa bảng giá", async (_, _) => await EditPricingAsync()));
        tools.Controls.Add(MakeButton("Quản lý thẻ xe", async (_, _) => await ManageCardsAsync()));
        tools.Controls.Add(MakeButton("Làm mới", async (_, _) => await RenderAsync()));

        // Tiêu đề + chú thích màu
        var header = new Panel { Dock = DockStyle.Top, Height = 62 };
        header.Controls.Add(new Label
        {
            Text = "SƠ ĐỒ VỊ TRÍ ĐỖ XE 2D & CẤU HÌNH BẢNG GIÁ",
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            AutoSize = true,
            UseMnemonic = false,
            Left = 12,
            Top = 8
        });
        header.Controls.Add(_lblSubtitle);
        var legend = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 270, Padding = new Padding(0, 14, 0, 0) };
        legend.Controls.Add(MakeLegend("Trống", AvailableColor));
        legend.Controls.Add(MakeLegend("Đang đỗ", OccupiedColor));
        legend.Controls.Add(MakeLegend("Bảo trì", MaintenanceColor));
        header.Controls.Add(legend);

        // Thứ tự Add: control Fill trước, các control Dock cạnh sau (cái thêm sau nằm ngoài cùng)
        Controls.Add(_zonesPanel);
        Controls.Add(right);
        Controls.Add(tools);
        Controls.Add(header);
    }

    private static Button MakeButton(string text, EventHandler onClick)
    {
        var button = new Button { Text = text, AutoSize = true, Height = 30, Margin = new Padding(3) };
        button.Click += onClick;
        return button;
    }

    private static Label MakeLegend(string text, Color color) => new Label
    {
        Text = text,
        BackColor = color,
        AutoSize = false,
        Width = 80,
        Height = 26,
        TextAlign = ContentAlignment.MiddleCenter
    };

    // ---------- Vẽ sơ đồ ----------
    private void OnMapChanged()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(OnMapChanged));
            return;
        }
        _ = RenderAsync();
    }

    private async Task RenderAsync()
    {
        try
        {
            _zones = await _map.GetMapAsync();
            _types = await _pricing.GetVehicleTypesAsync();
            var rules = await _pricing.GetAllAsync();

            ShowZones();
            ShowPricing(rules);
            ShowSlotInfo();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không tải được dữ liệu: " + ex.Message, "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowZones()
    {
        _zonesPanel.SuspendLayout();
        var oldControls = _zonesPanel.Controls.Cast<Control>().ToList();
        _zonesPanel.Controls.Clear();
        foreach (var control in oldControls) control.Dispose();
        _slotButtons.Clear();

        int width = Math.Max(300, _zonesPanel.ClientSize.Width - 40);
        foreach (var zone in _zones)
            _zonesPanel.Controls.Add(BuildZoneBox(zone, width));
        _zonesPanel.ResumeLayout();

        int total = _zones.Sum(z => z.Total);
        int free = _zones.Sum(z => z.Available);
        _lblSubtitle.Text = $"Bấm vào từng ô để kiểm tra xe hoặc khóa Bảo trì   |   Còn trống: {free} / {total} ô";
    }

    private GroupBox BuildZoneBox(ZoneView zone, int width)
    {
        string typeName = _types.FirstOrDefault(t => t.VehicleTypeId == zone.Zone.VehicleTypeId)?.TypeName
                          ?? zone.Zone.VehicleTypeId;
        var box = new GroupBox
        {
            Text = $"{zone.Zone.ZoneName} — {typeName} ({zone.Total} vị trí)",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MaximumSize = new Size(width, 0),
            MinimumSize = new Size(width, 70),
            Padding = new Padding(8, 6, 8, 8),
            Margin = new Padding(0, 0, 0, 12)
        };

        var flow = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MaximumSize = new Size(width - 20, 0),
            Dock = DockStyle.Top,
            WrapContents = true
        };
        foreach (var slot in zone.Slots)
        {
            var button = BuildSlotButton(slot);
            _slotButtons[slot.SlotCode] = button;
            flow.Controls.Add(button);
        }

        box.Controls.Add(flow);
        return box;
    }

    private Button BuildSlotButton(ParkingSlot slot)
    {
        string label = "Trống";
        if (slot.Status == SlotStatus.Occupied) label = slot.CurrentLicensePlate ?? "";
        else if (slot.Status == SlotStatus.Maintenance) label = "Bảo trì";

        var button = new Button
        {
            Width = 92,
            Height = 52,
            Margin = new Padding(4),
            FlatStyle = FlatStyle.Flat,
            BackColor = ColorOf(slot.Status),
            Text = $"{slot.SlotCode}\n{label}",
            TextAlign = ContentAlignment.MiddleLeft
        };
        MarkSelected(button, slot.SlotCode == _selectedCode);
        button.Click += (_, _) => SelectSlot(slot.SlotCode);
        return button;
    }

    private static Color ColorOf(string status)
    {
        if (status == SlotStatus.Occupied) return OccupiedColor;
        if (status == SlotStatus.Maintenance) return MaintenanceColor;
        return AvailableColor;
    }

    private static void MarkSelected(Button button, bool selected)
    {
        button.FlatAppearance.BorderSize = selected ? 3 : 1;
        button.FlatAppearance.BorderColor = selected ? Color.RoyalBlue : Color.Gray;
    }

    // ---------- Ô đang chọn ----------
    private void SelectSlot(string slotCode)
    {
        _selectedCode = slotCode;
        foreach (var pair in _slotButtons)
            MarkSelected(pair.Value, pair.Key == slotCode);
        ShowSlotInfo();
    }

    private void ShowSlotInfo()
    {
        var slot = _zones.SelectMany(z => z.Slots).FirstOrDefault(s => s.SlotCode == _selectedCode);
        bool has = slot != null;
        _btnMaintenance.Enabled = has;
        _btnDeleteSlot.Enabled = has;

        if (slot == null)
        {
            _lblSlotCode.Text = "Mã vị trí: —";
            _lblSlotStatus.Text = "Trạng thái: —";
            _lblSlotPlate.Text = "Biển số xe: —";
            return;
        }

        string statusText = "Đang trống (Available)";
        if (slot.Status == SlotStatus.Occupied) statusText = "Đang có xe (Occupied)";
        else if (slot.Status == SlotStatus.Maintenance) statusText = "Đang bảo trì (Maintenance)";

        _lblSlotCode.Text = "Mã vị trí: " + slot.SlotCode;
        _lblSlotStatus.Text = "Trạng thái: " + statusText;
        _lblSlotPlate.Text = "Biển số xe: " + (string.IsNullOrEmpty(slot.CurrentLicensePlate) ? "—" : slot.CurrentLicensePlate);
    }

    private void ShowPricing(List<PricingRule> rules)
    {
        var lines = new List<string>();
        foreach (var rule in rules)
        {
            string typeName = _types.FirstOrDefault(t => t.VehicleTypeId == rule.VehicleTypeId)?.TypeName ?? rule.VehicleTypeId;
            lines.Add(typeName.ToUpper());
            lines.Add($"   Ngày / Đêm: {Fmt.Money(rule.DayTurnPrice)} / {Fmt.Money(rule.OvernightPrice)}");
            lines.Add($"   Mỗi giờ: {Fmt.Money(rule.HourlyPrice)}");
            lines.Add($"   Vé tháng: {Fmt.Money(rule.MonthlyPassPrice)}");
            lines.Add($"   Phạt mất thẻ: {Fmt.Money(rule.LostCardPenalty)}");
            lines.Add("");
        }
        _lblPricing.Text = lines.Count == 0 ? "Chưa có bảng giá." : string.Join(Environment.NewLine, lines);
    }

    // ---------- Thao tác ----------
    private async Task ToggleMaintenanceAsync()
    {
        string? code = _selectedCode;
        if (code == null) return;
        await Dialogs.RunAsync(() => _map.ToggleMaintenanceAsync(code)); // thành công thì MapChanged tự vẽ lại
    }

    private async Task DeleteSlotAsync()
    {
        string? code = _selectedCode;
        if (code == null) return;

        var answer = MessageBox.Show($"Xóa ô {code}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer != DialogResult.Yes) return;

        if (await Dialogs.RunAsync(() => _map.DeleteSlotAsync(code)))
            _selectedCode = null;
    }

    private async Task AddZoneAsync()
    {
        var v = Dialogs.Prompt(this, "Thêm khu", ("Mã khu (VD: C)", ""), ("Tên khu", ""), ("Loại xe (XEMAY / OTO)", "XEMAY"), ("Sức chứa", "10"));
        if (v == null) return;

        if (!int.TryParse(v[3], out int capacity))
        {
            MessageBox.Show("Sức chứa phải là số.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        await Dialogs.RunAsync(() => _map.AddZoneAsync(v[0], v[1], v[2], capacity));
    }

    private async Task AddSlotsAsync()
    {
        var v = Dialogs.Prompt(this, "Thêm ô đỗ", ("Mã khu", ""), ("Tiền tố mã ô (VD: A-)", ""), ("Số lượng ô", "1"));
        if (v == null) return;

        if (!int.TryParse(v[2], out int count))
        {
            MessageBox.Show("Số lượng phải là số.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        await Dialogs.RunAsync(() => _map.AddSlotsAsync(v[0], v[1], count));
    }

    private async Task EditPricingAsync()
    {
        using var form = new PricingForm();
        form.ShowDialog(this);
        await RenderAsync();
    }

    private async Task ManageCardsAsync()
    {
        using var form = new ParkingCardForm();
        form.ShowDialog(this);
        await RenderAsync();
    }

}

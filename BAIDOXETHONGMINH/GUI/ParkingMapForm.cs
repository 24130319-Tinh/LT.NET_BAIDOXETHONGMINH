namespace BAIDOXETHONGMINH.GUI;

public partial class ParkingMapForm : Form
{
    private static readonly Color AvailableColor = Color.FromArgb(220, 252, 231);
    private static readonly Color OccupiedColor = Color.FromArgb(254, 226, 226);
    private static readonly Color MaintenanceColor = Color.FromArgb(254, 243, 199);

    private readonly ParkingMapService _map = AppService.Map;

    private sealed class SlotItem
    {
        public string Code { get; init; } = "";
        public string Status { get; set; } = "Available";
        public string? Plate { get; set; }
        public Button? ButtonRef { get; set; }
    }

    private readonly List<SlotItem> _slotsA = new();

    private readonly List<SlotItem> _slotsB = new();

    private SlotItem? _selectedSlot;

    public ParkingMapForm()
    {
        InitializeComponent();

        RoundedUiHelper.AttachRoundedFrame(lblLegend, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlZoneA, 16, RoundedUiHelper.CardBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlZoneB, 16, RoundedUiHelper.CardBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlSlotDetail, 16, RoundedUiHelper.CardBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlPricingCard, 16, RoundedUiHelper.CardBorder);
        RoundedUiHelper.AttachRoundedFrame(btnToggleMaint, 10, RoundedUiHelper.SlateBorder);

        InitializeDataBindings();
    }

    private void BuildSlotGrid(List<SlotItem> items, TableLayoutPanel grid, int cols)
    {
        foreach (Control control in grid.Controls.Cast<Control>().ToArray()) control.Dispose();
        for (int i = 0; i < items.Count; i++)
        {
            var slot = items[i];
            var btn = new Button
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(4),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Consolas", 8.8F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 0;
            slot.ButtonRef = btn;
            btn.Click += (s, e) => SelectSlot(slot);
            UpdateSlotButtonAppearance(slot);
            grid.Controls.Add(btn, i % cols, i / cols);
        }
    }

    private void UpdateSlotButtonAppearance(SlotItem slot)
    {
        if (slot.ButtonRef == null) return;

        Color bg = AvailableColor;
        Color border = Color.FromArgb(74, 222, 128);
        string sub = "Trống";

        if (slot.Status == "Occupied")
        {
            bg = OccupiedColor;
            border = Color.FromArgb(248, 113, 113);
            sub = slot.Plate ?? "Có xe";
        }
        else if (slot.Status == "Maintenance")
        {
            bg = MaintenanceColor;
            border = Color.FromArgb(251, 191, 36);
            sub = "Bảo trì";
        }

        if (_selectedSlot == slot)
        {
            border = Color.FromArgb(37, 99, 235);
        }

        slot.ButtonRef.BackColor = bg;
        slot.ButtonRef.Text = $"{slot.Code}\n{sub}";
        RoundedUiHelper.AttachRoundedFrame(slot.ButtonRef, 10, border);
    }

    private void SelectSlot(SlotItem slot)
    {
        var prev = _selectedSlot;
        _selectedSlot = slot;
        if (prev != null) UpdateSlotButtonAppearance(prev);
        UpdateSlotButtonAppearance(slot);

        lblDetCodeVal.Text = slot.Code;
        lblDetStatusVal.Text = slot.Status switch
        {
            "Occupied"    => "Đang có xe (Occupied)",
            "Maintenance" => "Đang bảo trì (Maintenance)",
            _             => "Đang trống (Available)"
        };
        lblDetPlateVal.Text = slot.Plate ?? "—";
        btnToggleMaint.Enabled = slot.Status != SlotStatus.Occupied;
    }

    private bool _refreshing;
    private bool _refreshAgain;
    private readonly Dictionary<string, (Panel Panel, Label Title, TableLayoutPanel Grid, List<SlotItem> Slots, int Columns)> _zoneViews = new();

    private void InitializeDataBindings()
    {
        _zoneViews["A"] = (pnlZoneA, lblZoneATitle, tlpGridA, _slotsA, 6);
        _zoneViews["B"] = (pnlZoneB, lblZoneBTitle, tlpGridB, _slotsB, 4);
        foreach (var pair in _zoneViews) AttachZoneMenu(pair.Value.Panel, pair.Key);
        var menu = new ContextMenuStrip();
        menu.Items.Add("Cấu hình bảng giá", null, async (_, _) => await EditPricingAsync());
        menu.Items.Add("Quản lý thẻ xe", null, (_, _) => { using var form = new ParkingCardForm(); form.ShowDialog(this); });
        pnlPricingCard.ContextMenuStrip = menu;
        lblPricingTitle.DoubleClick += async (_, _) => await EditPricingAsync();
        _map.MapChanged += OnMapChanged;
        FormClosed += (_, _) => _map.MapChanged -= OnMapChanged;
        Load += async (_, _) => await Dialogs.RunAsync(async () =>
        {
            await AppService.InitializeAsync();
            await RefreshMapAsync();
        });
    }

    private void OnMapChanged()
    {
        if (IsDisposed || !IsHandleCreated) return;
        if (InvokeRequired) { BeginInvoke(new Action(OnMapChanged)); return; }
        _ = Dialogs.RunAsync(RefreshMapAsync);
    }

    private async Task RefreshMapAsync()
    {
        if (_refreshing) { _refreshAgain = true; return; }
        _refreshing = true;
        try
        {
            do
            {
                _refreshAgain = false;
                string? selected = _selectedSlot?.Code;
                var zones = await _map.GetMapAsync();
                var rules = await AppService.Pricing.GetAllAsync();
                if (IsDisposed) return;
                foreach (var view in _zoneViews.Values) view.Panel.Visible = false;
                foreach (var zone in zones)
                {
                    if (!_zoneViews.TryGetValue(zone.Zone.ZoneId, out var view))
                    {
                        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(6), Padding = new Padding(16) };
                        var title = new Label { Dock = DockStyle.Top, Height = 30, Font = lblZoneATitle.Font, ForeColor = lblZoneATitle.ForeColor };
                        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6 };
                        for (int i = 0; i < 6; i++) grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 6));
                        panel.Controls.Add(grid);
                        panel.Controls.Add(title);
                        RoundedUiHelper.AttachRoundedFrame(panel, 16, RoundedUiHelper.CardBorder);
                        int row = tlpLeftZones.RowCount++;
                        tlpLeftZones.RowStyles.Add(new RowStyle(SizeType.Absolute, 240));
                        tlpLeftZones.Controls.Add(panel, 0, row);
                        tlpLeftZones.AutoScroll = true;
                        view = (panel, title, grid, new List<SlotItem>(), 6);
                        _zoneViews[zone.Zone.ZoneId] = view;
                        AttachZoneMenu(panel, zone.Zone.ZoneId);
                    }
                    view.Panel.Visible = true;
                    view.Title.Text = $"{zone.Zone.ZoneName} ({zone.Total} vị trí)";
                    view.Slots.Clear();
                    view.Slots.AddRange(zone.Slots.Select(s => new SlotItem { Code = s.SlotCode, Status = s.Status, Plate = s.CurrentLicensePlate }));
                    view.Grid.RowCount = Math.Max(1, (view.Slots.Count + view.Columns - 1) / view.Columns);
                    view.Grid.RowStyles.Clear();
                    for (int i = 0; i < view.Grid.RowCount; i++) view.Grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / view.Grid.RowCount));
                    BuildSlotGrid(view.Slots, view.Grid, view.Columns);
                }
                var allSlots = _zoneViews.Values.Where(v => v.Panel.Visible).SelectMany(v => v.Slots).ToList();
                _selectedSlot = null;
                var chosen = allSlots.FirstOrDefault(s => s.Code == selected) ?? allSlots.FirstOrDefault();
                if (chosen != null) SelectSlot(chosen);
                else
                {
                    lblDetCodeVal.Text = lblDetStatusVal.Text = lblDetPlateVal.Text = "—";
                }
                btnToggleMaint.Enabled = chosen != null && chosen.Status != SlotStatus.Occupied;
                foreach (Control control in flpPricingRows.Controls.Cast<Control>().ToArray()) control.Dispose();
                var bike = rules.FirstOrDefault(r => r.VehicleTypeId == "XEMAY");
                var car = rules.FirstOrDefault(r => r.VehicleTypeId == "OTO");
                AddPriceItem("Xe máy (Ngày / Đêm)", bike == null ? "Chưa cấu hình" : $"{Fmt.Number(bike.DayTurnPrice)} / {Fmt.Money(bike.OvernightPrice)}");
                AddPriceItem("Ô tô (Mỗi giờ)", car == null ? "Chưa cấu hình" : Fmt.Money(car.HourlyPrice) + " / giờ");
                AddPriceItem("Vé tháng (XM / Ô tô)", $"{(bike == null ? "—" : Fmt.MoneyK(bike.MonthlyPassPrice))} / {(car == null ? "—" : Fmt.MoneyK(car.MonthlyPassPrice))}");
                AddPriceItem("Phạt mất thẻ", bike == null ? "Chưa cấu hình" : car == null || car.LostCardPenalty == bike.LostCardPenalty
                    ? Fmt.Money(bike.LostCardPenalty) : $"{Fmt.MoneyK(bike.LostCardPenalty)} / {Fmt.MoneyK(car.LostCardPenalty)}");
            } while (_refreshAgain);
        }
        finally { _refreshing = false; }
    }

    private async void BtnToggleMaint_Click(object? sender, EventArgs e)
    {
        if (_selectedSlot == null) return;
        string code = _selectedSlot.Code;
        btnToggleMaint.Enabled = false;
        await Dialogs.RunAsync(async () => { await _map.ToggleMaintenanceAsync(code); await RefreshMapAsync(); });
        if (!IsDisposed) btnToggleMaint.Enabled = _selectedSlot != null && _selectedSlot.Status != SlotStatus.Occupied;
    }

    private async Task EditPricingAsync()
    {
        using var form = new PricingForm();
        form.ShowDialog(this);
        await Dialogs.RunAsync(RefreshMapAsync);
    }

    private void AttachZoneMenu(Control panel, string zoneId)
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Thêm ô đỗ", null, async (_, _) =>
        {
            var input = Dialogs.Prompt(this, "Thêm ô đỗ", ("Tiền tố", zoneId + "-"), ("Số lượng", "1"));
            if (input == null) return;
            await Dialogs.RunAsync(async () =>
            {
                if (!int.TryParse(input[1], out int count)) throw new ServiceException("Số lượng không hợp lệ.");
                await _map.AddSlotsAsync(zoneId, input[0], count);
                await RefreshMapAsync();
            });
        });
        menu.Items.Add("Xóa ô đang chọn", null, async (_, _) =>
        {
            if (_selectedSlot == null || !_zoneViews[zoneId].Slots.Any(s => s.Code == _selectedSlot.Code)) return;
            await Dialogs.RunAsync(async () => { await _map.DeleteSlotAsync(_selectedSlot.Code); await RefreshMapAsync(); });
        });
        menu.Items.Add("Thêm khu", null, async (_, _) =>
        {
            var input = Dialogs.Prompt(this, "Thêm khu", ("Mã khu", "C"), ("Tên khu", "Khu C"), ("Loại xe", "XEMAY"), ("Sức chứa", "20"));
            if (input == null) return;
            await Dialogs.RunAsync(async () =>
            {
                if (!int.TryParse(input[3], out int count)) throw new ServiceException("Sức chứa không hợp lệ.");
                await _map.AddZoneAsync(input[0], input[1], input[2], count);
                await RefreshMapAsync();
            });
        });
        menu.Items.Add("Xóa khu này", null, async (_, _) => await Dialogs.RunAsync(async () =>
        {
            await _map.DeleteZoneAsync(zoneId);
            await RefreshMapAsync();
        }));
        panel.ContextMenuStrip = menu;
    }
    private void AddPriceItem(string label, string price)
    {
        var row = new Panel
        {
            Width = 285,
            Height = 42,
            BackColor = Color.FromArgb(248, 250, 252),
            Margin = new Padding(0, 4, 0, 4),
            Padding = new Padding(10, 0, 10, 0)
        };
        var l1 = new Label
        {
            Text = label,
            Dock = DockStyle.Left,
            Width = 155,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 8.8F)
        };
        var l2 = new Label
        {
            Text = price,
            Dock = DockStyle.Right,
            Width = 105,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Consolas", 9F, FontStyle.Bold),
            ForeColor = label.Contains("Phạt") ? Color.FromArgb(225, 29, 72) : Color.FromArgb(15, 23, 42)
        };
        row.Controls.Add(l2);
        row.Controls.Add(l1);
        RoundedUiHelper.AttachRoundedFrame(row, 8, RoundedUiHelper.CardBorder);
        flpPricingRows.Controls.Add(row);
    }
}

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

    private readonly List<SlotItem> _slotsA =
    [
        new() { Code = "A-01", Status = "Occupied", Plate = "59X3-112.23" },
        new() { Code = "A-02", Status = "Occupied", Plate = "59P2-345.67" },
        new() { Code = "A-03", Status = "Available" },
        new() { Code = "A-04", Status = "Occupied", Plate = "59D1-908.11" },
        new() { Code = "A-05", Status = "Available" },
        new() { Code = "A-06", Status = "Available" },
        new() { Code = "A-07", Status = "Maintenance" },
        new() { Code = "A-08", Status = "Available" },
        new() { Code = "A-09", Status = "Occupied", Plate = "59G2-331.09" },
        new() { Code = "A-10", Status = "Available" },
        new() { Code = "A-11", Status = "Available" },
        new() { Code = "A-12", Status = "Available" }
    ];

    private readonly List<SlotItem> _slotsB =
    [
        new() { Code = "B-01", Status = "Occupied", Plate = "51H-999.99" },
        new() { Code = "B-02", Status = "Occupied", Plate = "51G-456.78" },
        new() { Code = "B-03", Status = "Available" },
        new() { Code = "B-04", Status = "Available" },
        new() { Code = "B-05", Status = "Occupied", Plate = "51K-128.88" },
        new() { Code = "B-06", Status = "Available" },
        new() { Code = "B-07", Status = "Maintenance" },
        new() { Code = "B-08", Status = "Available" }
    ];

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

        BuildSlotGrid(_slotsA, tlpGridA, 6);
        BuildSlotGrid(_slotsB, tlpGridB, 4);
        BuildPricingRows();

        SelectSlot(_slotsA[0]);
    }

    private void BuildSlotGrid(List<SlotItem> items, TableLayoutPanel grid, int cols)
    {
        grid.Controls.Clear();
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
    }

    private void BtnToggleMaint_Click(object? sender, EventArgs e)
    {
        if (_selectedSlot == null) return;
        if (_selectedSlot.Status == "Occupied")
        {
            MessageBox.Show("Ô đỗ này đang có xe, không thể chuyển sang Bảo trì!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _selectedSlot.Status = _selectedSlot.Status == "Maintenance" ? "Available" : "Maintenance";
        SelectSlot(_selectedSlot);
    }

    private void BuildPricingRows()
    {
        AddPriceItem("Xe máy (Ngày / Đêm)", "5.000 / 10.000đ");
        AddPriceItem("Ô tô (Mỗi giờ)", "20.000đ / giờ");
        AddPriceItem("Vé tháng (XM / Ô tô)", "150k / 1.200k");
        AddPriceItem("Phạt mất thẻ", "50.000đ");
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

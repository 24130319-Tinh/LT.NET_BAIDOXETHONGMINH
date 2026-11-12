#nullable enable
namespace BAIDOXETHONGMINH.GUI;

partial class ParkingMapForm
{
    private System.ComponentModel.IContainer? components = null;

    private Panel pnlHeader = null!;
    private Label lblTitle = null!;
    private Label lblSubtitle = null!;
    private Label lblLegend = null!;

    private TableLayoutPanel tlpMain = null!;
    private TableLayoutPanel tlpLeftZones = null!;
    private Panel pnlZoneA = null!;
    private Label lblZoneATitle = null!;
    private TableLayoutPanel tlpGridA = null!;
    private Panel pnlZoneB = null!;
    private Label lblZoneBTitle = null!;
    private TableLayoutPanel tlpGridB = null!;

    private TableLayoutPanel tlpRightSide = null!;
    private Panel pnlSlotDetail = null!;
    private Label lblDetailTitle = null!;
    private TableLayoutPanel tlpDetailInfo = null!;
    private Label lblDetCodeVal = null!;
    private Label lblDetStatusVal = null!;
    private Label lblDetPlateVal = null!;
    private Button btnToggleMaint = null!;

    private Panel pnlPricingCard = null!;
    private Label lblPricingTitle = null!;
    private FlowLayoutPanel flpPricingRows = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        pnlHeader = new Panel();
        lblTitle = new Label();
        lblSubtitle = new Label();
        lblLegend = new Label();

        tlpMain = new TableLayoutPanel();
        tlpLeftZones = new TableLayoutPanel();
        pnlZoneA = new Panel();
        lblZoneATitle = new Label();
        tlpGridA = new TableLayoutPanel();
        pnlZoneB = new Panel();
        lblZoneBTitle = new Label();
        tlpGridB = new TableLayoutPanel();

        tlpRightSide = new TableLayoutPanel();
        pnlSlotDetail = new Panel();
        lblDetailTitle = new Label();
        tlpDetailInfo = new TableLayoutPanel();
        lblDetCodeVal = new Label();
        lblDetStatusVal = new Label();
        lblDetPlateVal = new Label();
        btnToggleMaint = new Button();

        pnlPricingCard = new Panel();
        lblPricingTitle = new Label();
        flpPricingRows = new FlowLayoutPanel();

        SuspendLayout();

        ClientSize = new Size(1020, 720);

        pnlHeader.Size = new Size(1020, 68);
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.BackColor = Color.FromArgb(248, 250, 252);

        lblTitle.Text = "SƠ ĐỒ VỊ TRÍ ĐỖ XE 2D & CẤU HÌNH BẢNG GIÁ (PricingRule)";
        lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblTitle.Location = new Point(16, 10);
        lblTitle.AutoSize = true;

        lblSubtitle.Text = "Thành viên 2 • Click trực tiếp vào từng ô trên sơ đồ để kiểm tra xe hoặc khóa Bảo trì";
        lblSubtitle.Font = new Font("Segoe UI", 9F);
        lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
        lblSubtitle.Location = new Point(16, 36);
        lblSubtitle.AutoSize = true;

        lblLegend.Text = "● Xanh: Trống   ● Đỏ: Đang đỗ   ● Vàng: Bảo trì";
        lblLegend.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblLegend.ForeColor = Color.FromArgb(51, 65, 85);
        lblLegend.BackColor = Color.White;
        lblLegend.TextAlign = ContentAlignment.MiddleCenter;
        lblLegend.Size = new Size(290, 32);
        lblLegend.Location = new Point(710, 18);
        lblLegend.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        pnlHeader.Controls.Add(lblLegend);
        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Controls.Add(lblSubtitle);

        tlpMain.Size = new Size(1020, 652);
        tlpMain.Dock = DockStyle.Fill;
        tlpMain.ColumnCount = 2;
        tlpMain.RowCount = 1;
        tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66F));
        tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
        tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpMain.Padding = new Padding(10, 4, 10, 12);
        tlpMain.BackColor = Color.FromArgb(248, 250, 252);

        tlpLeftZones.Dock = DockStyle.Fill;
        tlpLeftZones.ColumnCount = 1;
        tlpLeftZones.RowCount = 2;
        tlpLeftZones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpLeftZones.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
        tlpLeftZones.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));

        pnlZoneA.Dock = DockStyle.Fill;
        pnlZoneA.BackColor = Color.White;
        pnlZoneA.Margin = new Padding(6);
        pnlZoneA.Padding = new Padding(16);

        lblZoneATitle.Text = "Khu A — Xe Máy (12 vị trí)";
        lblZoneATitle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        lblZoneATitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblZoneATitle.Dock = DockStyle.Top;
        lblZoneATitle.Height = 30;

        tlpGridA.Dock = DockStyle.Fill;
        tlpGridA.ColumnCount = 6;
        tlpGridA.RowCount = 2;
        for (int i = 0; i < 6; i++) tlpGridA.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));
        for (int r = 0; r < 2; r++) tlpGridA.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        pnlZoneA.Controls.Add(tlpGridA);
        pnlZoneA.Controls.Add(lblZoneATitle);

        pnlZoneB.Dock = DockStyle.Fill;
        pnlZoneB.BackColor = Color.White;
        pnlZoneB.Margin = new Padding(6);
        pnlZoneB.Padding = new Padding(16);

        lblZoneBTitle.Text = "Khu B — Ô Tô (8 vị trí)";
        lblZoneBTitle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        lblZoneBTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblZoneBTitle.Dock = DockStyle.Top;
        lblZoneBTitle.Height = 30;

        tlpGridB.Dock = DockStyle.Fill;
        tlpGridB.ColumnCount = 4;
        tlpGridB.RowCount = 2;
        for (int i = 0; i < 4; i++) tlpGridB.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        for (int r = 0; r < 2; r++) tlpGridB.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        pnlZoneB.Controls.Add(tlpGridB);
        pnlZoneB.Controls.Add(lblZoneBTitle);

        tlpLeftZones.Controls.Add(pnlZoneA, 0, 0);
        tlpLeftZones.Controls.Add(pnlZoneB, 0, 1);

        tlpRightSide.Dock = DockStyle.Fill;
        tlpRightSide.ColumnCount = 1;
        tlpRightSide.RowCount = 2;
        tlpRightSide.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpRightSide.RowStyles.Add(new RowStyle(SizeType.Absolute, 235F));
        tlpRightSide.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        pnlSlotDetail.Dock = DockStyle.Fill;
        pnlSlotDetail.BackColor = Color.White;
        pnlSlotDetail.Margin = new Padding(6);
        pnlSlotDetail.Padding = new Padding(16);

        lblDetailTitle.Text = "Thông tin Ô đỗ đang chọn";
        lblDetailTitle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        lblDetailTitle.Dock = DockStyle.Top;
        lblDetailTitle.Height = 30;

        btnToggleMaint.Text = "Đổi trạng thái Bảo trì / Mở trống";
        btnToggleMaint.Dock = DockStyle.Bottom;
        btnToggleMaint.Height = 40;
        btnToggleMaint.FlatStyle = FlatStyle.Flat;
        btnToggleMaint.FlatAppearance.BorderSize = 0;
        btnToggleMaint.BackColor = Color.FromArgb(241, 245, 249);
        btnToggleMaint.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnToggleMaint.Cursor = Cursors.Hand;
        btnToggleMaint.Click += BtnToggleMaint_Click;

        tlpDetailInfo.Dock = DockStyle.Fill;
        tlpDetailInfo.ColumnCount = 2;
        tlpDetailInfo.RowCount = 3;
        tlpDetailInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
        tlpDetailInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
        for (int i = 0; i < 3; i++) tlpDetailInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3F));

        AddDetailRow(tlpDetailInfo, 0, "Mã vị trí:", out lblDetCodeVal, "A-01");
        AddDetailRow(tlpDetailInfo, 1, "Trạng thái:", out lblDetStatusVal, "Đang có xe");
        AddDetailRow(tlpDetailInfo, 2, "Biển số xe:", out lblDetPlateVal, "59X3-112.23");

        pnlSlotDetail.Controls.Add(tlpDetailInfo);
        pnlSlotDetail.Controls.Add(btnToggleMaint);
        pnlSlotDetail.Controls.Add(lblDetailTitle);

        pnlPricingCard.Dock = DockStyle.Fill;
        pnlPricingCard.BackColor = Color.White;
        pnlPricingCard.Margin = new Padding(6);
        pnlPricingCard.Padding = new Padding(16);

        lblPricingTitle.Text = "Cấu hình Bảng giá (PricingRule)";
        lblPricingTitle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        lblPricingTitle.Dock = DockStyle.Top;
        lblPricingTitle.Height = 32;

        flpPricingRows.Dock = DockStyle.Fill;
        flpPricingRows.FlowDirection = FlowDirection.TopDown;
        flpPricingRows.WrapContents = false;

        pnlPricingCard.Controls.Add(flpPricingRows);
        pnlPricingCard.Controls.Add(lblPricingTitle);

        tlpRightSide.Controls.Add(pnlSlotDetail, 0, 0);
        tlpRightSide.Controls.Add(pnlPricingCard, 0, 1);

        tlpMain.Controls.Add(tlpLeftZones, 0, 0);
        tlpMain.Controls.Add(tlpRightSide, 1, 0);

        AutoScaleMode = AutoScaleMode.Font;
        FormBorderStyle = FormBorderStyle.None;
        Controls.Add(tlpMain);
        Controls.Add(pnlHeader);

        ResumeLayout(false);
    }

    private static void AddDetailRow(TableLayoutPanel tlp, int row, string title, out Label valLbl, string defaultVal)
    {
        var l = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(100, 116, 139),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        valLbl = new Label
        {
            Text = defaultVal,
            Font = new Font("Consolas", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight
        };
        tlp.Controls.Add(l, 0, row);
        tlp.Controls.Add(valLbl, 1, row);
    }
}

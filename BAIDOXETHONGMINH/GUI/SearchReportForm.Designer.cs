#nullable enable
namespace BAIDOXETHONGMINH.GUI;

partial class SearchReportForm
{
    private System.ComponentModel.IContainer? components = null;

    private Panel pnlHeader = null!;
    private Label lblTitle = null!;
    private Label lblSubtitle = null!;

    private TableLayoutPanel tlpMain = null!;
    private TableLayoutPanel tlpKpiRow = null!;
    private Panel pnlKpi1 = null!;
    private Panel pnlKpi2 = null!;
    private Panel pnlKpi3 = null!;
    private Panel pnlKpi4 = null!;

    private Panel pnlFilterCard = null!;
    private Panel pnlSearchInputBox = null!;
    private TextBox txtSearchPlate = null!;
    private Panel pnlShiftComboBox = null!;
    private ComboBox cboShiftFilter = null!;
    private Button btnSearch = null!;
    private Button btnExportCsv = null!;

    private Panel pnlGridCard = null!;
    private DataGridView dgvHistory = null!;

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
        tlpMain = new TableLayoutPanel();
        tlpKpiRow = new TableLayoutPanel();

        pnlFilterCard = new Panel();
        pnlSearchInputBox = new Panel();
        txtSearchPlate = new TextBox();
        pnlShiftComboBox = new Panel();
        cboShiftFilter = new ComboBox();
        btnSearch = new Button();
        btnExportCsv = new Button();

        pnlGridCard = new Panel();
        dgvHistory = new DataGridView();

        SuspendLayout();

        ClientSize = new Size(1020, 720);

        pnlHeader.Size = new Size(1020, 68);
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.BackColor = Color.FromArgb(248, 250, 252);

        lblTitle.Text = "TRA CỨU LỊCH SỬ XE RA/VÀO & BÁO CÁO DOANH THU";
        lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblTitle.Location = new Point(16, 10);
        lblTitle.AutoSize = true;

        lblSubtitle.Text = "Thành viên 3 • Tổng hợp số liệu xe trong bãi, tra cứu mất thẻ theo biển số & kết xuất doanh thu";
        lblSubtitle.Font = new Font("Segoe UI", 9F);
        lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
        lblSubtitle.Location = new Point(16, 36);
        lblSubtitle.AutoSize = true;

        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Controls.Add(lblSubtitle);

        tlpMain.Size = new Size(1020, 652);
        tlpMain.Dock = DockStyle.Fill;
        tlpMain.ColumnCount = 1;
        tlpMain.RowCount = 3;
        tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 118F));
        tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 88F));
        tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpMain.Padding = new Padding(10, 4, 10, 12);
        tlpMain.BackColor = Color.FromArgb(248, 250, 252);

        tlpKpiRow.Dock = DockStyle.Fill;
        tlpKpiRow.ColumnCount = 4;
        tlpKpiRow.RowCount = 1;
        for (int i = 0; i < 4; i++) tlpKpiRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        tlpKpiRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        pnlKpi1 = CreateKpiCard("Xe đang gửi trong bãi", "7 / 20 xe", "4 Xe máy • 3 Ô tô", Color.FromArgb(37, 99, 235));
        pnlKpi2 = CreateKpiCard("Doanh thu Vé lượt (Ca Sáng)", "685.000 đ", "42 lượt xe ra", Color.FromArgb(5, 150, 105));
        pnlKpi3 = CreateKpiCard("Doanh thu Vé tháng hôm nay", "2.550.000 đ", "Đăng ký & Gia hạn", Color.FromArgb(15, 23, 42));
        pnlKpi4 = CreateKpiCard("Sự cố mất thẻ hôm nay", "1 biên bản", "Thu phạt: 50.000 đ", Color.FromArgb(225, 29, 72));

        tlpKpiRow.Controls.Add(pnlKpi1, 0, 0);
        tlpKpiRow.Controls.Add(pnlKpi2, 1, 0);
        tlpKpiRow.Controls.Add(pnlKpi3, 2, 0);
        tlpKpiRow.Controls.Add(pnlKpi4, 3, 0);

        pnlFilterCard.Size = new Size(988, 76);
        pnlFilterCard.Dock = DockStyle.Fill;
        pnlFilterCard.BackColor = Color.White;
        pnlFilterCard.Margin = new Padding(6);

        var lblFind = new Label
        {
            Text = "Tìm theo Biển số xe / Mã thẻ",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(100, 116, 139),
            Location = new Point(16, 10),
            AutoSize = true
        };

        pnlSearchInputBox.Location = new Point(16, 30);
        pnlSearchInputBox.Size = new Size(440, 34);
        pnlSearchInputBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pnlSearchInputBox.BackColor = Color.White;
        pnlSearchInputBox.Padding = new Padding(10, 7, 10, 5);

        txtSearchPlate.Dock = DockStyle.Fill;
        txtSearchPlate.BorderStyle = BorderStyle.None;
        txtSearchPlate.Font = new Font("Segoe UI", 9.5F);
        txtSearchPlate.PlaceholderText = "Nhập biển số xe cần đối chiếu (VD: 59X3, 51G...)";
        pnlSearchInputBox.Controls.Add(txtSearchPlate);

        var lblShift = new Label
        {
            Text = "Ca trực",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(100, 116, 139),
            Location = new Point(470, 10),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            AutoSize = true
        };

        pnlShiftComboBox.Location = new Point(470, 30);
        pnlShiftComboBox.Size = new Size(210, 34);
        pnlShiftComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        pnlShiftComboBox.BackColor = Color.White;
        pnlShiftComboBox.Padding = new Padding(8, 4, 8, 4);

        cboShiftFilter.Dock = DockStyle.Fill;
        cboShiftFilter.FlatStyle = FlatStyle.Flat;
        cboShiftFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        cboShiftFilter.Font = new Font("Segoe UI", 9.2F);
        cboShiftFilter.Items.AddRange(new object[]
        {
            "Tất cả ca trực",
            "Ca Sáng (06:00 - 14:00)",
            "Ca Chiều (14:00 - 22:00)",
            "Ca Đêm (22:00 - 06:00)"
        });
        cboShiftFilter.SelectedIndex = 0;
        pnlShiftComboBox.Controls.Add(cboShiftFilter);

        btnSearch.Text = "Tìm kiếm";
        btnSearch.Location = new Point(692, 30);
        btnSearch.Size = new Size(120, 34);
        btnSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSearch.FlatStyle = FlatStyle.Flat;
        btnSearch.FlatAppearance.BorderSize = 0;
        btnSearch.BackColor = Color.FromArgb(37, 99, 235);
        btnSearch.ForeColor = Color.White;
        btnSearch.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnSearch.Cursor = Cursors.Hand;
        btnSearch.Click += BtnSearch_Click;

        btnExportCsv.Text = "Xuất Excel (.csv)";
        btnExportCsv.Location = new Point(822, 30);
        btnExportCsv.Size = new Size(150, 34);
        btnExportCsv.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnExportCsv.FlatStyle = FlatStyle.Flat;
        btnExportCsv.FlatAppearance.BorderSize = 0;
        btnExportCsv.BackColor = Color.FromArgb(241, 245, 249);
        btnExportCsv.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnExportCsv.Cursor = Cursors.Hand;
        btnExportCsv.Click += BtnExportCsv_Click;

        pnlFilterCard.Controls.Add(lblFind);
        pnlFilterCard.Controls.Add(pnlSearchInputBox);
        pnlFilterCard.Controls.Add(lblShift);
        pnlFilterCard.Controls.Add(pnlShiftComboBox);
        pnlFilterCard.Controls.Add(btnSearch);
        pnlFilterCard.Controls.Add(btnExportCsv);

        pnlGridCard.Dock = DockStyle.Fill;
        pnlGridCard.BackColor = Color.White;
        pnlGridCard.Margin = new Padding(6);
        pnlGridCard.Padding = new Padding(16);

        dgvHistory.Dock = DockStyle.Fill;
        RoundedUiHelper.StyleDataGridView(dgvHistory);
        pnlGridCard.Controls.Add(dgvHistory);

        tlpMain.Controls.Add(tlpKpiRow, 0, 0);
        tlpMain.Controls.Add(pnlFilterCard, 0, 1);
        tlpMain.Controls.Add(pnlGridCard, 0, 2);

        AutoScaleMode = AutoScaleMode.Font;
        FormBorderStyle = FormBorderStyle.None;
        Controls.Add(tlpMain);
        Controls.Add(pnlHeader);

        ResumeLayout(false);
    }

    private static Panel CreateKpiCard(string caption, string bigValue, string subText, Color valueColor)
    {
        var p = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(6),
            Padding = new Padding(14, 10, 14, 10)
        };
        var l1 = new Label
        {
            Text = caption,
            Font = new Font("Segoe UI", 8.8F),
            ForeColor = Color.FromArgb(100, 116, 139),
            Location = new Point(14, 10),
            AutoSize = true
        };
        var l2 = new Label
        {
            Text = bigValue,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            ForeColor = valueColor,
            Location = new Point(14, 34),
            AutoSize = true
        };
        var l3 = new Label
        {
            Text = subText,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 116, 139),
            Location = new Point(14, 70),
            AutoSize = true
        };
        p.Controls.Add(l1);
        p.Controls.Add(l2);
        p.Controls.Add(l3);
        return p;
    }
}

namespace BAIDOXETHONGMINH.GUI;

partial class MonthlyRenewForm
{
    private System.ComponentModel.IContainer? components = null;

    private Panel pnlHeader = null!;
    private Label lblTitle = null!;
    private Label lblSubtitle = null!;

    private TableLayoutPanel tlpMain = null!;
    private Panel pnlLeftTable = null!;
    private Label lblTableTitle = null!;
    private Panel pnlSearchBox = null!;
    private TextBox txtSearch = null!;
    private DataGridView dgvPasses = null!;

    private TableLayoutPanel tlpRightActions = null!;
    private Panel pnlRenewCard = null!;
    private Label lblRenewTitle = null!;
    private Label lblSelPassCode = null!;
    private Label lblSelOwner = null!;
    private Label lblSelOldEnd = null!;
    private Panel pnlRenewMonthsBox = null!;
    private ComboBox cboRenewMonths = null!;
    private Panel pnlRenewSummary = null!;
    private Label lblNewEndVal = null!;
    private Label lblRenewFeeVal = null!;
    private Button btnConfirmRenew = null!;

    private Panel pnlUpdateCard = null!;
    private Label lblUpdateTitle = null!;
    private Panel pnlUpdPhoneBox = null!;
    private TextBox txtUpdPhone = null!;
    private Panel pnlUpdBrandBox = null!;
    private TextBox txtUpdBrand = null!;
    private Button btnSaveUpdate = null!;
    private Button btnCancelPass = null!;

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

        pnlLeftTable = new Panel();
        lblTableTitle = new Label();
        pnlSearchBox = new Panel();
        txtSearch = new TextBox();
        dgvPasses = new DataGridView();

        tlpRightActions = new TableLayoutPanel();
        pnlRenewCard = new Panel();
        lblRenewTitle = new Label();
        lblSelPassCode = new Label();
        lblSelOwner = new Label();
        lblSelOldEnd = new Label();
        pnlRenewMonthsBox = new Panel();
        cboRenewMonths = new ComboBox();
        pnlRenewSummary = new Panel();
        lblNewEndVal = new Label();
        lblRenewFeeVal = new Label();
        btnConfirmRenew = new Button();

        pnlUpdateCard = new Panel();
        lblUpdateTitle = new Label();
        pnlUpdPhoneBox = new Panel();
        txtUpdPhone = new TextBox();
        pnlUpdBrandBox = new Panel();
        txtUpdBrand = new TextBox();
        btnSaveUpdate = new Button();
        btnCancelPass = new Button();

        SuspendLayout();

        ClientSize = new Size(1020, 720);

        // Header
        pnlHeader.Size = new Size(1020, 68);
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.BackColor = Color.FromArgb(248, 250, 252);

        lblTitle.Text = "3.2 GIA HẠN & CẬP NHẬT VÉ THÁNG";
        lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblTitle.Location = new Point(16, 10);
        lblTitle.AutoSize = true;

        lblSubtitle.Text = "Thành viên 2 • Chọn 1 vé tháng trong bảng bên trái để Gia hạn thêm tháng, Cập nhật thông tin hoặc Hủy vé";
        lblSubtitle.Font = new Font("Segoe UI", 9F);
        lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
        lblSubtitle.Location = new Point(16, 36);
        lblSubtitle.AutoSize = true;

        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Controls.Add(lblSubtitle);

        // Main 2 Columns (58% Left Table, 42% Right Actions)
        tlpMain.Size = new Size(1020, 652);
        tlpMain.Dock = DockStyle.Fill;
        tlpMain.ColumnCount = 2;
        tlpMain.RowCount = 1;
        tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
        tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
        tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpMain.Padding = new Padding(10, 4, 10, 12);
        tlpMain.BackColor = Color.FromArgb(248, 250, 252);

        // Left Table Panel
        pnlLeftTable.Size = new Size(560, 620);
        pnlLeftTable.Dock = DockStyle.Fill;
        pnlLeftTable.BackColor = Color.White;
        pnlLeftTable.Margin = new Padding(6);
        pnlLeftTable.Padding = new Padding(16);

        lblTableTitle.Text = "Bảng tra cứu & chọn Vé tháng cần cập nhật";
        lblTableTitle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        lblTableTitle.Location = new Point(16, 14);
        lblTableTitle.AutoSize = true;

        pnlSearchBox.Location = new Point(16, 46);
        pnlSearchBox.Size = new Size(526, 34);
        pnlSearchBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pnlSearchBox.BackColor = Color.White;
        pnlSearchBox.Padding = new Padding(10, 7, 10, 5);

        txtSearch.Dock = DockStyle.Fill;
        txtSearch.BorderStyle = BorderStyle.None;
        txtSearch.Font = new Font("Segoe UI", 9.5F);
        txtSearch.PlaceholderText = "Gõ biển số hoặc tên khách hàng để lọc nhanh...";
        pnlSearchBox.Controls.Add(txtSearch);

        dgvPasses.Location = new Point(16, 92);
        dgvPasses.Size = new Size(526, 505);
        dgvPasses.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        RoundedUiHelper.StyleDataGridView(dgvPasses);
        dgvPasses.SelectionChanged += DgvPasses_SelectionChanged;

        pnlLeftTable.Controls.Add(lblTableTitle);
        pnlLeftTable.Controls.Add(pnlSearchBox);
        pnlLeftTable.Controls.Add(dgvPasses);

        // Right Column (2 stacked cards)
        tlpRightActions.Dock = DockStyle.Fill;
        tlpRightActions.ColumnCount = 1;
        tlpRightActions.RowCount = 2;
        tlpRightActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpRightActions.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
        tlpRightActions.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));

        // Card 1: Gia hạn
        pnlRenewCard.Size = new Size(410, 360);
        pnlRenewCard.Dock = DockStyle.Fill;
        pnlRenewCard.BackColor = Color.White;
        pnlRenewCard.Margin = new Padding(6);

        lblRenewTitle.Text = "Gia hạn Vé tháng đang chọn";
        lblRenewTitle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        lblRenewTitle.ForeColor = Color.FromArgb(5, 150, 105);
        lblRenewTitle.Location = new Point(18, 16);
        lblRenewTitle.AutoSize = true;

        AddInfoLine(pnlRenewCard, "Mã vé & Biển số:", out lblSelPassCode, "MP-01 (51H-999.99)", 50);
        AddInfoLine(pnlRenewCard, "Chủ xe:", out lblSelOwner, "Trần Minh Hoàng", 78);
        AddInfoLine(pnlRenewCard, "Ngày hết hạn cũ:", out lblSelOldEnd, "01/11/2026", 106);

        var lblMo = new Label
        {
            Text = "Số tháng muốn gia hạn thêm:",
            Font = new Font("Segoe UI", 8.8F),
            ForeColor = Color.FromArgb(71, 85, 105),
            Location = new Point(18, 138),
            AutoSize = true
        };
        pnlRenewCard.Controls.Add(lblMo);

        pnlRenewMonthsBox.Location = new Point(18, 160);
        pnlRenewMonthsBox.Size = new Size(374, 34);
        pnlRenewMonthsBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pnlRenewMonthsBox.BackColor = Color.White;
        pnlRenewMonthsBox.Padding = new Padding(8, 4, 8, 4);

        cboRenewMonths.Dock = DockStyle.Fill;
        cboRenewMonths.FlatStyle = FlatStyle.Flat;
        cboRenewMonths.DropDownStyle = ComboBoxStyle.DropDownList;
        cboRenewMonths.Font = new Font("Segoe UI", 9.5F);
        cboRenewMonths.Items.AddRange(new object[] { "Gia hạn thêm +1 tháng", "Gia hạn thêm +3 tháng", "Gia hạn thêm +6 tháng" });
        cboRenewMonths.SelectedIndex = 0;
        cboRenewMonths.SelectedIndexChanged += (s, e) => UpdateRenewPreview();
        pnlRenewMonthsBox.Controls.Add(cboRenewMonths);
        pnlRenewCard.Controls.Add(pnlRenewMonthsBox);

        pnlRenewSummary.Location = new Point(18, 206);
        pnlRenewSummary.Size = new Size(374, 64);
        pnlRenewSummary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pnlRenewSummary.BackColor = Color.FromArgb(236, 253, 245);

        AddInfoLine(pnlRenewSummary, "Ngày hết hạn mới:", out lblNewEndVal, "01/12/2026", 10);
        AddInfoLine(pnlRenewSummary, "Số tiền thu gia hạn:", out lblRenewFeeVal, "1.200.000 VNĐ", 36);
        pnlRenewCard.Controls.Add(pnlRenewSummary);

        btnConfirmRenew.Text = "Xác Nhận Gia Hạn & Thu Phí";
        btnConfirmRenew.Location = new Point(18, 282);
        btnConfirmRenew.Size = new Size(374, 42);
        btnConfirmRenew.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        btnConfirmRenew.FlatStyle = FlatStyle.Flat;
        btnConfirmRenew.FlatAppearance.BorderSize = 0;
        btnConfirmRenew.BackColor = Color.FromArgb(5, 150, 105);
        btnConfirmRenew.ForeColor = Color.White;
        btnConfirmRenew.Font = new Font("Segoe UI", 9.8F, FontStyle.Bold);
        btnConfirmRenew.Cursor = Cursors.Hand;
        btnConfirmRenew.Click += BtnConfirmRenew_Click;
        pnlRenewCard.Controls.Add(lblRenewTitle);
        pnlRenewCard.Controls.Add(btnConfirmRenew);

        // Card 2: Cập nhật / Hủy vé
        pnlUpdateCard.Size = new Size(410, 240);
        pnlUpdateCard.Dock = DockStyle.Fill;
        pnlUpdateCard.BackColor = Color.White;
        pnlUpdateCard.Margin = new Padding(6);

        lblUpdateTitle.Text = "Cập nhật thông tin / Hủy vé";
        lblUpdateTitle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        lblUpdateTitle.Location = new Point(18, 16);
        lblUpdateTitle.AutoSize = true;

        var lblPh = new Label { Text = "SĐT liên hệ", Font = new Font("Segoe UI", 8.8F), Location = new Point(18, 48), AutoSize = true };
        var lblBr = new Label { Text = "Mô tả xe", Font = new Font("Segoe UI", 8.8F), Location = new Point(210, 48), AutoSize = true };

        pnlUpdPhoneBox.Location = new Point(18, 70);
        pnlUpdPhoneBox.Size = new Size(180, 34);
        pnlUpdPhoneBox.Padding = new Padding(10, 7, 10, 5);
        txtUpdPhone.Dock = DockStyle.Fill;
        txtUpdPhone.BorderStyle = BorderStyle.None;
        txtUpdPhone.Text = "0908123456";
        pnlUpdPhoneBox.Controls.Add(txtUpdPhone);

        pnlUpdBrandBox.Location = new Point(210, 70);
        pnlUpdBrandBox.Size = new Size(182, 34);
        pnlUpdBrandBox.Padding = new Padding(10, 7, 10, 5);
        txtUpdBrand.Dock = DockStyle.Fill;
        txtUpdBrand.BorderStyle = BorderStyle.None;
        txtUpdBrand.Text = "Mazda CX-5 Trắng";
        pnlUpdBrandBox.Controls.Add(txtUpdBrand);

        btnSaveUpdate.Text = "Lưu thay đổi";
        btnSaveUpdate.Location = new Point(18, 120);
        btnSaveUpdate.Size = new Size(240, 40);
        btnSaveUpdate.FlatStyle = FlatStyle.Flat;
        btnSaveUpdate.FlatAppearance.BorderSize = 0;
        btnSaveUpdate.BackColor = Color.FromArgb(241, 245, 249);
        btnSaveUpdate.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnSaveUpdate.Cursor = Cursors.Hand;
        btnSaveUpdate.Click += (s, e) => MessageBox.Show("Đã cập nhật thông tin vé tháng!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

        btnCancelPass.Text = "Hủy vé";
        btnCancelPass.Location = new Point(268, 120);
        btnCancelPass.Size = new Size(124, 40);
        btnCancelPass.FlatStyle = FlatStyle.Flat;
        btnCancelPass.FlatAppearance.BorderSize = 0;
        btnCancelPass.BackColor = Color.FromArgb(254, 226, 226);
        btnCancelPass.ForeColor = Color.FromArgb(225, 29, 72);
        btnCancelPass.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnCancelPass.Cursor = Cursors.Hand;
        btnCancelPass.Click += (s, e) => MessageBox.Show("Đã chuyển trạng thái vé sang Đã hủy!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        pnlUpdateCard.Controls.Add(lblUpdateTitle);
        pnlUpdateCard.Controls.Add(lblPh);
        pnlUpdateCard.Controls.Add(lblBr);
        pnlUpdateCard.Controls.Add(pnlUpdPhoneBox);
        pnlUpdateCard.Controls.Add(pnlUpdBrandBox);
        pnlUpdateCard.Controls.Add(btnSaveUpdate);
        pnlUpdateCard.Controls.Add(btnCancelPass);

        tlpRightActions.Controls.Add(pnlRenewCard, 0, 0);
        tlpRightActions.Controls.Add(pnlUpdateCard, 0, 1);

        tlpMain.Controls.Add(pnlLeftTable, 0, 0);
        tlpMain.Controls.Add(tlpRightActions, 1, 0);

        AutoScaleMode = AutoScaleMode.Font;
        FormBorderStyle = FormBorderStyle.None;
        Controls.Add(tlpMain);
        Controls.Add(pnlHeader);

        ResumeLayout(false);
    }

    private static void AddInfoLine(Control parent, string labelText, out Label valLbl, string defaultVal, int y)
    {
        var l = new Label
        {
            Text = labelText,
            Font = new Font("Segoe UI", 8.8F),
            ForeColor = Color.FromArgb(100, 116, 139),
            Location = new Point(14, y),
            AutoSize = true
        };
        valLbl = new Label
        {
            Text = defaultVal,
            Font = new Font("Consolas", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Location = new Point(180, y),
            Size = new Size(180, 20),
            TextAlign = ContentAlignment.MiddleRight
        };
        parent.Controls.Add(l);
        parent.Controls.Add(valLbl);
    }
}

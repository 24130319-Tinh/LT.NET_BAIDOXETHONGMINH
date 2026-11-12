#nullable enable
namespace BAIDOXETHONGMINH.GUI;

partial class MonthlyPassForm
{
    private System.ComponentModel.IContainer? components = null;

    private Panel pnlHeader = null!;
    private Label lblTitle = null!;
    private Label lblSubtitle = null!;

    private TableLayoutPanel tlpMain = null!;
    private Panel pnlLeftForm = null!;
    private Label lblFormHeader = null!;

    private Panel pnlNameBox = null!;
    private TextBox txtName = null!;
    private Panel pnlPhoneBox = null!;
    private TextBox txtPhone = null!;
    private Panel pnlCccdBox = null!;
    private TextBox txtCccd = null!;
    private Panel pnlPlateBox = null!;
    private TextBox txtPlate = null!;
    private Panel pnlTypeBox = null!;
    private ComboBox cboVehicleType = null!;
    private Panel pnlBrandBox = null!;
    private TextBox txtBrand = null!;
    private Panel pnlStartDateBox = null!;
    private TextBox txtStartDate = null!;
    private Panel pnlMonthsBox = null!;
    private ComboBox cboMonths = null!;

    private Panel pnlFeeBanner = null!;
    private Label lblFeeValue = null!;
    private Button btnRegister = null!;
    private Button btnClear = null!;

    private Panel pnlRightGrid = null!;
    private Label lblGridHeader = null!;
    private DataGridView dgvMonthlyPasses = null!;

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

        pnlLeftForm = new Panel();
        lblFormHeader = new Label();
        pnlNameBox = new Panel();
        txtName = new TextBox();
        pnlPhoneBox = new Panel();
        txtPhone = new TextBox();
        pnlCccdBox = new Panel();
        txtCccd = new TextBox();
        pnlPlateBox = new Panel();
        txtPlate = new TextBox();
        pnlTypeBox = new Panel();
        cboVehicleType = new ComboBox();
        pnlBrandBox = new Panel();
        txtBrand = new TextBox();
        pnlStartDateBox = new Panel();
        txtStartDate = new TextBox();
        pnlMonthsBox = new Panel();
        cboMonths = new ComboBox();
        pnlFeeBanner = new Panel();
        lblFeeValue = new Label();
        btnRegister = new Button();
        btnClear = new Button();

        pnlRightGrid = new Panel();
        lblGridHeader = new Label();
        dgvMonthlyPasses = new DataGridView();

        SuspendLayout();

        ClientSize = new Size(1020, 720);

        pnlHeader.Size = new Size(1020, 68);
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.BackColor = Color.FromArgb(248, 250, 252);

        lblTitle.Text = "3.1 ĐĂNG KÝ VÉ THÁNG MỚI (CHỈ DÀNH CHO TẠO MỚI)";
        lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblTitle.Location = new Point(16, 10);
        lblTitle.AutoSize = true;

        lblSubtitle.Text = "Thành viên 2 • Tạo mới hồ sơ khách hàng & cấp vé tháng theo biển số xe";
        lblSubtitle.Font = new Font("Segoe UI", 9F);
        lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
        lblSubtitle.Location = new Point(16, 36);
        lblSubtitle.AutoSize = true;

        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Controls.Add(lblSubtitle);

        tlpMain.Size = new Size(1020, 652);
        tlpMain.Dock = DockStyle.Fill;
        tlpMain.ColumnCount = 2;
        tlpMain.RowCount = 1;
        tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
        tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
        tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpMain.Padding = new Padding(10, 4, 10, 12);
        tlpMain.BackColor = Color.FromArgb(248, 250, 252);

        pnlLeftForm.Size = new Size(410, 620);
        pnlLeftForm.Dock = DockStyle.Fill;
        pnlLeftForm.BackColor = Color.White;
        pnlLeftForm.Margin = new Padding(6);

        lblFormHeader.Text = "Nhập Hồ sơ Đăng ký Mới";
        lblFormHeader.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        lblFormHeader.ForeColor = Color.FromArgb(37, 99, 235);
        lblFormHeader.Location = new Point(18, 16);
        lblFormHeader.AutoSize = true;

        AddLabel(pnlLeftForm, "Họ tên chủ xe (*)", 18, 48);
        CreateRoundedTextBox(pnlLeftForm, out pnlNameBox, out txtName, "Nhập họ tên khách hàng...", 18, 70, 374);

        AddLabel(pnlLeftForm, "Số điện thoại (*)", 18, 114);
        CreateRoundedTextBox(pnlLeftForm, out pnlPhoneBox, out txtPhone, "09xxxxxxxx", 18, 136, 180);

        AddLabel(pnlLeftForm, "Số CCCD (*)", 212, 114);
        CreateRoundedTextBox(pnlLeftForm, out pnlCccdBox, out txtCccd, "079xxxxxxxxx", 212, 136, 180);

        AddLabel(pnlLeftForm, "Biển số xe (*)", 18, 180);
        CreateRoundedTextBox(pnlLeftForm, out pnlPlateBox, out txtPlate, "59K1-234.56", 18, 202, 180);

        AddLabel(pnlLeftForm, "Loại xe (*)", 212, 180);
        CreateRoundedComboBox(pnlLeftForm, out pnlTypeBox, out cboVehicleType, 212, 202, 180,
            new object[] { "Xe máy (150k/tháng)", "Ô tô (1.200k/tháng)" });
        cboVehicleType.SelectedIndexChanged += (s, e) => RecalcFee();

        AddLabel(pnlLeftForm, "Hãng xe & Màu sơn", 18, 246);
        CreateRoundedTextBox(pnlLeftForm, out pnlBrandBox, out txtBrand, "VD: Honda AirBlade - Đen", 18, 268, 374);

        AddLabel(pnlLeftForm, "Ngày bắt đầu", 18, 312);
        CreateRoundedTextBox(pnlLeftForm, out pnlStartDateBox, out txtStartDate, "", 18, 334, 180);
        txtStartDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        txtStartDate.ReadOnly = true;

        AddLabel(pnlLeftForm, "Gói đăng ký ban đầu", 212, 312);
        CreateRoundedComboBox(pnlLeftForm, out pnlMonthsBox, out cboMonths, 212, 334, 180,
            new object[] { "1 tháng", "3 tháng", "6 tháng" });
        cboMonths.SelectedIndexChanged += (s, e) => RecalcFee();

        pnlFeeBanner.Location = new Point(18, 384);
        pnlFeeBanner.Size = new Size(374, 46);
        pnlFeeBanner.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pnlFeeBanner.BackColor = Color.FromArgb(239, 246, 255);
        pnlFeeBanner.Padding = new Padding(12, 0, 12, 0);

        var lblFeeCaption = new Label
        {
            Text = "Phí đăng ký mới:",
            Font = new Font("Segoe UI", 9.2F, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 64, 175),
            Dock = DockStyle.Left,
            Width = 150,
            TextAlign = ContentAlignment.MiddleLeft
        };
        lblFeeValue = new Label
        {
            Text = "150.000 VNĐ",
            Font = new Font("Consolas", 12F, FontStyle.Bold),
            ForeColor = Color.FromArgb(37, 99, 235),
            Dock = DockStyle.Right,
            Width = 180,
            TextAlign = ContentAlignment.MiddleRight
        };
        pnlFeeBanner.Controls.Add(lblFeeValue);
        pnlFeeBanner.Controls.Add(lblFeeCaption);
        pnlLeftForm.Controls.Add(pnlFeeBanner);

        btnRegister.Text = "Đăng Ký Vé Tháng Mới";
        btnRegister.Location = new Point(18, 444);
        btnRegister.Size = new Size(274, 42);
        btnRegister.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        btnRegister.FlatStyle = FlatStyle.Flat;
        btnRegister.FlatAppearance.BorderSize = 0;
        btnRegister.BackColor = Color.FromArgb(37, 99, 235);
        btnRegister.ForeColor = Color.White;
        btnRegister.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnRegister.Cursor = Cursors.Hand;
        btnRegister.Click += BtnRegister_Click;

        btnClear.Text = "Làm mới";
        btnClear.Location = new Point(300, 444);
        btnClear.Size = new Size(92, 42);
        btnClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnClear.FlatStyle = FlatStyle.Flat;
        btnClear.FlatAppearance.BorderSize = 0;
        btnClear.BackColor = Color.FromArgb(241, 245, 249);
        btnClear.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnClear.Cursor = Cursors.Hand;
        btnClear.Click += BtnClear_Click;

        pnlLeftForm.Controls.Add(lblFormHeader);
        pnlLeftForm.Controls.Add(btnRegister);
        pnlLeftForm.Controls.Add(btnClear);

        pnlRightGrid.Size = new Size(560, 620);
        pnlRightGrid.Dock = DockStyle.Fill;
        pnlRightGrid.BackColor = Color.White;
        pnlRightGrid.Margin = new Padding(6);
        pnlRightGrid.Padding = new Padding(16);

        lblGridHeader.Text = "Danh sách Vé tháng hiện có (DataGridView)";
        lblGridHeader.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        lblGridHeader.Dock = DockStyle.Top;
        lblGridHeader.Height = 36;

        dgvMonthlyPasses.Dock = DockStyle.Fill;
        RoundedUiHelper.StyleDataGridView(dgvMonthlyPasses);

        pnlRightGrid.Controls.Add(dgvMonthlyPasses);
        pnlRightGrid.Controls.Add(lblGridHeader);

        tlpMain.Controls.Add(pnlLeftForm, 0, 0);
        tlpMain.Controls.Add(pnlRightGrid, 1, 0);

        AutoScaleMode = AutoScaleMode.Font;
        FormBorderStyle = FormBorderStyle.None;
        Controls.Add(tlpMain);
        Controls.Add(pnlHeader);

        ResumeLayout(false);
    }

    private static void AddLabel(Control parent, string text, int x, int y)
    {
        var l = new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 8.8F),
            ForeColor = Color.FromArgb(71, 85, 105),
            Location = new Point(x, y),
            AutoSize = true
        };
        parent.Controls.Add(l);
    }

    private static void CreateRoundedTextBox(Control parent, out Panel box, out TextBox txt, string placeholder, int x, int y, int width)
    {
        box = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(width, 34),
            BackColor = Color.White,
            Padding = new Padding(10, 7, 10, 5)
        };
        txt = new TextBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 9.5F),
            PlaceholderText = placeholder
        };
        box.Controls.Add(txt);
        parent.Controls.Add(box);
    }

    private static void CreateRoundedComboBox(Control parent, out Panel box, out ComboBox cbo, int x, int y, int width, object[] items)
    {
        box = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(width, 34),
            BackColor = Color.White,
            Padding = new Padding(8, 4, 8, 4)
        };
        cbo = new ComboBox
        {
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9.2F)
        };
        cbo.Items.AddRange(items);
        cbo.SelectedIndex = 0;
        box.Controls.Add(cbo);
        parent.Controls.Add(box);
    }
}

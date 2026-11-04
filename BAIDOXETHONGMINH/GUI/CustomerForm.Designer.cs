namespace BAIDOXETHONGMINH.GUI;

partial class CustomerForm
{
    private System.ComponentModel.IContainer? components = null;

    private Panel pnlHeader = null!;
    private Label lblTitle = null!;
    private Label lblSubtitle = null!;

    private TableLayoutPanel tlpMain = null!;
    private Panel pnlLeftForm = null!;
    private Label lblFormHeader = null!;
    private Panel pnlFullNameBox = null!;
    private TextBox txtFullName = null!;
    private Panel pnlPhoneBox = null!;
    private TextBox txtPhone = null!;
    private Panel pnlCccdBox = null!;
    private TextBox txtCccd = null!;
    private Button btnAddCustomer = null!;
    private Button btnUpdateCustomer = null!;
    private Button btnDeleteCustomer = null!;

    private Panel pnlRightGrid = null!;
    private Label lblGridHeader = null!;
    private Panel pnlSearchBox = null!;
    private TextBox txtSearchCustomer = null!;
    private DataGridView dgvCustomers = null!;

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
        pnlFullNameBox = new Panel();
        txtFullName = new TextBox();
        pnlPhoneBox = new Panel();
        txtPhone = new TextBox();
        pnlCccdBox = new Panel();
        txtCccd = new TextBox();
        btnAddCustomer = new Button();
        btnUpdateCustomer = new Button();
        btnDeleteCustomer = new Button();

        pnlRightGrid = new Panel();
        lblGridHeader = new Label();
        pnlSearchBox = new Panel();
        txtSearchCustomer = new TextBox();
        dgvCustomers = new DataGridView();

        SuspendLayout();

        ClientSize = new Size(1020, 720);

        // Header
        pnlHeader.Size = new Size(1020, 68);
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.BackColor = Color.FromArgb(248, 250, 252);

        lblTitle.Text = "QUẢN LÝ HỒ SƠ KHÁCH HÀNG (Customer)";
        lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblTitle.Location = new Point(16, 10);
        lblTitle.AutoSize = true;

        lblSubtitle.Text = "Thành viên 2 • Quản lý thông tin chủ phương tiện đăng ký vé tháng và tra cứu CCCD";
        lblSubtitle.Font = new Font("Segoe UI", 9F);
        lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
        lblSubtitle.Location = new Point(16, 36);
        lblSubtitle.AutoSize = true;

        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Controls.Add(lblSubtitle);

        // Main 2 Columns
        tlpMain.Size = new Size(1020, 652);
        tlpMain.Dock = DockStyle.Fill;
        tlpMain.ColumnCount = 2;
        tlpMain.RowCount = 1;
        tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
        tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));
        tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpMain.Padding = new Padding(10, 4, 10, 12);
        tlpMain.BackColor = Color.FromArgb(248, 250, 252);

        // Left Card
        pnlLeftForm.Size = new Size(370, 620);
        pnlLeftForm.Dock = DockStyle.Fill;
        pnlLeftForm.BackColor = Color.White;
        pnlLeftForm.Margin = new Padding(6);

        lblFormHeader.Text = "Thông tin Khách hàng";
        lblFormHeader.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        lblFormHeader.ForeColor = Color.FromArgb(37, 99, 235);
        lblFormHeader.Location = new Point(18, 16);
        lblFormHeader.AutoSize = true;

        AddLabel(pnlLeftForm, "Họ và tên khách hàng (*)", 18, 56);
        CreateRoundedBox(pnlLeftForm, out pnlFullNameBox, out txtFullName, "Nhập họ và tên...", 18, 78, 330);

        AddLabel(pnlLeftForm, "Số điện thoại liên hệ (*)", 18, 128);
        CreateRoundedBox(pnlLeftForm, out pnlPhoneBox, out txtPhone, "09xxxxxxxx", 18, 150, 330);

        AddLabel(pnlLeftForm, "Số CCCD / CMND (*)", 18, 200);
        CreateRoundedBox(pnlLeftForm, out pnlCccdBox, out txtCccd, "079xxxxxxxxx", 18, 222, 330);

        btnAddCustomer.Text = "Thêm Khách Hàng";
        btnAddCustomer.Location = new Point(18, 282);
        btnAddCustomer.Size = new Size(330, 42);
        btnAddCustomer.FlatStyle = FlatStyle.Flat;
        btnAddCustomer.FlatAppearance.BorderSize = 0;
        btnAddCustomer.BackColor = Color.FromArgb(37, 99, 235);
        btnAddCustomer.ForeColor = Color.White;
        btnAddCustomer.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnAddCustomer.Cursor = Cursors.Hand;
        btnAddCustomer.Click += BtnAddCustomer_Click;

        btnUpdateCustomer.Text = "Cập Nhật Thông Tin";
        btnUpdateCustomer.Location = new Point(18, 334);
        btnUpdateCustomer.Size = new Size(210, 40);
        btnUpdateCustomer.FlatStyle = FlatStyle.Flat;
        btnUpdateCustomer.FlatAppearance.BorderSize = 0;
        btnUpdateCustomer.BackColor = Color.FromArgb(241, 245, 249);
        btnUpdateCustomer.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnUpdateCustomer.Cursor = Cursors.Hand;
        btnUpdateCustomer.Click += BtnUpdateCustomer_Click;

        btnDeleteCustomer.Text = "Xóa";
        btnDeleteCustomer.Location = new Point(236, 334);
        btnDeleteCustomer.Size = new Size(112, 40);
        btnDeleteCustomer.FlatStyle = FlatStyle.Flat;
        btnDeleteCustomer.FlatAppearance.BorderSize = 0;
        btnDeleteCustomer.BackColor = Color.FromArgb(254, 226, 226);
        btnDeleteCustomer.ForeColor = Color.FromArgb(225, 29, 72);
        btnDeleteCustomer.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnDeleteCustomer.Cursor = Cursors.Hand;
        btnDeleteCustomer.Click += BtnDeleteCustomer_Click;

        pnlLeftForm.Controls.Add(lblFormHeader);
        pnlLeftForm.Controls.Add(btnAddCustomer);
        pnlLeftForm.Controls.Add(btnUpdateCustomer);
        pnlLeftForm.Controls.Add(btnDeleteCustomer);

        // Right Card
        pnlRightGrid.Size = new Size(600, 620);
        pnlRightGrid.Dock = DockStyle.Fill;
        pnlRightGrid.BackColor = Color.White;
        pnlRightGrid.Margin = new Padding(6);
        pnlRightGrid.Padding = new Padding(16);

        lblGridHeader.Text = "Danh sách Hồ sơ Khách hàng";
        lblGridHeader.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        lblGridHeader.Location = new Point(16, 14);
        lblGridHeader.AutoSize = true;

        pnlSearchBox.Location = new Point(16, 46);
        pnlSearchBox.Size = new Size(566, 34);
        pnlSearchBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pnlSearchBox.BackColor = Color.White;
        pnlSearchBox.Padding = new Padding(10, 7, 10, 5);

        txtSearchCustomer.Dock = DockStyle.Fill;
        txtSearchCustomer.BorderStyle = BorderStyle.None;
        txtSearchCustomer.Font = new Font("Segoe UI", 9.5F);
        txtSearchCustomer.PlaceholderText = "Tìm kiếm khách hàng theo Họ tên, SĐT hoặc CCCD...";
        pnlSearchBox.Controls.Add(txtSearchCustomer);

        dgvCustomers.Location = new Point(16, 92);
        dgvCustomers.Size = new Size(566, 505);
        dgvCustomers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        RoundedUiHelper.StyleDataGridView(dgvCustomers);
        dgvCustomers.SelectionChanged += DgvCustomers_SelectionChanged;

        pnlRightGrid.Controls.Add(lblGridHeader);
        pnlRightGrid.Controls.Add(pnlSearchBox);
        pnlRightGrid.Controls.Add(dgvCustomers);

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

    private static void CreateRoundedBox(Control parent, out Panel box, out TextBox txt, string placeholder, int x, int y, int width)
    {
        box = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(width, 36),
            BackColor = Color.White,
            Padding = new Padding(10, 8, 10, 6)
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
}

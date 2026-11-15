#nullable enable
namespace BAIDOXETHONGMINH.GUI;

partial class MenuForm
{
    private System.ComponentModel.IContainer? components = null;

    private Panel pnlSidebar = null!;
    private Panel pnlLogo = null!;
    private Label lblAppName = null!;
    private Label lblAppSub = null!;
    private FlowLayoutPanel flpMenu = null!;
    private Button btnCheckInOut = null!;
    private Button btnParkingMap = null!;
    private Label lblMonthlyGroup = null!;
    private Button btnMonthlyRegister = null!;
    private Button btnMonthlyRenew = null!;
    private Button btnCustomer = null!;
    private Button btnSearchReport = null!;
    private Panel pnlShiftCard = null!;
    private Label lblShiftHeader = null!;
    private Label lblShiftAdmin = null!;
    private Label lblShiftTime = null!;
    private Label lblShiftSlots = null!;
    private PictureBox picAvatar = null!;

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
        pnlSidebar = new Panel();
        pnlLogo = new Panel();
        lblAppName = new Label();
        lblAppSub = new Label();
        flpMenu = new FlowLayoutPanel();
        btnCheckInOut = new Button();
        btnParkingMap = new Button();
        lblMonthlyGroup = new Label();
        btnMonthlyRegister = new Button();
        btnMonthlyRenew = new Button();
        btnCustomer = new Button();
        btnSearchReport = new Button();
        pnlShiftCard = new Panel();
        lblShiftHeader = new Label();
        lblShiftAdmin = new Label();
        lblShiftTime = new Label();
        lblShiftSlots = new Label();
        picAvatar = new PictureBox();

        SuspendLayout();

        pnlSidebar.BackColor = Color.FromArgb(24, 30, 44);
        pnlSidebar.Dock = DockStyle.Fill;
        pnlSidebar.Padding = new Padding(12);
        pnlSidebar.Controls.Add(flpMenu);
        pnlSidebar.Controls.Add(pnlLogo);
        pnlSidebar.Controls.Add(pnlShiftCard);

        pnlLogo.Dock = DockStyle.Top;
        pnlLogo.Height = 66;

        lblAppName.Text = "BÃI ĐỖ XE THÔNG MINH";
        lblAppName.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblAppName.ForeColor = Color.White;
        lblAppName.Location = new Point(6, 10);
        lblAppName.AutoSize = true;

        lblAppSub.Text = "Menu Quản Lý Desktop";
        lblAppSub.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
        lblAppSub.ForeColor = Color.FromArgb(148, 163, 184);
        lblAppSub.Location = new Point(6, 36);
        lblAppSub.AutoSize = true;

        pnlLogo.Controls.Add(lblAppName);
        pnlLogo.Controls.Add(lblAppSub);

        flpMenu.Dock = DockStyle.Fill;
        flpMenu.FlowDirection = FlowDirection.TopDown;
        flpMenu.WrapContents = false;
        flpMenu.AutoScroll = true;
        flpMenu.Padding = new Padding(0, 8, 0, 8);

        ConfigureMenuButton(btnCheckInOut, "1. Kiểm soát Xe Vào / Ra", false);
        btnCheckInOut.Click += (s, e) => SelectMenuItem(btnCheckInOut, "CheckInOut");

        ConfigureMenuButton(btnParkingMap, "2. Sơ đồ bãi & Bảng giá", false);
        btnParkingMap.Click += (s, e) => SelectMenuItem(btnParkingMap, "ParkingMap");

        lblMonthlyGroup.Text = "3. QUẢN LÝ VÉ THÁNG";
        lblMonthlyGroup.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        lblMonthlyGroup.ForeColor = Color.FromArgb(148, 163, 184);
        lblMonthlyGroup.Margin = new Padding(6, 12, 0, 4);
        lblMonthlyGroup.AutoSize = true;

        ConfigureMenuButton(btnMonthlyRegister, "   ↳ 3.1 Đăng ký Vé tháng mới", true);
        btnMonthlyRegister.Click += (s, e) => SelectMenuItem(btnMonthlyRegister, "MonthlyRegister");

        ConfigureMenuButton(btnMonthlyRenew, "   ↳ 3.2 Gia hạn & Cập nhật vé", true);
        btnMonthlyRenew.Click += (s, e) => SelectMenuItem(btnMonthlyRenew, "MonthlyRenew");

        ConfigureMenuButton(btnCustomer, "4. Quản lý Khách hàng", false);
        btnCustomer.Click += (s, e) => SelectMenuItem(btnCustomer, "Customer");

        ConfigureMenuButton(btnSearchReport, "5. Tra cứu & Thống kê", false);
        btnSearchReport.Click += (s, e) => SelectMenuItem(btnSearchReport, "SearchReport");

        flpMenu.Controls.Add(btnCheckInOut);
        flpMenu.Controls.Add(btnParkingMap);
        flpMenu.Controls.Add(lblMonthlyGroup);
        flpMenu.Controls.Add(btnMonthlyRegister);
        flpMenu.Controls.Add(btnMonthlyRenew);
        flpMenu.Controls.Add(btnCustomer);
        flpMenu.Controls.Add(btnSearchReport);

        pnlShiftCard.Dock = DockStyle.Bottom;
        pnlShiftCard.Height = 100;
        pnlShiftCard.BackColor = Color.FromArgb(30, 41, 59);
        pnlShiftCard.Padding = new Padding(10);

        lblShiftHeader.Text = "Thông tin Ca trực";
        lblShiftHeader.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblShiftHeader.ForeColor = Color.White;
        lblShiftHeader.Location = new Point(10, 10);
        lblShiftHeader.AutoSize = true;

        lblShiftAdmin.Text = "Quản lý: Admin Tính";
        lblShiftAdmin.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
        lblShiftAdmin.ForeColor = Color.FromArgb(203, 213, 225);
        lblShiftAdmin.Location = new Point(10, 32);
        lblShiftAdmin.AutoSize = true;

        lblShiftTime.Text = "Ca: Ca Sáng (06h-14h)";
        lblShiftTime.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        lblShiftTime.ForeColor = Color.FromArgb(52, 211, 153);
        lblShiftTime.Location = new Point(10, 52);
        lblShiftTime.AutoSize = true;

        lblShiftSlots.Text = "Chỗ trống: 13 / 20 ô";
        lblShiftSlots.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
        lblShiftSlots.ForeColor = Color.FromArgb(203, 213, 225);
        lblShiftSlots.Location = new Point(10, 72);
        lblShiftSlots.AutoSize = true;

        picAvatar.Size = new Size(56, 56);
        picAvatar.Location = new Point(168, 22);
        picAvatar.SizeMode = PictureBoxSizeMode.Zoom;
        picAvatar.Paint += PicAvatar_Paint;

        pnlShiftCard.Controls.Add(lblShiftHeader);
        pnlShiftCard.Controls.Add(lblShiftAdmin);
        pnlShiftCard.Controls.Add(lblShiftTime);
        pnlShiftCard.Controls.Add(lblShiftSlots);
        pnlShiftCard.Controls.Add(picAvatar);

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(260, 760);
        FormBorderStyle = FormBorderStyle.None;
        Controls.Add(pnlSidebar);

        ResumeLayout(false);
    }

    private static void ConfigureMenuButton(Button btn, string text, bool isSubItem)
    {
        btn.Text = text;
        btn.Width = 232;
        btn.Height = isSubItem ? 38 : 42;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.TextAlign = ContentAlignment.MiddleLeft;
        btn.Font = new Font("Segoe UI", isSubItem ? 9.2F : 9.8F, FontStyle.Regular);
        btn.ForeColor = Color.FromArgb(226, 232, 240);
        btn.BackColor = Color.FromArgb(24, 30, 44);
        btn.Cursor = Cursors.Hand;
        btn.Margin = new Padding(0, 2, 0, 2);
    }
}

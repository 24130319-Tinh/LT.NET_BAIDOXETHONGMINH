namespace BAIDOXETHONGMINH.GUI;

partial class CheckInOutForm
{
    private System.ComponentModel.IContainer? components = null;

    private Panel pnlHeader = null!;
    private Label lblTitle = null!;
    private Label lblSubtitle = null!;
    private Label lblShortcutBadge = null!;

    private TableLayoutPanel tlpMainColumns = null!;

    // Cột trái: Làn Vào
    private Panel pnlCheckIn = null!;
    private Label lblCheckInHeader = null!;
    private PictureBox picVehicleIn = null!;
    private Panel pnlImagePathBox = null!;
    private TextBox txtImagePathIn = null!;
    private Button btnBrowseImage = null!;
    private TableLayoutPanel tlpInputsIn = null!;
    private Label lblPlateIn = null!;
    private Panel pnlPlateInBox = null!;
    private TextBox txtPlateIn = null!;
    private Label lblVehicleTypeIn = null!;
    private Panel pnlVehicleTypeInBox = null!;
    private ComboBox cboVehicleTypeIn = null!;
    private Label lblCardIn = null!;
    private Panel pnlCardInBox = null!;
    private ComboBox cboCardIn = null!;
    private Label lblSlotIn = null!;
    private Panel pnlSlotInBox = null!;
    private TextBox txtSlotIn = null!;
    private Panel pnlPassStatusBanner = null!;
    private Label lblPassStatusIn = null!;
    private Label lblPassTestHint = null!;
    private Button btnSubmitCheckIn = null!;
    private Button btnResetCheckIn = null!;

    // Cột phải: Làn Ra
    private Panel pnlCheckOut = null!;
    private Label lblCheckOutHeader = null!;
    private Panel pnlSearchSessionBox = null!;
    private ComboBox cboSearchSession = null!;
    private TableLayoutPanel tlpCompareOut = null!;
    private Panel pnlSavedPhotoCard = null!;
    private Label lblSavedPhotoTitle = null!;
    private Panel pnlSavedPhotoInner = null!;
    private Label lblSavedFileName = null!;
    private Label lblSavedPlate = null!;
    private Label lblSavedTimeIn = null!;
    private Panel pnlSessionDetailsCard = null!;
    private TableLayoutPanel tlpDetailsRows = null!;
    private Label lblOutCardVal = null!;
    private Label lblOutPlateVal = null!;
    private Label lblOutTypeSlotVal = null!;
    private Label lblOutDurationVal = null!;
    private Label lblOutTicketTypeVal = null!;
    private Panel pnlLostCardBox = null!;
    private CheckBox chkLostCard = null!;
    private Panel pnlTotalBilling = null!;
    private Label lblTotalTitle = null!;
    private Label lblFeeBreakdown = null!;
    private Label lblTotalAmount = null!;
    private Button btnSubmitCheckOut = null!;

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
        lblShortcutBadge = new Label();
        tlpMainColumns = new TableLayoutPanel();

        pnlCheckIn = new Panel();
        lblCheckInHeader = new Label();
        picVehicleIn = new PictureBox();
        pnlImagePathBox = new Panel();
        txtImagePathIn = new TextBox();
        btnBrowseImage = new Button();
        tlpInputsIn = new TableLayoutPanel();
        lblPlateIn = new Label();
        pnlPlateInBox = new Panel();
        txtPlateIn = new TextBox();
        lblVehicleTypeIn = new Label();
        pnlVehicleTypeInBox = new Panel();
        cboVehicleTypeIn = new ComboBox();
        lblCardIn = new Label();
        pnlCardInBox = new Panel();
        cboCardIn = new ComboBox();
        lblSlotIn = new Label();
        pnlSlotInBox = new Panel();
        txtSlotIn = new TextBox();
        pnlPassStatusBanner = new Panel();
        lblPassStatusIn = new Label();
        lblPassTestHint = new Label();
        btnSubmitCheckIn = new Button();
        btnResetCheckIn = new Button();

        pnlCheckOut = new Panel();
        lblCheckOutHeader = new Label();
        pnlSearchSessionBox = new Panel();
        cboSearchSession = new ComboBox();
        tlpCompareOut = new TableLayoutPanel();
        pnlSavedPhotoCard = new Panel();
        lblSavedPhotoTitle = new Label();
        pnlSavedPhotoInner = new Panel();
        lblSavedFileName = new Label();
        lblSavedPlate = new Label();
        lblSavedTimeIn = new Label();
        pnlSessionDetailsCard = new Panel();
        tlpDetailsRows = new TableLayoutPanel();
        lblOutCardVal = new Label();
        lblOutPlateVal = new Label();
        lblOutTypeSlotVal = new Label();
        lblOutDurationVal = new Label();
        lblOutTicketTypeVal = new Label();
        pnlLostCardBox = new Panel();
        chkLostCard = new CheckBox();
        pnlTotalBilling = new Panel();
        lblTotalTitle = new Label();
        lblFeeBreakdown = new Label();
        lblTotalAmount = new Label();
        btnSubmitCheckOut = new Button();

        SuspendLayout();

        ClientSize = new Size(1020, 720);

        // 
        // pnlHeader (Thanh tiêu đề trên cùng)
        // 
        pnlHeader.Size = new Size(1020, 68);
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.BackColor = Color.FromArgb(248, 250, 252);
        pnlHeader.Padding = new Padding(16, 10, 16, 6);

        lblTitle.Text = "BÀN ĐIỀU KHIỂN XE VÀO / XE RA (CHỌN HÌNH TỪ MÁY & NHẬP THỦ CÔNG)";
        lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblTitle.Location = new Point(16, 10);
        lblTitle.AutoSize = true;

        lblSubtitle.Text = "Thành viên 1 • Không dùng Camera — Dùng OpenFileDialog chọn file ảnh xe và nhập tay các trường thông tin";
        lblSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
        lblSubtitle.Location = new Point(16, 36);
        lblSubtitle.AutoSize = true;

        lblShortcutBadge.Text = "Phím tắt PC: F1 / F2";
        lblShortcutBadge.Font = new Font("Consolas", 9F, FontStyle.Bold);
        lblShortcutBadge.ForeColor = Color.FromArgb(51, 65, 85);
        lblShortcutBadge.BackColor = Color.White;
        lblShortcutBadge.TextAlign = ContentAlignment.MiddleCenter;
        lblShortcutBadge.Size = new Size(168, 32);
        lblShortcutBadge.Location = new Point(832, 16);
        lblShortcutBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        pnlHeader.Controls.Add(lblShortcutBadge);
        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Controls.Add(lblSubtitle);

        // 
        // tlpMainColumns (Chia 2 cột 50% - 50%)
        // 
        tlpMainColumns.Size = new Size(1020, 652);
        tlpMainColumns.Dock = DockStyle.Fill;
        tlpMainColumns.ColumnCount = 2;
        tlpMainColumns.RowCount = 1;
        tlpMainColumns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tlpMainColumns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tlpMainColumns.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpMainColumns.Padding = new Padding(10, 4, 10, 12);
        tlpMainColumns.BackColor = Color.FromArgb(248, 250, 252);

        // =========================================================
        // CỘT TRÁI: LÀN VÀO — GHI NHẬN XE VÀO BÃI
        // =========================================================
        pnlCheckIn.Size = new Size(480, 620);
        pnlCheckIn.Dock = DockStyle.Fill;
        pnlCheckIn.BackColor = Color.White;
        pnlCheckIn.Margin = new Padding(6);

        lblCheckInHeader.Text = "LÀN VÀO — GHI NHẬN XE VÀO BÃI";
        lblCheckInHeader.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        lblCheckInHeader.ForeColor = Color.FromArgb(37, 99, 235);
        lblCheckInHeader.Location = new Point(18, 16);
        lblCheckInHeader.AutoSize = true;

        picVehicleIn.Location = new Point(18, 46);
        picVehicleIn.Size = new Size(444, 168);
        picVehicleIn.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        picVehicleIn.BackColor = Color.FromArgb(248, 250, 252);
        picVehicleIn.SizeMode = PictureBoxSizeMode.Zoom;
        picVehicleIn.Cursor = Cursors.Hand;
        picVehicleIn.Paint += PicVehicleIn_Paint;
        picVehicleIn.Click += BtnBrowseImage_Click;

        // Khung bo tròn bọc ô đường dẫn ảnh
        pnlImagePathBox.Location = new Point(18, 223);
        pnlImagePathBox.Size = new Size(302, 34);
        pnlImagePathBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pnlImagePathBox.BackColor = Color.White;
        pnlImagePathBox.Padding = new Padding(10, 8, 10, 6);

        txtImagePathIn.Dock = DockStyle.Fill;
        txtImagePathIn.BorderStyle = BorderStyle.None;
        txtImagePathIn.BackColor = Color.White;
        txtImagePathIn.ReadOnly = true;
        txtImagePathIn.Font = new Font("Consolas", 9F);
        txtImagePathIn.PlaceholderText = @"Đường dẫn file ảnh (C:\Images\xe_01.jpg)...";
        pnlImagePathBox.Controls.Add(txtImagePathIn);

        btnBrowseImage.Text = "Chọn hình từ máy...";
        btnBrowseImage.Location = new Point(328, 223);
        btnBrowseImage.Size = new Size(134, 34);
        btnBrowseImage.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowseImage.FlatStyle = FlatStyle.Flat;
        btnBrowseImage.FlatAppearance.BorderSize = 0;
        btnBrowseImage.BackColor = Color.FromArgb(241, 245, 249);
        btnBrowseImage.Font = new Font("Segoe UI", 8.8F, FontStyle.Bold);
        btnBrowseImage.Cursor = Cursors.Hand;
        btnBrowseImage.Click += BtnBrowseImage_Click;

        // Bảng 2x2 cho 4 trường nhập thủ công (Tất cả đều bọc trong Panel bo tròn)
        tlpInputsIn.Location = new Point(18, 265);
        tlpInputsIn.Size = new Size(444, 134);
        tlpInputsIn.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        tlpInputsIn.ColumnCount = 2;
        tlpInputsIn.RowCount = 4;
        tlpInputsIn.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tlpInputsIn.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tlpInputsIn.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
        tlpInputsIn.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        tlpInputsIn.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
        tlpInputsIn.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));

        lblPlateIn.Text = "Nhập Biển số xe (*)";
        lblPlateIn.Font = new Font("Segoe UI", 9F);
        lblPlateIn.Dock = DockStyle.Fill;
        lblPlateIn.TextAlign = ContentAlignment.BottomLeft;

        lblVehicleTypeIn.Text = "Chọn Loại xe (*)";
        lblVehicleTypeIn.Font = new Font("Segoe UI", 9F);
        lblVehicleTypeIn.Dock = DockStyle.Fill;
        lblVehicleTypeIn.TextAlign = ContentAlignment.BottomLeft;
        lblVehicleTypeIn.Margin = new Padding(8, 0, 0, 0);

        pnlPlateInBox.Dock = DockStyle.Fill;
        pnlPlateInBox.Margin = new Padding(0, 4, 6, 4);
        pnlPlateInBox.BackColor = Color.White;
        pnlPlateInBox.Padding = new Padding(10, 8, 10, 6);

        txtPlateIn.Dock = DockStyle.Fill;
        txtPlateIn.BorderStyle = BorderStyle.None;
        txtPlateIn.Font = new Font("Consolas", 10.5F, FontStyle.Bold);
        txtPlateIn.PlaceholderText = "VD: 59A1-686.86";
        txtPlateIn.TextChanged += TxtPlateIn_TextChanged;
        pnlPlateInBox.Controls.Add(txtPlateIn);

        pnlVehicleTypeInBox.Dock = DockStyle.Fill;
        pnlVehicleTypeInBox.Margin = new Padding(8, 4, 0, 4);
        pnlVehicleTypeInBox.BackColor = Color.White;
        pnlVehicleTypeInBox.Padding = new Padding(8, 5, 8, 4);

        cboVehicleTypeIn.Dock = DockStyle.Fill;
        cboVehicleTypeIn.FlatStyle = FlatStyle.Flat;
        cboVehicleTypeIn.DropDownStyle = ComboBoxStyle.DropDownList;
        cboVehicleTypeIn.Font = new Font("Segoe UI", 9.8F);
        cboVehicleTypeIn.Items.AddRange(new object[] { "Xe máy (Khu A)", "Ô tô (Khu B)" });
        cboVehicleTypeIn.SelectedIndex = 0;
        cboVehicleTypeIn.SelectedIndexChanged += CboVehicleTypeIn_SelectedIndexChanged;
        pnlVehicleTypeInBox.Controls.Add(cboVehicleTypeIn);

        lblCardIn.Text = "Chọn Mã thẻ cấp (*)";
        lblCardIn.Font = new Font("Segoe UI", 9F);
        lblCardIn.Dock = DockStyle.Fill;
        lblCardIn.TextAlign = ContentAlignment.BottomLeft;

        lblSlotIn.Text = "Chọn Vị trí đỗ (*)";
        lblSlotIn.Font = new Font("Segoe UI", 9F);
        lblSlotIn.Dock = DockStyle.Fill;
        lblSlotIn.TextAlign = ContentAlignment.BottomLeft;
        lblSlotIn.Margin = new Padding(8, 0, 0, 0);

        pnlCardInBox.Dock = DockStyle.Fill;
        pnlCardInBox.Margin = new Padding(0, 4, 6, 4);
        pnlCardInBox.BackColor = Color.White;
        pnlCardInBox.Padding = new Padding(8, 5, 8, 4);

        cboCardIn.Dock = DockStyle.Fill;
        cboCardIn.FlatStyle = FlatStyle.Flat;
        cboCardIn.DropDownStyle = ComboBoxStyle.DropDownList;
        cboCardIn.Font = new Font("Consolas", 9.8F);
        cboCardIn.Items.AddRange(new object[] { "THE008 (Trống)", "THE009 (Trống)", "THE010 (Trống)" });
        cboCardIn.SelectedIndex = 0;
        pnlCardInBox.Controls.Add(cboCardIn);

        pnlSlotInBox.Dock = DockStyle.Fill;
        pnlSlotInBox.Margin = new Padding(8, 4, 0, 4);
        pnlSlotInBox.BackColor = Color.White;
        pnlSlotInBox.Padding = new Padding(10, 8, 10, 6);

        txtSlotIn.Dock = DockStyle.Fill;
        txtSlotIn.BorderStyle = BorderStyle.None;
        txtSlotIn.Font = new Font("Segoe UI", 9.8F, FontStyle.Bold);
        txtSlotIn.Text = "A-03 (Trống)";
        pnlSlotInBox.Controls.Add(txtSlotIn);

        tlpInputsIn.Controls.Add(lblPlateIn, 0, 0);
        tlpInputsIn.Controls.Add(lblVehicleTypeIn, 1, 0);
        tlpInputsIn.Controls.Add(pnlPlateInBox, 0, 1);
        tlpInputsIn.Controls.Add(pnlVehicleTypeInBox, 1, 1);
        tlpInputsIn.Controls.Add(lblCardIn, 0, 2);
        tlpInputsIn.Controls.Add(lblSlotIn, 1, 2);
        tlpInputsIn.Controls.Add(pnlCardInBox, 0, 3);
        tlpInputsIn.Controls.Add(pnlSlotInBox, 1, 3);

        // Banner đối chiếu vé tháng (Bo góc)
        pnlPassStatusBanner.Location = new Point(18, 408);
        pnlPassStatusBanner.Size = new Size(444, 40);
        pnlPassStatusBanner.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pnlPassStatusBanner.BackColor = Color.FromArgb(239, 246, 255);
        pnlPassStatusBanner.Padding = new Padding(12, 4, 12, 4);

        lblPassStatusIn.Text = "Đối chiếu Vé tháng: Chưa nhập biển số";
        lblPassStatusIn.Font = new Font("Segoe UI", 8.8F, FontStyle.Bold);
        lblPassStatusIn.ForeColor = Color.FromArgb(30, 64, 175);
        lblPassStatusIn.Dock = DockStyle.Left;
        lblPassStatusIn.AutoSize = false;
        lblPassStatusIn.Width = 265;
        lblPassStatusIn.TextAlign = ContentAlignment.MiddleLeft;

        lblPassTestHint.Text = "Test mã: 51H-999.99";
        lblPassTestHint.Font = new Font("Consolas", 8.5F, FontStyle.Bold);
        lblPassTestHint.ForeColor = Color.FromArgb(71, 85, 105);
        lblPassTestHint.Dock = DockStyle.Right;
        lblPassTestHint.AutoSize = false;
        lblPassTestHint.Width = 145;
        lblPassTestHint.TextAlign = ContentAlignment.MiddleRight;

        pnlPassStatusBanner.Controls.Add(lblPassTestHint);
        pnlPassStatusBanner.Controls.Add(lblPassStatusIn);

        // Nút Lưu Xe Vào & Xóa trắng
        btnSubmitCheckIn.Text = "[F1] Cho Xe Vào Bãi & Lưu CSDL";
        btnSubmitCheckIn.Location = new Point(18, 462);
        btnSubmitCheckIn.Size = new Size(344, 44);
        btnSubmitCheckIn.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        btnSubmitCheckIn.FlatStyle = FlatStyle.Flat;
        btnSubmitCheckIn.FlatAppearance.BorderSize = 0;
        btnSubmitCheckIn.BackColor = Color.FromArgb(37, 99, 235);
        btnSubmitCheckIn.ForeColor = Color.White;
        btnSubmitCheckIn.Font = new Font("Segoe UI", 9.8F, FontStyle.Bold);
        btnSubmitCheckIn.Cursor = Cursors.Hand;
        btnSubmitCheckIn.Click += BtnSubmitCheckIn_Click;

        btnResetCheckIn.Text = "Xóa trắng";
        btnResetCheckIn.Location = new Point(370, 462);
        btnResetCheckIn.Size = new Size(92, 44);
        btnResetCheckIn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnResetCheckIn.FlatStyle = FlatStyle.Flat;
        btnResetCheckIn.FlatAppearance.BorderSize = 0;
        btnResetCheckIn.BackColor = Color.White;
        btnResetCheckIn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnResetCheckIn.Cursor = Cursors.Hand;
        btnResetCheckIn.Click += BtnResetCheckIn_Click;

        pnlCheckIn.Controls.Add(lblCheckInHeader);
        pnlCheckIn.Controls.Add(picVehicleIn);
        pnlCheckIn.Controls.Add(pnlImagePathBox);
        pnlCheckIn.Controls.Add(btnBrowseImage);
        pnlCheckIn.Controls.Add(tlpInputsIn);
        pnlCheckIn.Controls.Add(pnlPassStatusBanner);
        pnlCheckIn.Controls.Add(btnSubmitCheckIn);
        pnlCheckIn.Controls.Add(btnResetCheckIn);

        // =========================================================
        // CỘT PHẢI: LÀN RA — ĐỐI CHIẾU HÌNH & TÍNH TIỀN
        // =========================================================
        pnlCheckOut.Size = new Size(480, 620);
        pnlCheckOut.Dock = DockStyle.Fill;
        pnlCheckOut.BackColor = Color.White;
        pnlCheckOut.Margin = new Padding(6);

        lblCheckOutHeader.Text = "LÀN RA — ĐỐI CHIẾU HÌNH & TÍNH TIỀN";
        lblCheckOutHeader.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        lblCheckOutHeader.ForeColor = Color.FromArgb(5, 150, 105);
        lblCheckOutHeader.Location = new Point(18, 16);
        lblCheckOutHeader.AutoSize = true;

        // Khung bo tròn bọc ComboBox tìm kiếm lượt vào
        pnlSearchSessionBox.Location = new Point(18, 46);
        pnlSearchSessionBox.Size = new Size(444, 36);
        pnlSearchSessionBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pnlSearchSessionBox.BackColor = Color.White;
        pnlSearchSessionBox.Padding = new Padding(8, 5, 8, 4);

        cboSearchSession.Dock = DockStyle.Fill;
        cboSearchSession.FlatStyle = FlatStyle.Flat;
        cboSearchSession.DropDownStyle = ComboBoxStyle.DropDownList;
        cboSearchSession.Font = new Font("Consolas", 9.2F);
        cboSearchSession.Items.AddRange(new object[]
        {
            "Nhập mã thẻ: THE003 — Biển số 51H-999.99 (Ô tô • Vé tháng)",
            "Nhập mã thẻ: THE002 — Biển số 51G-456.78 (Ô tô • Vé lượt 3h05p)",
            "Nhập mã thẻ: THE001 — Biển số 59X3-112.23 (Xe máy • Vé lượt ngày)"
        });
        cboSearchSession.SelectedIndexChanged += CboSearchSession_SelectedIndexChanged;
        pnlSearchSessionBox.Controls.Add(cboSearchSession);

        // Khung 2 cột đối chiếu Hình đã lưu & Thông tin lượt vào
        tlpCompareOut.Location = new Point(18, 94);
        tlpCompareOut.Size = new Size(444, 175);
        tlpCompareOut.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        tlpCompareOut.ColumnCount = 2;
        tlpCompareOut.RowCount = 1;
        tlpCompareOut.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tlpCompareOut.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tlpCompareOut.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        pnlSavedPhotoCard.Dock = DockStyle.Fill;
        pnlSavedPhotoCard.BackColor = Color.FromArgb(248, 250, 252);
        pnlSavedPhotoCard.Margin = new Padding(0, 0, 6, 0);
        pnlSavedPhotoCard.Padding = new Padding(12, 8, 12, 8);

        lblSavedPhotoTitle.Text = "HÌNH ĐÃ LƯU LÚC VÀO";
        lblSavedPhotoTitle.Font = new Font("Segoe UI", 8.8F, FontStyle.Bold);
        lblSavedPhotoTitle.ForeColor = Color.FromArgb(71, 85, 105);
        lblSavedPhotoTitle.Dock = DockStyle.Top;
        lblSavedPhotoTitle.Height = 28;
        lblSavedPhotoTitle.TextAlign = ContentAlignment.MiddleCenter;

        lblSavedTimeIn.Text = "Vào: 29/10 07:15:00";
        lblSavedTimeIn.Font = new Font("Segoe UI", 8.5F);
        lblSavedTimeIn.ForeColor = Color.FromArgb(71, 85, 105);
        lblSavedTimeIn.Dock = DockStyle.Bottom;
        lblSavedTimeIn.Height = 28;
        lblSavedTimeIn.TextAlign = ContentAlignment.MiddleCenter;

        pnlSavedPhotoInner.Dock = DockStyle.Fill;
        pnlSavedPhotoInner.BackColor = Color.White;
        pnlSavedPhotoInner.Padding = new Padding(6);

        lblSavedFileName.Text = "oto_51H99999.jpg";
        lblSavedFileName.Font = new Font("Consolas", 9F);
        lblSavedFileName.ForeColor = Color.FromArgb(100, 116, 139);
        lblSavedFileName.Dock = DockStyle.Top;
        lblSavedFileName.Height = 40;
        lblSavedFileName.TextAlign = ContentAlignment.BottomCenter;

        lblSavedPlate.Text = "51H-999.99";
        lblSavedPlate.Font = new Font("Consolas", 10F, FontStyle.Bold);
        lblSavedPlate.ForeColor = Color.FromArgb(15, 23, 42);
        lblSavedPlate.Dock = DockStyle.Fill;
        lblSavedPlate.TextAlign = ContentAlignment.TopCenter;

        pnlSavedPhotoInner.Controls.Add(lblSavedPlate);
        pnlSavedPhotoInner.Controls.Add(lblSavedFileName);

        pnlSavedPhotoCard.Controls.Add(pnlSavedPhotoInner);
        pnlSavedPhotoCard.Controls.Add(lblSavedPhotoTitle);
        pnlSavedPhotoCard.Controls.Add(lblSavedTimeIn);

        // Cột nhỏ bên phải: Thông tin chi tiết
        pnlSessionDetailsCard.Dock = DockStyle.Fill;
        pnlSessionDetailsCard.BackColor = Color.White;
        pnlSessionDetailsCard.Margin = new Padding(6, 0, 0, 0);
        pnlSessionDetailsCard.Padding = new Padding(10, 8, 10, 8);

        tlpDetailsRows.Dock = DockStyle.Fill;
        tlpDetailsRows.ColumnCount = 2;
        tlpDetailsRows.RowCount = 5;
        tlpDetailsRows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44F));
        tlpDetailsRows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56F));
        for (int i = 0; i < 5; i++)
        {
            tlpDetailsRows.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
        }

        AddDetailRowToTable(tlpDetailsRows, 0, "Mã thẻ:", out lblOutCardVal, "THE003", false);
        AddDetailRowToTable(tlpDetailsRows, 1, "Biển số vào:", out lblOutPlateVal, "51H-999.99", false);
        AddDetailRowToTable(tlpDetailsRows, 2, "Loại / Ô đỗ:", out lblOutTypeSlotVal, "Ô tô • Ô B-01", false);
        AddDetailRowToTable(tlpDetailsRows, 3, "Thời gian đỗ:", out lblOutDurationVal, "1h 40p", false);
        AddDetailRowToTable(tlpDetailsRows, 4, "Loại vé:", out lblOutTicketTypeVal, "★ Vé tháng (0đ)", true);

        pnlSessionDetailsCard.Controls.Add(tlpDetailsRows);

        tlpCompareOut.Controls.Add(pnlSavedPhotoCard, 0, 0);
        tlpCompareOut.Controls.Add(pnlSessionDetailsCard, 1, 0);

        // Khung Checkbox Khách mất thẻ
        pnlLostCardBox.Location = new Point(18, 282);
        pnlLostCardBox.Size = new Size(444, 44);
        pnlLostCardBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pnlLostCardBox.BackColor = Color.FromArgb(248, 250, 252);
        pnlLostCardBox.Padding = new Padding(12, 4, 12, 4);

        chkLostCard.Text = "Khách mất thẻ (+50.000đ tiền phạt & nhập CCCD biên bản)";
        chkLostCard.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        chkLostCard.Dock = DockStyle.Fill;
        chkLostCard.CheckedChanged += ChkLostCard_CheckedChanged;
        pnlLostCardBox.Controls.Add(chkLostCard);

        // Khung Tổng tiền thu
        pnlTotalBilling.Location = new Point(18, 340);
        pnlTotalBilling.Size = new Size(444, 76);
        pnlTotalBilling.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pnlTotalBilling.BackColor = Color.FromArgb(236, 253, 245);
        pnlTotalBilling.Padding = new Padding(14, 10, 14, 10);

        lblTotalAmount.Text = "0 VNĐ";
        lblTotalAmount.Font = new Font("Consolas", 16F, FontStyle.Bold);
        lblTotalAmount.ForeColor = Color.FromArgb(5, 150, 105);
        lblTotalAmount.Dock = DockStyle.Right;
        lblTotalAmount.Width = 185;
        lblTotalAmount.TextAlign = ContentAlignment.MiddleRight;

        lblTotalTitle.Text = "TỔNG TIỀN THU";
        lblTotalTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblTotalTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblTotalTitle.Location = new Point(14, 14);
        lblTotalTitle.AutoSize = true;

        lblFeeBreakdown.Text = "Tiền gửi: 0đ + Phạt: 0đ";
        lblFeeBreakdown.Font = new Font("Segoe UI", 8.8F);
        lblFeeBreakdown.ForeColor = Color.FromArgb(71, 85, 105);
        lblFeeBreakdown.Location = new Point(14, 40);
        lblFeeBreakdown.AutoSize = true;

        pnlTotalBilling.Controls.Add(lblTotalAmount);
        pnlTotalBilling.Controls.Add(lblTotalTitle);
        pnlTotalBilling.Controls.Add(lblFeeBreakdown);

        // Nút Xác nhận thu tiền
        btnSubmitCheckOut.Text = "[F2] Xác Nhận Thu Tiền & Giải Phóng Chỗ Đỗ";
        btnSubmitCheckOut.Location = new Point(18, 430);
        btnSubmitCheckOut.Size = new Size(444, 44);
        btnSubmitCheckOut.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        btnSubmitCheckOut.FlatStyle = FlatStyle.Flat;
        btnSubmitCheckOut.FlatAppearance.BorderSize = 0;
        btnSubmitCheckOut.BackColor = Color.FromArgb(5, 150, 105);
        btnSubmitCheckOut.ForeColor = Color.White;
        btnSubmitCheckOut.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnSubmitCheckOut.Cursor = Cursors.Hand;
        btnSubmitCheckOut.Click += BtnSubmitCheckOut_Click;

        pnlCheckOut.Controls.Add(lblCheckOutHeader);
        pnlCheckOut.Controls.Add(pnlSearchSessionBox);
        pnlCheckOut.Controls.Add(tlpCompareOut);
        pnlCheckOut.Controls.Add(pnlLostCardBox);
        pnlCheckOut.Controls.Add(pnlTotalBilling);
        pnlCheckOut.Controls.Add(btnSubmitCheckOut);

        tlpMainColumns.Controls.Add(pnlCheckIn, 0, 0);
        tlpMainColumns.Controls.Add(pnlCheckOut, 1, 0);

        // 
        // CheckInOutForm
        // 
        AutoScaleMode = AutoScaleMode.Font;
        FormBorderStyle = FormBorderStyle.None;
        KeyPreview = true;
        Controls.Add(tlpMainColumns);
        Controls.Add(pnlHeader);

        ResumeLayout(false);
    }

    private static void AddDetailRowToTable(TableLayoutPanel table, int row, string labelText, out Label valueLabel, string defaultVal, bool isAccent)
    {
        var lblTitle = new Label
        {
            Text = labelText,
            Font = new Font("Segoe UI", 8.8F),
            ForeColor = Color.FromArgb(100, 116, 139),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        valueLabel = new Label
        {
            Text = defaultVal,
            Font = new Font(isAccent ? "Segoe UI" : "Consolas", 8.8F, FontStyle.Bold),
            ForeColor = isAccent ? Color.FromArgb(37, 99, 235) : Color.FromArgb(15, 23, 42),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight
        };

        table.Controls.Add(lblTitle, 0, row);
        table.Controls.Add(valueLabel, 1, row);
    }
}

using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using BAIDOXETHONGMINH.DTO.Entities;
using BAIDOXETHONGMINH.DTO.Models;
using BAIDOXETHONGMINH.Services;

namespace BAIDOXETHONGMINH.GUI;

/// <summary>
/// Component UI Bàn điều khiển Xe Vào / Xe Ra (Tất cả khung hình chữ nhật đều được bo tròn góc)
/// </summary>
public partial class CheckInOutForm : Form
{
    private readonly ParkingService _parkingService = new ParkingService();
    private bool _isLoadingCheckInData = false;
    private bool _isLoadingCheckOutData = false;
    private CheckOutPreviewModel? _currentCheckOutPreview = null;

    public event Action<int, int>? SlotCapacityChanged;

    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    private static extern IntPtr CreateRoundRectRgn(
        int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    [DllImport("Gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    public CheckInOutForm()
    {
        InitializeComponent();
        SetupAllRoundedRectangles();
        Load += CheckInOutForm_Load;
    }

    private void CheckInOutForm_Load(object? sender, EventArgs e)
    {
        LoadCheckInCatalogData();
        LoadActiveSessionsForCheckOut();
    }

    private void LoadCheckInCatalogData()
    {
        string? previousVehicleTypeId = cboVehicleTypeIn.SelectedValue?.ToString();

        _isLoadingCheckInData = true;
        try
        {
            List<VehicleType> vehicleTypes = _parkingService.GetVehicleTypes();
            cboVehicleTypeIn.DisplayMember = nameof(VehicleType.TypeName);
            cboVehicleTypeIn.ValueMember = nameof(VehicleType.VehicleTypeId);
            cboVehicleTypeIn.DataSource = vehicleTypes;

            if (!string.IsNullOrEmpty(previousVehicleTypeId))
            {
                cboVehicleTypeIn.SelectedValue = previousVehicleTypeId;
            }

            if (cboVehicleTypeIn.SelectedIndex < 0 && vehicleTypes.Count > 0)
            {
                cboVehicleTypeIn.SelectedIndex = 0;
            }

            List<ParkingCard> availableCards = _parkingService.GetAvailableCards();
            cboCardIn.DisplayMember = nameof(ParkingCard.CardCode);
            cboCardIn.ValueMember = nameof(ParkingCard.CardCode);
            cboCardIn.DataSource = availableCards;
        }
        finally
        {
            _isLoadingCheckInData = false;
        }

        LoadAvailableSlotsForSelectedVehicleType();
        NotifySlotCapacityChanged();
    }

    private void LoadAvailableSlotsForSelectedVehicleType()
    {
        VehicleType? selectedType = cboVehicleTypeIn.SelectedItem as VehicleType;
        string vehicleTypeId = selectedType?.VehicleTypeId ?? "XEMAY";

        List<ParkingSlot> availableSlots = _parkingService.GetAvailableSlots(vehicleTypeId);
        cboSlotIn.DisplayMember = nameof(ParkingSlot.SlotCode);
        cboSlotIn.ValueMember = nameof(ParkingSlot.SlotCode);
        cboSlotIn.DataSource = availableSlots;
    }

    private void NotifySlotCapacityChanged()
    {
        (int AvailableSlots, int TotalSlots) summary = _parkingService.GetSlotCapacitySummary();
        SlotCapacityChanged?.Invoke(summary.AvailableSlots, summary.TotalSlots);
    }

    /// <summary>
    /// Bo tròn tất cả các khung hình chữ nhật / hình vuông (Card, Ô nhập TextBox, Ô chọn ComboBox, Khung ảnh, Nút bấm)
    /// </summary>
    private void SetupAllRoundedRectangles()
    {
        Color slateBorder = Color.FromArgb(148, 163, 184);
        Color cardBorder = Color.FromArgb(203, 213, 225);

        // 1. Hai khung lớn Làn Vào & Làn Ra
        AttachRoundedFrame(pnlCheckIn, 18, cardBorder);
        AttachRoundedFrame(pnlCheckOut, 18, cardBorder);

        // 2. Các ô nhập liệu & chọn danh sách (TextBox / ComboBox bọc trong Panel bo tròn)
        AttachRoundedFrame(pnlImagePathBox, 10, slateBorder);
        AttachRoundedFrame(pnlPlateInBox, 10, slateBorder);
        AttachRoundedFrame(pnlVehicleTypeInBox, 10, slateBorder);
        AttachRoundedFrame(pnlCardInBox, 10, slateBorder);
        AttachRoundedFrame(pnlSlotInBox, 10, slateBorder);
        AttachRoundedFrame(pnlSearchSessionBox, 10, slateBorder);

        // 3. Các khung thông tin, đối chiếu ảnh, mất thẻ, tổng tiền
        AttachRoundedFrame(lblShortcutBadge, 10, slateBorder);
        AttachRoundedFrame(pnlPassStatusBanner, 12, Color.FromArgb(147, 197, 253));
        AttachRoundedFrame(pnlSavedPhotoCard, 14, cardBorder);
        AttachRoundedFrame(pnlSavedPhotoInner, 10, cardBorder);
        AttachRoundedFrame(pnlSessionDetailsCard, 14, cardBorder);
        AttachRoundedFrame(pnlLostCardBox, 12, cardBorder);
        AttachRoundedFrame(pnlTotalBilling, 14, Color.FromArgb(110, 231, 183));

        // 4. Các nút bấm
        AttachRoundedFrame(btnBrowseImage, 10, slateBorder);
        AttachRoundedFrame(btnSubmitCheckIn, 12, Color.FromArgb(37, 99, 235));
        AttachRoundedFrame(btnResetCheckIn, 12, slateBorder);
        AttachRoundedFrame(btnSubmitCheckOut, 12, Color.FromArgb(5, 150, 105));
    }

    /// <summary>
    /// Cắt vùng hiển thị bo tròn (Region) + Vẽ đường viền bo tròn khử răng cưa (AntiAlias)
    /// </summary>
    private static void AttachRoundedFrame(Control control, int radius, Color borderColor)
    {
        void UpdateRegion()
        {
            if (control.Width <= 4 || control.Height <= 4) return;
            IntPtr hrgn = CreateRoundRectRgn(0, 0, control.Width + 1, control.Height + 1, radius, radius);
            control.Region = Region.FromHrgn(hrgn);
            DeleteObject(hrgn);
            control.Invalidate();
        }

        control.HandleCreated += (s, e) => UpdateRegion();
        control.Resize += (s, e) => UpdateRegion();

        control.Paint += (s, e) =>
        {
            if (control.Width <= 4 || control.Height <= 4) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(1, 1, control.Width - 3, control.Height - 3);
            using GraphicsPath path = CreateRoundedRectanglePath(rect, radius / 2);
            using Pen borderPen = new Pen(borderColor, 1.4f);
            g.DrawPath(borderPen, path);
        };
    }

    private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
    {
        GraphicsPath path = new GraphicsPath();
        int d = Math.Max(2, radius * 2);
        if (d > rect.Width) d = rect.Width;
        if (d > rect.Height) d = rect.Height;

        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    /// <summary>
    /// Vẽ khung viền nét đứt bo tròn và biểu tượng hình ảnh bo tròn khi chưa chọn file ảnh
    /// </summary>
    private void PicVehicleIn_Paint(object? sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        Rectangle rect = new Rectangle(1, 1, picVehicleIn.Width - 3, picVehicleIn.Height - 3);
        using GraphicsPath path = CreateRoundedRectanglePath(rect, 12);

        Color parentBg = picVehicleIn.Parent?.BackColor ?? Color.White;
        using Region outsideRegion = new Region(new Rectangle(0, 0, picVehicleIn.Width, picVehicleIn.Height));
        outsideRegion.Exclude(path);
        using SolidBrush maskBrush = new SolidBrush(parentBg);
        g.FillRegion(maskBrush, outsideRegion);

        using Pen dashPen = new Pen(Color.FromArgb(148, 163, 184), 1.5f)
        {
            DashStyle = DashStyle.Dash
        };
        g.DrawPath(dashPen, path);

        if (picVehicleIn.Image != null) return;

        // Vẽ icon bức ảnh (khung bo tròn) ở giữa
        int cx = picVehicleIn.Width / 2;
        int cy = picVehicleIn.Height / 2 - 18;
        using Pen iconPen = new Pen(Color.FromArgb(71, 85, 105), 2f);
        using GraphicsPath iconPath = CreateRoundedRectanglePath(new Rectangle(cx - 22, cy - 16, 44, 32), 5);
        g.DrawPath(iconPen, iconPath);
        g.FillEllipse(Brushes.DimGray, cx - 12, cy - 8, 6, 6);
        g.DrawLines(iconPen, new[]
        {
            new Point(cx - 18, cy + 12),
            new Point(cx - 4, cy - 2),
            new Point(cx + 6, cy + 8),
            new Point(cx + 12, cy + 2),
            new Point(cx + 18, cy + 12)
        });

        using Font titleFont = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        using Font subFont = new Font("Segoe UI", 8.5F, FontStyle.Regular);
        StringFormat sf = new StringFormat { Alignment = StringAlignment.Center };

        g.DrawString("Khung hiển thị hình xe tải lên", titleFont, Brushes.Black, new RectangleF(0, cy + 22, picVehicleIn.Width, 22), sf);
        g.DrawString("Bấm \"Chọn hình từ máy...\" để mở hộp thoại chọn file ảnh", subFont, Brushes.DimGray, new RectangleF(0, cy + 42, picVehicleIn.Width, 22), sf);
    }

    private void BtnBrowseImage_Click(object? sender, EventArgs e)
    {
        using OpenFileDialog dialog = new OpenFileDialog
        {
            Title = "Chọn hình ảnh phương tiện lúc vào bãi",
            Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp"
        };

        if (dialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        try
        {
            byte[] imageBytes = File.ReadAllBytes(dialog.FileName);
            using MemoryStream stream = new MemoryStream(imageBytes);
            using Image tempImage = Image.FromStream(stream);

            txtImagePathIn.Text = dialog.FileName;
            picVehicleIn.Image?.Dispose();
            picVehicleIn.Image = new Bitmap(tempImage);
            picVehicleIn.Invalidate();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể đọc tệp hình ảnh: {ex.Message}", "Lỗi đọc ảnh", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void TxtPlateIn_TextChanged(object? sender, EventArgs e)
    {
        string rawPlate = txtPlateIn.Text.Trim();
        if (string.IsNullOrEmpty(rawPlate))
        {
            lblPassStatusIn.Text = "Đối chiếu Vé tháng: Chưa nhập biển số";
            lblPassStatusIn.ForeColor = Color.FromArgb(30, 64, 175);
            return;
        }

        MonthlyPass? activePass = _parkingService.GetActiveMonthlyPass(rawPlate);
        if (activePass != null)
        {
            lblPassStatusIn.Text = $"★ Khách Vé Tháng (Miễn phí lượt) - HSD: {activePass.EndDate:dd/MM/yyyy}";
            lblPassStatusIn.ForeColor = Color.FromArgb(5, 150, 105);

            string? currentTypeId = cboVehicleTypeIn.SelectedValue?.ToString();
            if (!string.Equals(currentTypeId, activePass.VehicleTypeId, StringComparison.OrdinalIgnoreCase))
            {
                cboVehicleTypeIn.SelectedValue = activePass.VehicleTypeId;
            }
            return;
        }

        lblPassStatusIn.Text = "Đối chiếu Vé tháng: Khách gửi Vé lượt";
        lblPassStatusIn.ForeColor = Color.FromArgb(30, 64, 175);
    }

    private void CboVehicleTypeIn_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_isLoadingCheckInData)
        {
            return;
        }

        LoadAvailableSlotsForSelectedVehicleType();
    }

    private void BtnResetCheckIn_Click(object? sender, EventArgs e)
    {
        txtPlateIn.Clear();
        txtImagePathIn.Clear();
        picVehicleIn.Image?.Dispose();
        picVehicleIn.Image = null;
        picVehicleIn.Invalidate();
    }

    private void BtnSubmitCheckIn_Click(object? sender, EventArgs e)
    {
        string licensePlate = txtPlateIn.Text.Trim();
        if (string.IsNullOrWhiteSpace(licensePlate))
        {
            MessageBox.Show("Vui lòng nhập Biển số xe thủ công trước khi lưu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtPlateIn.Focus();
            return;
        }

        VehicleType? selectedVehicleType = cboVehicleTypeIn.SelectedItem as VehicleType;
        if (selectedVehicleType == null)
        {
            MessageBox.Show("Chưa có Loại phương tiện hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        ParkingCard? selectedCard = cboCardIn.SelectedItem as ParkingCard;
        if (selectedCard == null)
        {
            MessageBox.Show("Hiện không còn Thẻ từ nào trống để cấp!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        ParkingSlot? selectedSlot = cboSlotIn.SelectedItem as ParkingSlot;
        if (selectedSlot == null)
        {
            MessageBox.Show("Khu vực của loại xe này hiện đã hết Ô đỗ trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string? imagePath = string.IsNullOrWhiteSpace(txtImagePathIn.Text) ? null : txtImagePathIn.Text.Trim();
        (bool IsSuccess, string Message) result = _parkingService.CheckInVehicle(
            licensePlate,
            selectedVehicleType.VehicleTypeId,
            selectedCard.CardCode,
            selectedSlot.SlotCode,
            imagePath
        );

        MessageBox.Show(
            result.Message,
            result.IsSuccess ? "Cho xe vào bãi" : "Không thể cho xe vào bãi",
            MessageBoxButtons.OK,
            result.IsSuccess ? MessageBoxIcon.Information : MessageBoxIcon.Warning
        );

        if (result.IsSuccess)
        {
            BtnResetCheckIn_Click(sender, e);
            LoadCheckInCatalogData();
            LoadActiveSessionsForCheckOut();
            txtPlateIn.Focus();
        }
    }

    private void LoadActiveSessionsForCheckOut(string? preferredCardCode = null)
    {
        _isLoadingCheckOutData = true;
        try
        {
            List<CheckOutPreviewModel> activePreviews = _parkingService.GetActiveCheckOutPreviews(false);
            cboSearchSession.DataSource = null;
            cboSearchSession.DisplayMember = nameof(CheckOutPreviewModel.DisplaySearchText);
            cboSearchSession.ValueMember = nameof(CheckOutPreviewModel.CardCode);
            cboSearchSession.DataSource = activePreviews;

            if (!string.IsNullOrEmpty(preferredCardCode))
            {
                cboSearchSession.SelectedValue = preferredCardCode;
            }

            if (cboSearchSession.SelectedIndex < 0 && activePreviews.Count > 0)
            {
                cboSearchSession.SelectedIndex = 0;
            }

            chkLostCard.Checked = false;
            txtLostCardCccd.Clear();
            txtLostCardCccd.Enabled = false;
            txtLostCardCccd.BackColor = Color.FromArgb(241, 245, 249);
        }
        finally
        {
            _isLoadingCheckOutData = false;
        }

        RefreshSelectedCheckOutSession();
    }

    private void RefreshSelectedCheckOutSession()
    {
        CheckOutPreviewModel? selectedItem = cboSearchSession.SelectedItem as CheckOutPreviewModel;
        if (selectedItem == null)
        {
            ClearCheckOutDisplay();
            return;
        }

        CheckOutPreviewModel? latestPreview = _parkingService.PreviewCheckOut(
            selectedItem.CardCode,
            chkLostCard.Checked
        );

        DisplayCheckOutPreview(latestPreview ?? selectedItem);
    }

    private void DisplayCheckOutPreview(CheckOutPreviewModel preview)
    {
        _currentCheckOutPreview = preview;

        lblSavedTimeIn.Text = $"Vào: {preview.CheckInTime:dd/MM/yyyy HH:mm:ss}";
        lblSavedPlate.Text = preview.LicensePlate;
        lblSavedFileName.Text = string.IsNullOrWhiteSpace(preview.CheckInImagePath)
            ? "(Không có file ảnh)"
            : Path.GetFileName(preview.CheckInImagePath);

        LoadCheckOutVehicleImage(preview.CheckInImagePath);

        lblOutCardVal.Text = preview.CardCode;
        lblOutPlateVal.Text = preview.LicensePlate;
        lblOutTypeSlotVal.Text = $"{preview.VehicleTypeName} • Ô {preview.SlotCode}";
        lblOutDurationVal.Text = preview.FeeResult.DurationText;
        lblOutTicketTypeVal.Text = preview.FeeResult.TicketTypeDisplay;
        lblOutTicketTypeVal.ForeColor = preview.FeeResult.IsMonthlyPass
            ? Color.FromArgb(5, 150, 105)
            : Color.FromArgb(37, 99, 235);

        lblFeeBreakdown.Text = $"Tiền gửi: {preview.FeeResult.ParkingFee:N0}đ + Phạt: {preview.FeeResult.PenaltyFee:N0}đ";
        lblTotalAmount.Text = $"{preview.FeeResult.TotalAmount:N0} VNĐ";
    }

    private void LoadCheckOutVehicleImage(string? imagePath)
    {
        picVehicleOut.Image?.Dispose();
        picVehicleOut.Image = null;

        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
        {
            picVehicleOut.Visible = false;
            return;
        }

        try
        {
            byte[] imageBytes = File.ReadAllBytes(imagePath);
            using MemoryStream stream = new MemoryStream(imageBytes);
            using Image tempImage = Image.FromStream(stream);

            picVehicleOut.Image = new Bitmap(tempImage);
            picVehicleOut.Visible = true;
            picVehicleOut.BringToFront();
        }
        catch (Exception)
        {
            picVehicleOut.Visible = false;
        }
    }

    private void ClearCheckOutDisplay()
    {
        _currentCheckOutPreview = null;

        picVehicleOut.Image?.Dispose();
        picVehicleOut.Image = null;
        picVehicleOut.Visible = false;

        lblSavedTimeIn.Text = "Vào: —";
        lblSavedFileName.Text = "Không có xe đang đỗ";
        lblSavedPlate.Text = "—";

        lblOutCardVal.Text = "—";
        lblOutPlateVal.Text = "—";
        lblOutTypeSlotVal.Text = "—";
        lblOutDurationVal.Text = "—";
        lblOutTicketTypeVal.Text = "—";
        lblOutTicketTypeVal.ForeColor = Color.FromArgb(37, 99, 235);

        lblFeeBreakdown.Text = "Tiền gửi: 0đ + Phạt: 0đ";
        lblTotalAmount.Text = "0 VNĐ";
    }

    private void CboSearchSession_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_isLoadingCheckOutData)
        {
            return;
        }

        _isLoadingCheckOutData = true;
        try
        {
            chkLostCard.Checked = false;
            txtLostCardCccd.Clear();
            txtLostCardCccd.Enabled = false;
            txtLostCardCccd.BackColor = Color.FromArgb(241, 245, 249);
        }
        finally
        {
            _isLoadingCheckOutData = false;
        }

        RefreshSelectedCheckOutSession();
    }

    private void ChkLostCard_CheckedChanged(object? sender, EventArgs e)
    {
        bool isLostCard = chkLostCard.Checked;
        txtLostCardCccd.Enabled = isLostCard;
        txtLostCardCccd.BackColor = isLostCard ? Color.White : Color.FromArgb(241, 245, 249);

        if (!isLostCard)
        {
            txtLostCardCccd.Clear();
        }
        else if (!_isLoadingCheckOutData)
        {
            txtLostCardCccd.Focus();
        }

        if (_isLoadingCheckOutData)
        {
            return;
        }

        RefreshSelectedCheckOutSession();
    }

    private void BtnSubmitCheckOut_Click(object? sender, EventArgs e)
    {
        if (_currentCheckOutPreview == null)
        {
            MessageBox.Show(
                "Hiện không có xe nào đang chọn ở Làn Ra để làm thủ tục trả xe!",
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return;
        }

        bool isLostCard = chkLostCard.Checked;
        string cccdText = txtLostCardCccd.Text.Trim();

        if (isLostCard && string.IsNullOrWhiteSpace(cccdText))
        {
            MessageBox.Show(
                "Khách làm mất thẻ bắt buộc phải nhập Số CCCD / Ghi chú biên bản trước khi xác nhận thu tiền!",
                "Thiếu thông tin biên bản",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            txtLostCardCccd.Focus();
            return;
        }

        (bool IsSuccess, string Message) result = _parkingService.ConfirmCheckOut(
            _currentCheckOutPreview.CardCode,
            isLostCard,
            isLostCard ? cccdText : null
        );

        MessageBox.Show(
            result.Message,
            result.IsSuccess ? "Cho xe ra bãi" : "Không thể cho xe ra bãi",
            MessageBoxButtons.OK,
            result.IsSuccess ? MessageBoxIcon.Information : MessageBoxIcon.Warning
        );

        if (result.IsSuccess)
        {
            LoadCheckInCatalogData();
            LoadActiveSessionsForCheckOut();
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F1)
        {
            BtnSubmitCheckIn_Click(this, EventArgs.Empty);
            return true;
        }

        if (keyData == Keys.F2)
        {
            BtnSubmitCheckOut_Click(this, EventArgs.Empty);
            return true;
        }

        if (keyData == Keys.Escape)
        {
            BtnResetCheckIn_Click(this, EventArgs.Empty);
            chkLostCard.Checked = false;
            txtLostCardCccd.Clear();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }
}

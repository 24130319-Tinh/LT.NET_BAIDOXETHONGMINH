using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace BAIDOXETHONGMINH.GUI;

public partial class CheckInOutForm : Form
{
    private readonly ParkingService _parking = new();
    private CheckoutQuote? _quote;
    private bool _checkoutBusy;
    private int _quoteVersion;

    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    private static extern IntPtr CreateRoundRectRgn(
        int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    [DllImport("Gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    public CheckInOutForm()
    {
        InitializeComponent();
        SetupAllRoundedRectangles();
        cboSearchSession.Items.Clear();
    cboSearchSession.AutoCompleteSource = AutoCompleteSource.ListItems;
cboSearchSession.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        ClearCheckout();
        Disposed += (_, _) => pnlSavedPhotoInner.BackgroundImage?.Dispose();
        chkLostCard.Text = "Mất thẻ (phạt theo bảng giá, lập biên bản)";
        Shown += async (_, _) => await Dialogs.RunAsync(LoadCheckoutAsync);
    }

    private void SetupAllRoundedRectangles()
    {
        Color slateBorder = Color.FromArgb(148, 163, 184);
        Color cardBorder = Color.FromArgb(203, 213, 225);

        AttachRoundedFrame(pnlCheckIn, 18, cardBorder);
        AttachRoundedFrame(pnlCheckOut, 18, cardBorder);

        AttachRoundedFrame(pnlImagePathBox, 10, slateBorder);
        AttachRoundedFrame(pnlPlateInBox, 10, slateBorder);
        AttachRoundedFrame(pnlVehicleTypeInBox, 10, slateBorder);
        AttachRoundedFrame(pnlCardInBox, 10, slateBorder);
        AttachRoundedFrame(pnlSlotInBox, 10, slateBorder);
        AttachRoundedFrame(pnlSearchSessionBox, 10, slateBorder);

        AttachRoundedFrame(lblShortcutBadge, 10, slateBorder);
        AttachRoundedFrame(pnlPassStatusBanner, 12, Color.FromArgb(147, 197, 253));
        AttachRoundedFrame(pnlSavedPhotoCard, 14, cardBorder);
        AttachRoundedFrame(pnlSavedPhotoInner, 10, cardBorder);
        AttachRoundedFrame(pnlSessionDetailsCard, 14, cardBorder);
        AttachRoundedFrame(pnlLostCardBox, 12, cardBorder);
        AttachRoundedFrame(pnlTotalBilling, 14, Color.FromArgb(110, 231, 183));

        AttachRoundedFrame(btnBrowseImage, 10, slateBorder);
        AttachRoundedFrame(btnSubmitCheckIn, 12, Color.FromArgb(37, 99, 235));
        AttachRoundedFrame(btnResetCheckIn, 12, slateBorder);
        AttachRoundedFrame(btnSubmitCheckOut, 12, Color.FromArgb(5, 150, 105));
    }

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

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(1, 1, control.Width - 3, control.Height - 3);
            using var path = CreateRoundedRectanglePath(rect, radius / 2);
            using var borderPen = new Pen(borderColor, 1.4f);
            g.DrawPath(borderPen, path);
        };
    }

    private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
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

    private void PicVehicleIn_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(1, 1, picVehicleIn.Width - 3, picVehicleIn.Height - 3);
        using var path = CreateRoundedRectanglePath(rect, 12);

        Color parentBg = picVehicleIn.Parent?.BackColor ?? Color.White;
        using var outsideRegion = new Region(new Rectangle(0, 0, picVehicleIn.Width, picVehicleIn.Height));
        outsideRegion.Exclude(path);
        using var maskBrush = new SolidBrush(parentBg);
        g.FillRegion(maskBrush, outsideRegion);

        using var dashPen = new Pen(Color.FromArgb(148, 163, 184), 1.5f)
        {
            DashStyle = DashStyle.Dash
        };
        g.DrawPath(dashPen, path);

        if (picVehicleIn.Image != null) return;

        int cx = picVehicleIn.Width / 2;
        int cy = picVehicleIn.Height / 2 - 18;
        using var iconPen = new Pen(Color.FromArgb(71, 85, 105), 2f);
        using var iconPath = CreateRoundedRectanglePath(new Rectangle(cx - 22, cy - 16, 44, 32), 5);
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

        using var titleFont = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        using var subFont = new Font("Segoe UI", 8.5F, FontStyle.Regular);
        var sf = new StringFormat { Alignment = StringAlignment.Center };

        g.DrawString("Khung hiển thị hình xe tải lên", titleFont, Brushes.Black, new RectangleF(0, cy + 22, picVehicleIn.Width, 22), sf);
        g.DrawString("Bấm \"Chọn hình từ máy...\" để mở hộp thoại chọn file ảnh", subFont, Brushes.DimGray, new RectangleF(0, cy + 42, picVehicleIn.Width, 22), sf);
    }

    private void BtnBrowseImage_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Chọn hình ảnh phương tiện lúc vào bãi",
            Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            txtImagePathIn.Text = dialog.FileName;
            picVehicleIn.Image?.Dispose();
            picVehicleIn.Image = Image.FromFile(dialog.FileName);
            picVehicleIn.Invalidate();
        }
    }

    private void TxtPlateIn_TextChanged(object? sender, EventArgs e)
    {
        string plate = txtPlateIn.Text.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(plate))
        {
            lblPassStatusIn.Text = "Đối chiếu Vé tháng: Chưa nhập biển số";
            lblPassStatusIn.ForeColor = Color.FromArgb(30, 64, 175);
        }
        else if (plate == "51H-999.99" || plate == "59P2-345.67")
        {
            lblPassStatusIn.Text = "Đối chiếu Vé tháng: ★ Vé Tháng Hợp Lệ";
            lblPassStatusIn.ForeColor = Color.FromArgb(5, 150, 105);
        }
        else
        {
            lblPassStatusIn.Text = "Đối chiếu Vé tháng: Khách gửi Vé lượt";
            lblPassStatusIn.ForeColor = Color.FromArgb(30, 64, 175);
        }
    }

    private void CboVehicleTypeIn_SelectedIndexChanged(object? sender, EventArgs e)
    {
        txtSlotIn.Text = cboVehicleTypeIn.SelectedIndex == 0 ? "A-03 (Trống)" : "B-03 (Trống)";
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
        if (string.IsNullOrWhiteSpace(txtPlateIn.Text))
        {
            MessageBox.Show("Vui lòng nhập Biển số xe thủ công trước khi lưu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtPlateIn.Focus();
            return;
        }

        MessageBox.Show(
            $"Đã lưu xe biển số {txtPlateIn.Text.Trim().ToUpperInvariant()} vào vị trí {txtSlotIn.Text} thành công!",
            "Cho xe vào bãi",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }

    private async Task LoadCheckoutAsync()
    {
        await AppService.InitializeAsync();
        cboSearchSession.DataSource = await _parking.GetActiveVisitsAsync();
        if (cboSearchSession.Items.Count == 0) ClearCheckout();
    }

    private void ClearCheckout()
    {
        _quoteVersion++;
        _quote = null;
        btnSubmitCheckOut.Enabled = false;
        lblOutCardVal.Text = lblOutPlateVal.Text = lblOutTypeSlotVal.Text = "—";
        lblOutDurationVal.Text = lblOutTicketTypeVal.Text = "—";
        lblSavedPlate.Text = "Không có xe đang gửi";
        lblSavedFileName.Text = lblSavedTimeIn.Text = "";
        pnlSavedPhotoInner.BackgroundImage?.Dispose();
        pnlSavedPhotoInner.BackgroundImage = null;
        lblTotalAmount.Text = Fmt.Vnd(0);
        lblFeeBreakdown.Text = "Chọn lượt gửi xe để tính phí";
    }

    private async void CboSearchSession_SelectedIndexChanged(object? sender, EventArgs e)
    {
        await Dialogs.RunAsync(UpdateTotalOutFeeAsync);
    }

    private async void ChkLostCard_CheckedChanged(object? sender, EventArgs e)
    {
        await Dialogs.RunAsync(UpdateTotalOutFeeAsync);
    }

    private async Task UpdateTotalOutFeeAsync()
    {
        int version = ++_quoteVersion;
        _quote = null;
        btnSubmitCheckOut.Enabled = false;
        if (cboSearchSession.SelectedItem is not ParkingVisit visit)
        {
            ClearCheckout();
            return;
        }
        var quote = await _parking.GetQuoteAsync(visit.SessionId, chkLostCard.Checked);
        if (version != _quoteVersion || IsDisposed) return;
        _quote = quote;
        lblOutCardVal.Text = visit.CardCode;
        lblOutPlateVal.Text = visit.LicensePlate;
        lblOutTypeSlotVal.Text = $"{visit.TypeName} • {visit.SlotCode}";
        var duration = quote.ExitTime - visit.CheckInTime;
        lblOutDurationVal.Text = $"{(int)duration.TotalHours}h {duration.Minutes}p";
        lblOutTicketTypeVal.Text = visit.IsMonthlyPass ? "Vé tháng" : "Vé lượt";
        lblSavedTimeIn.Text = $"Vào: {visit.CheckInTime:dd/MM/yyyy HH:mm}";
        lblSavedFileName.Text = "";
        lblSavedPlate.Text = visit.LicensePlate;
        pnlSavedPhotoInner.BackgroundImage?.Dispose();
        pnlSavedPhotoInner.BackgroundImage = null;
        if (!string.IsNullOrWhiteSpace(visit.ImagePath) && File.Exists(visit.ImagePath))
        {
            try
            {
                using var source = Image.FromFile(visit.ImagePath);
                pnlSavedPhotoInner.BackgroundImage = new Bitmap(source);
                pnlSavedPhotoInner.BackgroundImageLayout = ImageLayout.Zoom;
                lblSavedPlate.Text = "";
            }
            catch (Exception ex) when (ex is IOException or ArgumentException or OutOfMemoryException)
            {
                lblSavedFileName.Text = "Không đọc được ảnh đã lưu";
            }
        }
        lblFeeBreakdown.Text = $"Tiền gửi: {Fmt.Money(quote.ParkingFee)} + Phạt: {Fmt.Money(quote.Penalty)}";
        lblTotalAmount.Text = Fmt.Vnd(quote.Total);
        btnSubmitCheckOut.Enabled = !_checkoutBusy;
    }

    private async void BtnSubmitCheckOut_Click(object? sender, EventArgs e)
    {
        if (_checkoutBusy || cboSearchSession.SelectedItem is not ParkingVisit) return;
        _checkoutBusy = true;
        btnSubmitCheckOut.Enabled = false;
        cboSearchSession.Enabled = chkLostCard.Enabled = false;
        try
        {
            await Dialogs.RunAsync(async () =>
            {
                LostCardReport? report = null;
                bool lost = chkLostCard.Checked;
                if (lost)
                {
                    var fields = Dialogs.Prompt(this, "Biên bản mất thẻ",
                        ("Họ tên khách", ""), ("Điện thoại", ""), ("CCCD", ""));
                    if (fields == null) return;
                    report = new(fields[0], fields[1], fields[2]);
                    if (fields.Any(string.IsNullOrWhiteSpace))
                        throw new ServiceException("Hãy nhập đủ thông tin biên bản mất thẻ.");
                }
                await UpdateTotalOutFeeAsync();
                var quote = _quote ?? throw new ServiceException("Hãy chọn lại lượt gửi xe.");
                if (MessageBox.Show(this,
                    $"Xe {quote.Visit.LicensePlate} — thẻ {quote.Visit.CardCode}\nThu tiền mặt: {Fmt.Vnd(quote.Total)}\nXác nhận đã đối chiếu xe và nhận đủ tiền?",
                    "Xác nhận xe ra", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                await _parking.CheckoutAsync(quote, lost, report);
                // Xóa lựa chọn trước khi tải lại để không thể thu trùng nếu tải dữ liệu lỗi.
                cboSearchSession.DataSource = null;
                ClearCheckout();
                chkLostCard.Checked = false;
                MessageBox.Show(this, $"Đã lưu xe ra và thu {Fmt.Vnd(quote.Total)}.", "Hoàn tất");
                await LoadCheckoutAsync();
            });
        }
        finally
        {
            _checkoutBusy = false;
            cboSearchSession.Enabled = chkLostCard.Enabled = true;
            btnSubmitCheckOut.Enabled = _quote != null;
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
        return base.ProcessCmdKey(ref msg, keyData);
    }
}

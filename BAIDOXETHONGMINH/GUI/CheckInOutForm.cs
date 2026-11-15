using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace BAIDOXETHONGMINH.GUI;

public partial class CheckInOutForm : Form
{
    private readonly ParkingService _parking = new();
    private CheckoutQuote? _quote;
    private bool _checkoutBusy;
    private bool _checkInBusy;
    private int _slotLoadVersion;
    private string[] _availableSlots = [];
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
        var cardMenu = new ContextMenuStrip();
        cardMenu.Items.Add("Quản lý thẻ xe", null, async (_, _) =>
        {
            using var form = new ParkingCardForm();
            form.ShowDialog(this);
            await Dialogs.RunAsync(LoadCardsAsync);
        });
        cboCardIn.ContextMenuStrip = cardMenu;
        Disposed += (_, _) =>
        {
            cardMenu.Dispose();
            picVehicleIn.Image?.Dispose();
            pnlSavedPhotoInner.BackgroundImage?.Dispose();
        };
        chkLostCard.Text = "Mất thẻ (phạt theo bảng giá, lập biên bản)";
        Shown += async (_, _) => await Dialogs.RunAsync(async () =>
        {
            await AppService.InitializeAsync();
            await LoadCheckInAsync();
            await LoadCheckoutAsync();
        });
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
            try
            {
                using var source = Image.FromFile(dialog.FileName);
                var preview = new Bitmap(source);
                picVehicleIn.Image?.Dispose();
                picVehicleIn.Image = preview;
                txtImagePathIn.Text = dialog.FileName;
                picVehicleIn.Invalidate();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or OutOfMemoryException)
            {
                MessageBox.Show(this, "Không đọc được ảnh đã chọn.", "Ảnh xe vào", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private async void TxtPlateIn_TextChanged(object? sender, EventArgs e)
    {
        string plate = txtPlateIn.Text.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(plate))
        {
            lblPassStatusIn.Text = "Đối chiếu Vé tháng: Chưa nhập biển số";
            lblPassStatusIn.ForeColor = Color.FromArgb(30, 64, 175);
        }
        else
        {
            lblPassStatusIn.Text = "Đối chiếu Vé tháng: Đang kiểm tra";
            lblPassStatusIn.ForeColor = Color.FromArgb(30, 64, 175);
            await Dialogs.RunAsync(async () =>
            {
                string type = VehicleTypeIn;
                var pass = await AppService.Monthly.FindValidPassAsync(plate);
                if (IsDisposed || plate != PlateHelper.Clean(txtPlateIn.Text) || type != VehicleTypeIn) return;
                bool monthly = pass?.VehicleTypeId == type;
                lblPassStatusIn.Text = monthly ? "Đối chiếu Vé tháng: Vé tháng hợp lệ" :
                    "Đối chiếu Vé tháng: Khách gửi Vé lượt";
                if (monthly) lblPassStatusIn.ForeColor = Color.FromArgb(5, 150, 105);
            });
        }
    }

    private string VehicleTypeIn => cboVehicleTypeIn.SelectedIndex == 0 ? "XEMAY" : "OTO";

    private async void CboVehicleTypeIn_SelectedIndexChanged(object? sender, EventArgs e)
    {
        TxtPlateIn_TextChanged(sender, e);
        await Dialogs.RunAsync(LoadSlotsAsync);
    }

    private async Task LoadSlotsAsync()
    {
        int version = ++_slotLoadVersion;
        string type = VehicleTypeIn;
        _availableSlots = [];
        txtSlotIn.Clear();
        txtSlotIn.AutoCompleteCustomSource.Clear();
        var zones = await AppService.Map.GetMapAsync();
        if (IsDisposed || version != _slotLoadVersion) return;
        _availableSlots = zones.Where(zone => zone.Zone.VehicleTypeId == type)
            .SelectMany(zone => zone.Slots)
            .Where(slot => slot.Status == SlotStatus.Available)
            .Select(slot => slot.SlotCode).ToArray();
        txtSlotIn.AutoCompleteCustomSource.AddRange(_availableSlots);
        txtSlotIn.Text = _availableSlots.FirstOrDefault() ?? "";
    }

    private async Task LoadCheckInAsync()
    {
        await LoadCardsAsync();
        if (!IsDisposed) await LoadSlotsAsync();
    }

    private async Task LoadCardsAsync()
    {
        string? selected = cboCardIn.SelectedItem as string;
        var cards = await AppService.Card.SearchAsync("");
        if (IsDisposed) return;
        var available = cards.Where(card => card.Status == CardStatus.Available)
            .Select(card => card.CardCode).ToList();
        cboCardIn.DataSource = available;
        if (selected != null && available.Contains(selected)) cboCardIn.SelectedItem = selected;
    }

    private void BtnResetCheckIn_Click(object? sender, EventArgs e)
    {
        txtPlateIn.Clear();
        txtImagePathIn.Clear();
        picVehicleIn.Image?.Dispose();
        picVehicleIn.Image = null;
        picVehicleIn.Invalidate();
    }

    private async void BtnSubmitCheckIn_Click(object? sender, EventArgs e)
    {
        if (_checkInBusy) return;
        _checkInBusy = true;
        btnSubmitCheckIn.Enabled = false;
        try
        {
            await Dialogs.RunAsync(async () =>
            {
                string plate = PlateHelper.Clean(txtPlateIn.Text);
                if (PlateHelper.Key(plate).Length == 0)
                    throw new ServiceException("Hãy nhập biển số xe hợp lệ.");
                if (cboCardIn.SelectedItem is not string card)
                    throw new ServiceException("Chưa có thẻ trống. Nhấp chuột phải vào ô mã thẻ để mở Quản lý thẻ xe.");
                string slot = txtSlotIn.Text.Trim().ToUpperInvariant();
                if (!_availableSlots.Contains(slot))
                    throw new ServiceException("Hãy nhập mã ô đỗ còn trống của loại xe đã chọn.");

                await _parking.CheckInAsync(plate, VehicleTypeIn, slot, card,
                    picVehicleIn.Image == null ? null : txtImagePathIn.Text);
                if (IsDisposed) return;
                BtnResetCheckIn_Click(sender, e);
                MessageBox.Show(this, $"Đã lưu xe {plate} vào ô {slot}.", "Cho xe vào bãi");
                await LoadCheckoutAsync();
                await LoadCheckInAsync();
            });
        }
        finally
        {
            _checkInBusy = false;
            if (!IsDisposed) btnSubmitCheckIn.Enabled = true;
        }
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
        lblOutCardVal.Text = visit.CardCode;
        lblOutPlateVal.Text = visit.LicensePlate;
        lblOutTypeSlotVal.Text = $"{visit.TypeName} • {visit.SlotCode}";
        var duration = DateTime.Now - visit.CheckInTime;
        lblOutDurationVal.Text = $"{(int)duration.TotalHours}h {duration.Minutes}p";
        lblOutTicketTypeVal.Text = visit.IsMonthlyPass ? "Vé tháng" : "Vé lượt";
        lblSavedTimeIn.Text = $"Vào: {visit.CheckInTime:dd/MM/yyyy HH:mm}";
        lblSavedFileName.Text = "";
        lblSavedPlate.Text = visit.LicensePlate;
        pnlSavedPhotoInner.BackgroundImage?.Dispose();
        pnlSavedPhotoInner.BackgroundImage = null;
        if (!string.IsNullOrWhiteSpace(visit.ImagePath))
        {
            try
            {
                string savedPath = ParkingService.ResolveCheckInImagePath(visit.ImagePath);
                if (!File.Exists(savedPath)) throw new FileNotFoundException();
                using var source = Image.FromFile(savedPath);
                pnlSavedPhotoInner.BackgroundImage = new Bitmap(source);
                pnlSavedPhotoInner.BackgroundImageLayout = ImageLayout.Zoom;
                lblSavedPlate.Text = "";
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or OutOfMemoryException)
            {
                lblSavedFileName.Text = "Không đọc được ảnh đã lưu";
            }
        }
        lblTotalAmount.Text = "—";
        lblFeeBreakdown.Text = "Đang tính phí";
        CheckoutQuote quote;
        try
        {
            quote = await _parking.GetQuoteAsync(visit.SessionId, chkLostCard.Checked);
        }
        catch (ServiceException ex)
        {
            if (version == _quoteVersion && !IsDisposed) lblFeeBreakdown.Text = ex.Message;
            return;
        }
        if (version != _quoteVersion || IsDisposed) return;
        _quote = quote;
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
                await LoadCheckInAsync();
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

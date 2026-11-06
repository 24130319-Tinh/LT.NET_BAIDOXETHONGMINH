using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace BAIDOXETHONGMINH.GUI;

/// <summary>
/// Component UI Bàn điều khiển Xe Vào / Xe Ra (Tất cả khung hình chữ nhật đều được bo tròn góc)
/// </summary>
public partial class CheckInOutForm : Form
{
    private decimal _currentBaseFee = 0m;

    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    private static extern IntPtr CreateRoundRectRgn(
        int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    [DllImport("Gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    public CheckInOutForm()
    {
        InitializeComponent();
        SetupAllRoundedRectangles();
        cboSearchSession.SelectedIndex = 0;
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

    /// <summary>
    /// Vẽ khung viền nét đứt bo tròn và biểu tượng hình ảnh bo tròn khi chưa chọn file ảnh
    /// </summary>
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

        // Vẽ icon bức ảnh (khung bo tròn) ở giữa
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

    private void CboSearchSession_SelectedIndexChanged(object? sender, EventArgs e)
    {
        chkLostCard.Checked = false;

        switch (cboSearchSession.SelectedIndex)
        {
            case 0:
                lblOutCardVal.Text = "THE003";
                lblOutPlateVal.Text = "51H-999.99";
                lblSavedPlate.Text = "51H-999.99";
                lblSavedFileName.Text = "oto_51H99999.jpg";
                lblOutTypeSlotVal.Text = "Ô tô • Ô B-01";
                lblOutDurationVal.Text = "1h 40p";
                lblOutTicketTypeVal.Text = "★ Vé tháng (0đ)";
                _currentBaseFee = 0m;
                break;

            case 1:
                lblOutCardVal.Text = "THE002";
                lblOutPlateVal.Text = "51G-456.78";
                lblSavedPlate.Text = "51G-456.78";
                lblSavedFileName.Text = "oto_51G45678.jpg";
                lblOutTypeSlotVal.Text = "Ô tô • Ô B-02";
                lblOutDurationVal.Text = "3h 05p (Tính 4h)";
                lblOutTicketTypeVal.Text = "Vé lượt (20k/h)";
                _currentBaseFee = 80000m;
                break;

            default:
                lblOutCardVal.Text = "THE001";
                lblOutPlateVal.Text = "59X3-112.23";
                lblSavedPlate.Text = "59X3-112.23";
                lblSavedFileName.Text = "xemay_59X3.jpg";
                lblOutTypeSlotVal.Text = "Xe máy • Ô A-01";
                lblOutDurationVal.Text = "2h 15p";
                lblOutTicketTypeVal.Text = "Vé lượt ngày (5k)";
                _currentBaseFee = 5000m;
                break;
        }

        UpdateTotalOutFee();
    }

    private void ChkLostCard_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateTotalOutFee();
    }

    private void UpdateTotalOutFee()
    {
        decimal penalty = chkLostCard.Checked ? 50000m : 0m;
        decimal total = _currentBaseFee + penalty;

        lblFeeBreakdown.Text = $"Tiền gửi: {_currentBaseFee:N0}đ + Phạt: {penalty:N0}đ";
        lblTotalAmount.Text = $"{total:N0} VNĐ";
    }

    private void BtnSubmitCheckOut_Click(object? sender, EventArgs e)
    {
        MessageBox.Show(
            $"Đã xác nhận thu {lblTotalAmount.Text} và giải phóng chỗ đỗ cho xe {lblOutPlateVal.Text}!",
            "Cho xe ra bãi",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
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

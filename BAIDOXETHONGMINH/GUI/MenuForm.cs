using System.Drawing.Drawing2D;

namespace BAIDOXETHONGMINH.GUI;

/// <summary>
/// Component UI Menu bên trái độc lập, phát sự kiện MenuItemSelected khi người dùng chọn chức năng
/// </summary>
public partial class MenuForm : Form
{
    private Button? _activeButton;

    /// <summary>
    /// Sự kiện báo cho MainLayout biết người dùng vừa bấm chọn mục nào trên Menu
    /// </summary>
    public event Action<string>? MenuItemSelected;

    public MenuForm()
    {
        InitializeComponent();
        SetupRoundedMenuControls();
        HighlightButton(btnCheckInOut);
    }

    private void SetupRoundedMenuControls()
    {
        AttachRoundedCorner(pnlShiftCard, 12, Color.FromArgb(51, 65, 85));
        AttachRoundedCorner(btnCheckInOut, 8, Color.Transparent);
        AttachRoundedCorner(btnParkingMap, 8, Color.Transparent);
        AttachRoundedCorner(btnMonthlyRegister, 8, Color.Transparent);
        AttachRoundedCorner(btnMonthlyRenew, 8, Color.Transparent);
        AttachRoundedCorner(btnCustomer, 8, Color.Transparent);
        AttachRoundedCorner(btnSearchReport, 8, Color.Transparent);
    }

    private static void AttachRoundedCorner(Control control, int radius, Color borderColor)
    {
        control.Paint += (s, e) =>
        {
            if (control.Width <= 2 || control.Height <= 2) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(1, 1, control.Width - 3, control.Height - 3);
            using var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            Color parentBg = control.Parent?.BackColor ?? Color.FromArgb(24, 30, 44);
            using var outsideRegion = new Region(new Rectangle(0, 0, control.Width, control.Height));
            outsideRegion.Exclude(path);
            using var maskBrush = new SolidBrush(parentBg);
            g.FillRegion(maskBrush, outsideRegion);

            if (borderColor != Color.Transparent)
            {
                using var pen = new Pen(borderColor, 1.2f);
                g.DrawPath(pen, path);
            }
        };

        control.Resize += (s, e) => control.Invalidate();
    }

    private void SelectMenuItem(Button clickedButton, string menuKey)
    {
        HighlightButton(clickedButton);
        MenuItemSelected?.Invoke(menuKey);
    }

    private void HighlightButton(Button clickedButton)
    {
        if (_activeButton != null)
        {
            _activeButton.BackColor = Color.FromArgb(24, 30, 44);
            _activeButton.ForeColor = Color.FromArgb(226, 232, 240);
            _activeButton.Font = new Font(_activeButton.Font.FontFamily, _activeButton.Font.Size, FontStyle.Regular);
            _activeButton.Invalidate();
        }

        _activeButton = clickedButton;
        _activeButton.BackColor = Color.FromArgb(37, 99, 235);
        _activeButton.ForeColor = Color.White;
        _activeButton.Font = new Font(_activeButton.Font.FontFamily, _activeButton.Font.Size, FontStyle.Bold);
        _activeButton.Invalidate();
    }

    private void PicAvatar_Paint(object? sender, PaintEventArgs e)
    {
        if (picAvatar.Image != null) return;

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var bgBrush = new SolidBrush(Color.FromArgb(37, 99, 235));
        using var borderPen = new Pen(Color.FromArgb(96, 165, 250), 2f);
        g.FillEllipse(bgBrush, 2, 2, 50, 50);
        g.DrawEllipse(borderPen, 2, 2, 50, 50);

        using var font = new Font("Segoe UI", 13F, FontStyle.Bold);
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString("AD", font, Brushes.White, new RectangleF(2, 2, 50, 50), sf);

        using var statusBrush = new SolidBrush(Color.FromArgb(16, 185, 129));
        g.FillEllipse(statusBrush, 38, 38, 12, 12);
    }
}

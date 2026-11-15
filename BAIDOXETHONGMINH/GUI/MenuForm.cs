using System.Drawing.Drawing2D;

namespace BAIDOXETHONGMINH.GUI;

public partial class MenuForm : Form
{
    private Button? _activeButton;

    public event Action<string>? MenuItemSelected;

    // Hàm khởi tạo MenuForm, thiết lập bo góc và chọn mặc định mục Kiểm soát Xe Vào / Ra
    public MenuForm()
    {
        InitializeComponent();
        SetupRoundedMenuControls();
        HighlightButton(btnCheckInOut);
    }

    // Hàm gắn hiệu ứng bo góc cho thẻ thông tin ca trực và các nút chức năng trên Menu
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

    // Hàm vẽ vùng bo góc và đường viền khử răng cưa cho một Control trên Sidebar
    private static void AttachRoundedCorner(Control control, int radius, Color borderColor)
    {
        control.Paint += (s, e) =>
        {
            if (control.Width <= 2 || control.Height <= 2) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(1, 1, control.Width - 3, control.Height - 3);
            using GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            Color parentBg = control.Parent?.BackColor ?? Color.FromArgb(24, 30, 44);
            using Region outsideRegion = new Region(new Rectangle(0, 0, control.Width, control.Height));
            outsideRegion.Exclude(path);
            using SolidBrush maskBrush = new SolidBrush(parentBg);
            g.FillRegion(maskBrush, outsideRegion);

            if (borderColor != Color.Transparent)
            {
                using Pen pen = new Pen(borderColor, 1.2f);
                g.DrawPath(pen, path);
            }
        };

        control.Resize += (s, e) => control.Invalidate();
    }

    // Hàm xử lý khi người dùng bấm chọn một nút chức năng trên Menu và phát sự kiện sang MainLayout
    private void SelectMenuItem(Button clickedButton, string menuKey)
    {
        HighlightButton(clickedButton);
        MenuItemSelected?.Invoke(menuKey);
    }

    // Hàm cập nhật số lượng chỗ trống hiển thị trên thẻ Ca làm việc ở Sidebar
    public void UpdateSlotCapacity(int availableSlots, int totalSlots)
    {
        lblShiftSlots.Text = $"Chỗ trống: {availableSlots} / {totalSlots} ô";
    }

    // Hàm làm nổi bật nút Menu đang được chọn và đặt lại màu cho nút trước đó
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

    // Hàm vẽ biểu tượng hình đại diện tròn và chấm trạng thái trực tuyến cho nhân viên trực ca
    private void PicAvatar_Paint(object? sender, PaintEventArgs e)
    {
        if (picAvatar.Image != null) return;

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using SolidBrush bgBrush = new SolidBrush(Color.FromArgb(37, 99, 235));
        using Pen borderPen = new Pen(Color.FromArgb(96, 165, 250), 2f);
        g.FillEllipse(bgBrush, 2, 2, 50, 50);
        g.DrawEllipse(borderPen, 2, 2, 50, 50);

        using Font font = new Font("Segoe UI", 13F, FontStyle.Bold);
        StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString("AD", font, Brushes.White, new RectangleF(2, 2, 50, 50), sf);

        using SolidBrush statusBrush = new SolidBrush(Color.FromArgb(16, 185, 129));
        g.FillEllipse(statusBrush, 38, 38, 12, 12);
    }
}

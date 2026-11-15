using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace BAIDOXETHONGMINH.GUI;

/// <summary>
/// Tiện ích dùng chung để bo tròn các khung hình chữ nhật (Panel, Button, Label, Input Box) cho toàn bộ các Form
/// </summary>
public static class RoundedUiHelper
{
    public static readonly Color SlateBorder = Color.FromArgb(148, 163, 184);
    public static readonly Color CardBorder = Color.FromArgb(203, 213, 225);

    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    private static extern IntPtr CreateRoundRectRgn(
        int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    [DllImport("Gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    public static void AttachRoundedFrame(Control control, int radius, Color borderColor)
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
        if (control.IsHandleCreated) UpdateRegion();

        control.Paint += (s, e) =>
        {
            if (control.Width <= 4 || control.Height <= 4) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(1, 1, control.Width - 3, control.Height - 3);
            using var path = CreateRoundedRectanglePath(rect, Math.Max(2, radius / 2));
            using var borderPen = new Pen(borderColor, 1.4f);
            g.DrawPath(borderPen, path);
        };
    }

    public static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
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

    public static void StyleDataGridView(DataGridView dgv)
    {
        dgv.BackgroundColor = Color.White;
        dgv.BorderStyle = BorderStyle.None;
        dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dgv.GridColor = Color.FromArgb(226, 232, 240);
        dgv.EnableHeadersVisualStyles = false;
        dgv.RowHeadersVisible = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.MultiSelect = false;
        dgv.ReadOnly = true;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToResizeRows = false;
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.ColumnHeadersHeight = 36;
        dgv.RowTemplate.Height = 34;

        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

        dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
        dgv.DefaultCellStyle.ForeColor = Color.FromArgb(15, 23, 42);
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
        dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 64, 175);
    }
}

#nullable enable
namespace BAIDOXETHONGMINH.GUI;

partial class MainLayout
{
    private System.ComponentModel.IContainer? components = null;

    private Panel pnlLeftMenu = null!;
    private Panel pnlRightContent = null!;

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
        pnlLeftMenu = new Panel();
        pnlRightContent = new Panel();

        SuspendLayout();

        pnlLeftMenu.Dock = DockStyle.Left;
        pnlLeftMenu.Width = 260;
        pnlLeftMenu.BackColor = Color.FromArgb(24, 30, 44);

        pnlRightContent.Dock = DockStyle.Fill;
        pnlRightContent.BackColor = Color.FromArgb(248, 250, 252);

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1280, 760);
        MinimumSize = new Size(1060, 680);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Hệ thống Quản lý Bãi đỗ xe Thông minh";
        Controls.Add(pnlRightContent);
        Controls.Add(pnlLeftMenu);

        ResumeLayout(false);
    }
}

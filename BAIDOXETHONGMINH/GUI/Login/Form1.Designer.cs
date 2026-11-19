namespace BAIDOXETHONGMINH.GUI;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;
    private Panel pnlCard = null!;
    private Label lblAppBadge = null!;
    private Label lblTitle = null!;
    private Label lblSubtitle = null!;

    private Panel pnlLoginView = null!;
    private Label lblEmail = null!;
    private Panel pnlEmailBox = null!;
    private TextBox txtEmail = null!;
    private Label lblPassword = null!;
    private Panel pnlPasswordBox = null!;
    private TextBox txtPassword = null!;
    private Label lblShift = null!;
    private Panel pnlShiftBox = null!;
    private ComboBox cboShift = null!;
    private Button btnLogin = null!;
    private Button btnForgotPassword = null!;

    private Panel pnlForgotView = null!;
    private Label lblForgotHeader = null!;
    private Label lblVerifyCccd = null!;
    private Panel pnlVerifyCccdBox = null!;
    private TextBox txtVerifyCccd = null!;
    private Label lblVerifyEmpCode = null!;
    private Panel pnlVerifyEmpCodeBox = null!;
    private TextBox txtVerifyEmpCode = null!;
    private Button btnVerifyIdentity = null!;
    private Label lblVerifiedUserStatus = null!;

    private Panel pnlNewPasswordSection = null!;
    private Label lblNewPassword = null!;
    private Panel pnlNewPasswordBox = null!;
    private TextBox txtNewPassword = null!;
    private Label lblConfirmPassword = null!;
    private Panel pnlConfirmPasswordBox = null!;
    private TextBox txtConfirmPassword = null!;
    private Button btnSaveNewPassword = null!;
    private Button btnBackToLogin = null!;

    // Hàm giải phóng tài nguyên giao diện khi đóng Form đăng nhập
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    // Hàm khởi tạo bố cục giao diện cho màn hình Đăng nhập và Quên / Đổi mật khẩu
    private void InitializeComponent()
    {
        this.SuspendLayout();

        this.BackColor = Color.FromArgb(241, 245, 249);
        this.ClientSize = new Size(500, 568);
        this.Font = new Font("Segoe UI", 9.5F);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.Name = "Form1";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "Đăng Nhập Hệ Thống - Quản Lý Bãi Đỗ Xe Thông Minh";

        pnlCard = new Panel
        {
            Location = new Point(24, 20),
            Size = new Size(452, 528),
            BackColor = Color.White
        };

        lblAppBadge = new Label
        {
            Text = "BÃI ĐỖ XE THÔNG MINH",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(37, 99, 235),
            BackColor = Color.FromArgb(219, 234, 254),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(126, 20),
            Size = new Size(200, 26)
        };

        lblTitle = new Label
        {
            Text = "ĐĂNG NHẬP HỆ THỐNG",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(24, 54),
            Size = new Size(404, 34)
        };

        lblSubtitle = new Label
        {
            Text = "Xác thực nhân viên trực ca & quản lý bãi xe",
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(100, 116, 139),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(24, 88),
            Size = new Size(404, 22)
        };

        pnlLoginView = new Panel
        {
            Location = new Point(28, 122),
            Size = new Size(396, 384),
            BackColor = Color.White
        };

        lblEmail = new Label
        {
            Text = "Email hoặc Tên đăng nhập:",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 65, 85),
            AutoSize = true,
            Location = new Point(4, 6)
        };

        pnlEmailBox = CreateInputBox(new Point(4, 30), new Size(388, 38), out txtEmail, "Nhập email hoặc tên đăng nhập...");

        lblPassword = new Label
        {
            Text = "Mật khẩu:",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 65, 85),
            AutoSize = true,
            Location = new Point(4, 82)
        };

        pnlPasswordBox = CreateInputBox(new Point(4, 106), new Size(388, 38), out txtPassword, "");
        txtPassword.UseSystemPasswordChar = true;

        lblShift = new Label
        {
            Text = "Ca trực làm việc:",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 65, 85),
            AutoSize = true,
            Location = new Point(4, 158)
        };

        pnlShiftBox = new Panel
        {
            Location = new Point(4, 182),
            Size = new Size(388, 38),
            BackColor = Color.FromArgb(248, 250, 252)
        };

        cboShift = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F),
            ForeColor = Color.FromArgb(15, 23, 42),
            BackColor = Color.FromArgb(248, 250, 252),
            Location = new Point(10, 6),
            Size = new Size(368, 28)
        };
        cboShift.Items.AddRange(new object[]
        {
            "Ca Sáng (06:00 - 14:00)",
            "Ca Chiều (14:00 - 22:00)",
            "Ca Đêm (22:00 - 06:00)"
        });
        cboShift.SelectedIndex = 0;
        pnlShiftBox.Controls.Add(cboShift);

        btnLogin = new Button
        {
            Text = "ĐĂNG NHẬP VÀO CA TRỰC",
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Location = new Point(4, 244),
            Size = new Size(388, 44),
            Cursor = Cursors.Hand
        };
        btnLogin.FlatAppearance.BorderSize = 0;

        btnForgotPassword = new Button
        {
            Text = "Quên mật khẩu / Đổi mật khẩu",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            BackColor = Color.FromArgb(241, 245, 249),
            ForeColor = Color.FromArgb(37, 99, 235),
            FlatStyle = FlatStyle.Flat,
            Location = new Point(4, 302),
            Size = new Size(388, 40),
            Cursor = Cursors.Hand
        };
        btnForgotPassword.FlatAppearance.BorderSize = 0;

        pnlLoginView.Controls.AddRange(new Control[]
        {
            lblEmail, pnlEmailBox,
            lblPassword, pnlPasswordBox,
            lblShift, pnlShiftBox,
            btnLogin, btnForgotPassword
        });

        pnlForgotView = new Panel
        {
            Location = new Point(28, 122),
            Size = new Size(396, 396),
            BackColor = Color.White,
            Visible = false
        };

        lblForgotHeader = new Label
        {
            Text = "XÁC MINH CĂN CƯỚC & MÃ NHÂN VIÊN",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 64, 175),
            Location = new Point(4, 0),
            Size = new Size(388, 22)
        };

        lblVerifyCccd = new Label
        {
            Text = "Số Căn cước công dân (CCCD):",
            Font = new Font("Segoe UI", 8.75F, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 65, 85),
            AutoSize = true,
            Location = new Point(4, 24)
        };

        pnlVerifyCccdBox = CreateInputBox(new Point(4, 44), new Size(388, 34), out txtVerifyCccd, "Nhập số Căn cước công dân...");

        lblVerifyEmpCode = new Label
        {
            Text = "Mã nhân viên:",
            Font = new Font("Segoe UI", 8.75F, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 65, 85),
            AutoSize = true,
            Location = new Point(4, 84)
        };

        pnlVerifyEmpCodeBox = CreateInputBox(new Point(4, 104), new Size(246, 34), out txtVerifyEmpCode, "Nhập mã nhân viên...");

        btnVerifyIdentity = new Button
        {
            Text = "Xác minh",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Location = new Point(258, 104),
            Size = new Size(134, 34),
            Cursor = Cursors.Hand
        };
        btnVerifyIdentity.FlatAppearance.BorderSize = 0;

        lblVerifiedUserStatus = new Label
        {
            Text = "Vui lòng nhập CCCD và Mã nhân viên rồi bấm Xác minh.",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
            ForeColor = Color.FromArgb(100, 116, 139),
            Location = new Point(4, 142),
            Size = new Size(388, 20)
        };

        pnlNewPasswordSection = new Panel
        {
            Location = new Point(0, 166),
            Size = new Size(396, 184),
            BackColor = Color.White,
            Visible = false
        };

        lblNewPassword = new Label
        {
            Text = "Mật khẩu mới:",
            Font = new Font("Segoe UI", 8.75F, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 65, 85),
            AutoSize = true,
            Location = new Point(4, 2)
        };

        pnlNewPasswordBox = CreateInputBox(new Point(4, 22), new Size(388, 34), out txtNewPassword, "");
        txtNewPassword.UseSystemPasswordChar = true;

        lblConfirmPassword = new Label
        {
            Text = "Nhập lại mật khẩu mới:",
            Font = new Font("Segoe UI", 8.75F, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 65, 85),
            AutoSize = true,
            Location = new Point(4, 62)
        };

        pnlConfirmPasswordBox = CreateInputBox(new Point(4, 82), new Size(388, 34), out txtConfirmPassword, "");
        txtConfirmPassword.UseSystemPasswordChar = true;

        btnSaveNewPassword = new Button
        {
            Text = "Thiết lập mật khẩu mới",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            BackColor = Color.FromArgb(16, 185, 129),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Location = new Point(4, 128),
            Size = new Size(388, 38),
            Cursor = Cursors.Hand
        };
        btnSaveNewPassword.FlatAppearance.BorderSize = 0;

        pnlNewPasswordSection.Controls.AddRange(new Control[]
        {
            lblNewPassword, pnlNewPasswordBox,
            lblConfirmPassword, pnlConfirmPasswordBox,
            btnSaveNewPassword
        });

        btnBackToLogin = new Button
        {
            Text = "← Quay lại trang Đăng nhập",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            BackColor = Color.FromArgb(241, 245, 249),
            ForeColor = Color.FromArgb(71, 85, 105),
            FlatStyle = FlatStyle.Flat,
            Location = new Point(4, 354),
            Size = new Size(388, 36),
            Cursor = Cursors.Hand
        };
        btnBackToLogin.FlatAppearance.BorderSize = 0;

        pnlForgotView.Controls.AddRange(new Control[]
        {
            lblForgotHeader,
            lblVerifyCccd, pnlVerifyCccdBox,
            lblVerifyEmpCode, pnlVerifyEmpCodeBox, btnVerifyIdentity,
            lblVerifiedUserStatus,
            pnlNewPasswordSection,
            btnBackToLogin
        });

        pnlCard.Controls.AddRange(new Control[]
        {
            lblAppBadge,
            lblTitle,
            lblSubtitle,
            pnlLoginView,
            pnlForgotView
        });

        this.Controls.Add(pnlCard);
        this.ResumeLayout(false);
    }

    // Hàm tạo ô nhập liệu có khung nền sáng và hiệu ứng placeholder cho Form Đăng nhập
    private static Panel CreateInputBox(Point location, Size size, out TextBox textBox, string placeholder)
    {
        Panel box = new Panel
        {
            Location = location,
            Size = size,
            BackColor = Color.FromArgb(248, 250, 252)
        };

        TextBox tb = new TextBox
        {
            BorderStyle = BorderStyle.None,
            BackColor = Color.FromArgb(248, 250, 252),
            Font = new Font("Segoe UI", 10F),
            ForeColor = string.IsNullOrEmpty(placeholder) ? Color.FromArgb(15, 23, 42) : Color.Gray,
            Text = placeholder,
            Location = new Point(12, Math.Max(4, (size.Height - 20) / 2)),
            Width = size.Width - 24
        };

        if (!string.IsNullOrEmpty(placeholder))
        {
            tb.Enter += (s, e) =>
            {
                if (tb.Text == placeholder)
                {
                    tb.Text = "";
                    tb.ForeColor = Color.FromArgb(15, 23, 42);
                }
            };

            tb.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Text = placeholder;
                    tb.ForeColor = Color.Gray;
                }
            };
        }

        box.Controls.Add(tb);
        box.Click += (s, e) => tb.Focus();
        textBox = tb;
        return box;
    }
}

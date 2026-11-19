using BAIDOXETHONGMINH.DAL.Repositories;
using BAIDOXETHONGMINH.DTO.Entities;

namespace BAIDOXETHONGMINH.GUI;

public partial class Form1 : Form
{
    private readonly ParkingRepository _repository = new ParkingRepository();
    private User? _verifiedUserForReset;

    public User? AuthenticatedUser { get; private set; }
    public string SelectedShiftName { get; private set; } = "Ca Sáng";

    // Hàm khởi tạo Form Đăng nhập, gắn bo góc, tự chọn đúng ca trực theo giờ hệ thống và đăng ký sự kiện
    public Form1()
    {
        InitializeComponent();
        SetupRoundedLoginControls();
        SelectCurrentShiftByClock();
        WireEvents();
    }

    // Hàm tự động chọn sẵn đúng ca trực hiện tại trên ComboBox dựa theo đồng hồ hệ thống
    private void SelectCurrentShiftByClock()
    {
        int currentShiftIndex = GetShiftIndexByTime(DateTime.Now);
        if (currentShiftIndex >= 0 && currentShiftIndex < cboShift.Items.Count)
        {
            cboShift.SelectedIndex = currentShiftIndex;
        }
    }

    // Hàm xác định chỉ số ca trực theo giờ hệ thống (0: Ca Sáng 06h-14h, 1: Ca Chiều 14h-22h, 2: Ca Đêm 22h-06h)
    private static int GetShiftIndexByTime(DateTime now)
    {
        int hour = now.Hour;
        if (hour >= 6 && hour < 14)
        {
            return 0;
        }

        if (hour >= 14 && hour < 22)
        {
            return 1;
        }

        return 2;
    }

    // Hàm trả về tên hiển thị đầy đủ kèm khung giờ của ca trực theo chỉ số
    private static string GetShiftDisplayNameByIndex(int shiftIndex)
    {
        if (shiftIndex == 0)
        {
            return "Ca Sáng (06:00 - 14:00)";
        }

        if (shiftIndex == 1)
        {
            return "Ca Chiều (14:00 - 22:00)";
        }

        return "Ca Đêm (22:00 - 06:00)";
    }

    // Hàm kiểm tra ca trực người dùng chọn có khớp với khung giờ thực tế hiện tại hay không
    private bool IsSelectedShiftValidForCurrentTime(DateTime now, out int expectedShiftIndex, out string expectedShiftDisplay, out string selectedShiftDisplay)
    {
        expectedShiftIndex = GetShiftIndexByTime(now);
        expectedShiftDisplay = GetShiftDisplayNameByIndex(expectedShiftIndex);
        selectedShiftDisplay = cboShift.SelectedItem?.ToString() ?? GetShiftDisplayNameByIndex(cboShift.SelectedIndex);
        return cboShift.SelectedIndex == expectedShiftIndex;
    }

    // Hàm gắn hiệu ứng bo góc cho khung thẻ và các ô nhập liệu, nút bấm trên Form Đăng nhập
    private void SetupRoundedLoginControls()
    {
        RoundedUiHelper.AttachRoundedFrame(pnlCard, 18, RoundedUiHelper.CardBorder);
        RoundedUiHelper.AttachRoundedFrame(lblAppBadge, 12, Color.FromArgb(191, 219, 254));

        RoundedUiHelper.AttachRoundedFrame(pnlEmailBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlPasswordBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlShiftBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(btnLogin, 10, Color.FromArgb(37, 99, 235));
        RoundedUiHelper.AttachRoundedFrame(btnForgotPassword, 10, RoundedUiHelper.CardBorder);

        RoundedUiHelper.AttachRoundedFrame(pnlVerifyCccdBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlVerifyEmpCodeBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(btnVerifyIdentity, 10, Color.FromArgb(37, 99, 235));
        RoundedUiHelper.AttachRoundedFrame(pnlNewPasswordBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlConfirmPasswordBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(btnSaveNewPassword, 10, Color.FromArgb(16, 185, 129));
        RoundedUiHelper.AttachRoundedFrame(btnBackToLogin, 10, RoundedUiHelper.CardBorder);
    }

    // Hàm đăng ký sự kiện click và phím Enter cho các điều khiển trên màn hình Đăng nhập và Quên mật khẩu
    private void WireEvents()
    {
        btnLogin.Click += BtnLogin_Click;
        btnForgotPassword.Click += BtnForgotPassword_Click;
        btnVerifyIdentity.Click += BtnVerifyIdentity_Click;
        btnSaveNewPassword.Click += BtnSaveNewPassword_Click;
        btnBackToLogin.Click += BtnBackToLogin_Click;

        txtEmail.KeyDown += LoginInput_KeyDown;
        txtPassword.KeyDown += LoginInput_KeyDown;
        txtVerifyCccd.KeyDown += VerifyInput_KeyDown;
        txtVerifyEmpCode.KeyDown += VerifyInput_KeyDown;
        txtNewPassword.KeyDown += NewPasswordInput_KeyDown;
        txtConfirmPassword.KeyDown += NewPasswordInput_KeyDown;
    }

    // Hàm xử lý phím Enter tại ô Email hoặc Mật khẩu để thực hiện Đăng nhập ngay
    private void LoginInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            BtnLogin_Click(sender, EventArgs.Empty);
        }
    }

    // Hàm xử lý phím Enter tại ô CCCD hoặc Mã nhân viên để thực hiện Xác minh ngay
    private void VerifyInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            BtnVerifyIdentity_Click(sender, EventArgs.Empty);
        }
    }

    // Hàm xử lý phím Enter tại ô Mật khẩu mới để lưu mật khẩu mới ngay
    private void NewPasswordInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            BtnSaveNewPassword_Click(sender, EventArgs.Empty);
        }
    }

    // Hàm xác thực Email và Mật khẩu, mở ca trực trong CSDL và chuyển vào màn hình chính
    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        string emailInput = GetCleanInputText(txtEmail, "Nhập email hoặc tên đăng nhập...");
        string passwordInput = txtPassword.Text.Trim();

        if (string.IsNullOrWhiteSpace(emailInput) || string.IsNullOrWhiteSpace(passwordInput))
        {
            MessageBox.Show(
                "Vui lòng nhập đầy đủ Email và Mật khẩu để đăng nhập!",
                "Thiếu thông tin",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        User? user = _repository.AuthenticateUserByEmail(emailInput, passwordInput);
        if (user == null)
        {
            MessageBox.Show(
                "Email hoặc mật khẩu không chính xác!",
                "Đăng nhập thất bại",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            txtPassword.Focus();
            txtPassword.SelectAll();
            return;
        }

        DateTime now = DateTime.Now;
        if (!IsSelectedShiftValidForCurrentTime(now, out int expectedShiftIndex, out string expectedShiftDisplay, out string selectedShiftDisplay))
        {
            MessageBox.Show(
                $"Hiện tại là {now:HH:mm} — đang thuộc khung giờ {expectedShiftDisplay}.\n" +
                $"Bạn không thể đăng nhập vào {selectedShiftDisplay}!\n\n" +
                $"Hệ thống đã tự động chuyển lại về {expectedShiftDisplay}.",
                "Sai khung giờ ca trực",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            if (expectedShiftIndex >= 0 && expectedShiftIndex < cboShift.Items.Count)
            {
                cboShift.SelectedIndex = expectedShiftIndex;
            }
            cboShift.Focus();
            return;
        }

        string rawShiftSelection = cboShift.SelectedItem?.ToString() ?? expectedShiftDisplay;
        (long ShiftId, string ShiftName) shiftInfo = _repository.EnsureOpenWorkShift(user.UserId, rawShiftSelection);

        AuthenticatedUser = user;
        SelectedShiftName = shiftInfo.ShiftName;
        AppService.CurrentUserId = user.UserId;

        DialogResult = DialogResult.OK;
        Close();
    }

    // Hàm chuyển từ giao diện Đăng nhập sang giao diện Xác minh CCCD & Mã nhân viên để đổi mật khẩu
    private void BtnForgotPassword_Click(object? sender, EventArgs e)
    {
        _verifiedUserForReset = null;
        ResetInputWithPlaceholder(txtVerifyCccd, "Nhập số Căn cước công dân...");
        ResetInputWithPlaceholder(txtVerifyEmpCode, "Nhập mã nhân viên...");
        txtNewPassword.Text = "";
        txtConfirmPassword.Text = "";
        lblVerifiedUserStatus.Text = "Vui lòng nhập CCCD và Mã nhân viên rồi bấm Xác minh.";
        lblVerifiedUserStatus.ForeColor = Color.FromArgb(100, 116, 139);
        pnlNewPasswordSection.Visible = false;

        lblTitle.Text = "QUÊN / ĐỔI MẬT KHẨU";
        lblSubtitle.Text = "Xác minh Căn cước công dân và Mã nhân viên";
        pnlLoginView.Visible = false;
        pnlForgotView.Visible = true;
        txtVerifyCccd.Focus();
    }

    // Hàm kiểm tra số CCCD và Mã nhân viên, nếu khớp thì hiển thị khung nhập mật khẩu mới
    private void BtnVerifyIdentity_Click(object? sender, EventArgs e)
    {
        string cccd = GetCleanInputText(txtVerifyCccd, "Nhập số Căn cước công dân...");
        string empCode = GetCleanInputText(txtVerifyEmpCode, "Nhập mã nhân viên...");

        if (string.IsNullOrWhiteSpace(cccd) || string.IsNullOrWhiteSpace(empCode))
        {
            MessageBox.Show(
                "Vui lòng nhập đầy đủ số Căn cước công dân (CCCD) và Mã nhân viên!",
                "Thiếu thông tin xác minh",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        User? verifiedUser = _repository.VerifyUserForPasswordReset(cccd, empCode);
        if (verifiedUser == null)
        {
            _verifiedUserForReset = null;
            pnlNewPasswordSection.Visible = false;
            lblVerifiedUserStatus.Text = "Không tìm thấy nhân viên khớp với CCCD và Mã nhân viên đã nhập!";
            lblVerifiedUserStatus.ForeColor = Color.FromArgb(220, 38, 38);
            MessageBox.Show(
                "Số Căn cước công dân hoặc Mã nhân viên không chính xác!",
                "Xác minh thất bại",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        _verifiedUserForReset = verifiedUser;
        lblVerifiedUserStatus.Text = $"✔ Đã xác minh: {verifiedUser.FullName} ({verifiedUser.Email}) — Mời nhập mật khẩu mới:";
        lblVerifiedUserStatus.ForeColor = Color.FromArgb(5, 150, 105);
        pnlNewPasswordSection.Visible = true;
        txtNewPassword.Text = "";
        txtConfirmPassword.Text = "";
        txtNewPassword.Focus();
    }

    // Hàm lưu mật khẩu mới sau khi xác minh thành công và tự động quay lại trang Đăng nhập
    private void BtnSaveNewPassword_Click(object? sender, EventArgs e)
    {
        if (_verifiedUserForReset == null)
        {
            MessageBox.Show(
                "Vui lòng xác minh CCCD và Mã nhân viên trước khi đặt mật khẩu mới!",
                "Chưa xác minh",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        string newPass = txtNewPassword.Text.Trim();
        string confirmPass = txtConfirmPassword.Text.Trim();

        if (string.IsNullOrWhiteSpace(newPass))
        {
            MessageBox.Show(
                "Mật khẩu mới không được để trống!",
                "Cảnh báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            txtNewPassword.Focus();
            return;
        }

        if (!string.Equals(newPass, confirmPass, StringComparison.Ordinal))
        {
            MessageBox.Show(
                "Mật khẩu xác nhận không khớp với mật khẩu mới!",
                "Cảnh báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            txtConfirmPassword.Focus();
            txtConfirmPassword.SelectAll();
            return;
        }

        bool isUpdated = _repository.ResetUserPassword(_verifiedUserForReset.UserId, newPass);
        if (!isUpdated)
        {
            MessageBox.Show(
                "Không thể cập nhật mật khẩu mới!",
                "Lỗi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        string userEmail = string.IsNullOrWhiteSpace(_verifiedUserForReset.Email)
            ? _verifiedUserForReset.Username
            : _verifiedUserForReset.Email;

        MessageBox.Show(
            $"Thiết lập mật khẩu mới thành công cho tài khoản {userEmail}!\nHệ thống sẽ quay lại trang Đăng nhập.",
            "Đổi mật khẩu thành công",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        SwitchBackToLoginView(userEmail);
    }

    // Hàm xử lý khi bấm nút Quay lại trang Đăng nhập từ màn hình Quên mật khẩu
    private void BtnBackToLogin_Click(object? sender, EventArgs e)
    {
        SwitchBackToLoginView(null);
    }

    // Hàm chuyển giao diện từ màn hình Quên mật khẩu về lại trang Đăng nhập
    private void SwitchBackToLoginView(string? prefillEmail)
    {
        _verifiedUserForReset = null;
        pnlNewPasswordSection.Visible = false;
        pnlForgotView.Visible = false;
        pnlLoginView.Visible = true;

        lblTitle.Text = "ĐĂNG NHẬP HỆ THỐNG";
        lblSubtitle.Text = "Xác thực nhân viên trực ca & quản lý bãi xe";

        if (!string.IsNullOrWhiteSpace(prefillEmail))
        {
            txtEmail.Text = prefillEmail;
            txtEmail.ForeColor = Color.FromArgb(15, 23, 42);
        }

        SelectCurrentShiftByClock();
        txtPassword.Text = "";
        txtPassword.Focus();
    }

    // Hàm lấy nội dung người dùng đã nhập vào TextBox sau khi loại bỏ chuỗi placeholder
    private static string GetCleanInputText(TextBox textBox, string placeholder)
    {
        string raw = textBox.Text.Trim();
        if (string.Equals(raw, placeholder, StringComparison.Ordinal))
        {
            return string.Empty;
        }

        return raw;
    }

    // Hàm khôi phục trạng thái placeholder mặc định cho một TextBox
    private static void ResetInputWithPlaceholder(TextBox textBox, string placeholder)
    {
        textBox.Text = placeholder;
        textBox.ForeColor = Color.Gray;
    }
}

namespace BAIDOXETHONGMINH.GUI;

public partial class MonthlyPassForm : Form
{
    public MonthlyPassForm()
    {
        InitializeComponent();

        RoundedUiHelper.AttachRoundedFrame(pnlLeftForm, 16, RoundedUiHelper.CardBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlRightGrid, 16, RoundedUiHelper.CardBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlNameBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlPhoneBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlCccdBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlPlateBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlTypeBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlBrandBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlStartDateBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlMonthsBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlFeeBanner, 10, Color.FromArgb(147, 197, 253));
        RoundedUiHelper.AttachRoundedFrame(btnRegister, 10, Color.FromArgb(37, 99, 235));
        RoundedUiHelper.AttachRoundedFrame(btnClear, 10, RoundedUiHelper.SlateBorder);

        SetupSampleGrid();
    }

    private void SetupSampleGrid()
    {
        dgvMonthlyPasses.Columns.Add("PassCode", "Mã Vé");
        dgvMonthlyPasses.Columns.Add("LicensePlate", "Biển số");
        dgvMonthlyPasses.Columns.Add("Customer", "Chủ xe / SĐT");
        dgvMonthlyPasses.Columns.Add("Vehicle", "Phương tiện");
        dgvMonthlyPasses.Columns.Add("Duration", "Thời hạn");
        dgvMonthlyPasses.Columns.Add("Status", "Trạng thái");

        dgvMonthlyPasses.Rows.Add("MP-01", "51H-999.99", "Trần Minh Hoàng (0908123456)", "Ô tô • Mazda CX-5 Trắng", "01/10/2026 → 01/11/2026", "Sắp hết hạn (3 ngày)");
        dgvMonthlyPasses.Rows.Add("MP-02", "59P2-345.67", "Nguyễn Thị Lan (0912345678)", "Xe máy • Honda Vision Đỏ", "15/10/2026 → 15/12/2026", "Còn hạn");
        dgvMonthlyPasses.Rows.Add("MP-03", "59T1-882.10", "Lê Quốc Bảo (0987654321)", "Xe máy • AirBlade Đen", "01/09/2026 → 01/10/2026", "Đã hết hạn");
    }

    private void RecalcFee()
    {
        decimal unit = cboVehicleType.SelectedIndex == 0 ? 150000m : 1200000m;
        int months = cboMonths.SelectedIndex switch { 1 => 3, 2 => 6, _ => 1 };
        lblFeeValue.Text = $"{unit * months:N0} VNĐ";
    }

    private void BtnRegister_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPlate.Text))
        {
            MessageBox.Show("Vui lòng nhập đầy đủ Họ tên chủ xe và Biển số xe!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string code = $"MP-0{dgvMonthlyPasses.Rows.Count + 1}";
        string plate = txtPlate.Text.Trim().ToUpperInvariant();
        string type = cboVehicleType.SelectedIndex == 0 ? "Xe máy" : "Ô tô";
        int months = cboMonths.SelectedIndex switch { 1 => 3, 2 => 6, _ => 1 };
        string endDate = DateTime.Now.AddMonths(months).ToString("dd/MM/yyyy");

        dgvMonthlyPasses.Rows.Insert(0, code, plate, $"{txtName.Text.Trim()} ({txtPhone.Text.Trim()})",
            $"{type} • {txtBrand.Text.Trim()}", $"{txtStartDate.Text} → {endDate}", "Còn hạn");

        MessageBox.Show($"Đã đăng ký mới Vé tháng {code} cho xe {plate} thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnClear_Click(object? sender, EventArgs e)
    {
        txtName.Clear();
        txtPhone.Clear();
        txtCccd.Clear();
        txtPlate.Clear();
        txtBrand.Clear();
    }
}

namespace BAIDOXETHONGMINH.GUI;

public partial class MonthlyPassForm : Form
{
    public MonthlyPassForm(int tabIndex = 0)
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

    private bool _ready;
    private bool _saving;
    private int _feeVersion;
    private readonly MonthlyPassService _service = AppService.Monthly;

    private void SetupSampleGrid()
    {
        dgvMonthlyPasses.Columns.Add("PassCode", "Mã Vé");
        dgvMonthlyPasses.Columns.Add("LicensePlate", "Biển số");
        dgvMonthlyPasses.Columns.Add("Customer", "Chủ xe / SĐT");
        dgvMonthlyPasses.Columns.Add("Vehicle", "Phương tiện");
        dgvMonthlyPasses.Columns.Add("Duration", "Thời hạn");
        dgvMonthlyPasses.Columns.Add("Status", "Trạng thái");
        Load += async (_, _) => await Dialogs.RunAsync(async () =>
        {
            await AppService.InitializeAsync();
            cboVehicleType.DisplayMember = "TypeName";
            cboVehicleType.ValueMember = "VehicleTypeId";
            cboVehicleType.DataSource = await AppService.Pricing.GetVehicleTypesAsync();
            _ready = true;
            RecalcFee();
            await RefreshPassesAsync();
        });
    }

    private async Task RefreshPassesAsync()
    {
        var passes = await _service.GetViewsAsync();
        if (IsDisposed) return;
        dgvMonthlyPasses.Rows.Clear();
        foreach (var pass in passes)
            dgvMonthlyPasses.Rows.Add(pass.Code, pass.LicensePlate, pass.OwnerText,
                pass.VehicleText, pass.PeriodText, pass.StatusText);
    }

    private int Months => cboMonths.SelectedIndex switch { 1 => 3, 2 => 6, _ => 1 };

    private async void RecalcFee()
    {
        if (!_ready || cboVehicleType.SelectedValue is not string typeId) return;
        int version = ++_feeVersion;
        try
        {
            var fee = await _service.GetFeeAsync(typeId, Months);
            if (!IsDisposed && version == _feeVersion) lblFeeValue.Text = Fmt.Vnd(fee);
        }
        catch (Exception)
        {
            if (!IsDisposed && version == _feeVersion) lblFeeValue.Text = "Chưa có bảng giá";
        }
    }

    private async void BtnRegister_Click(object? sender, EventArgs e)
    {
        if (_saving || !_ready) return;
        _saving = true;
        btnRegister.Enabled = false;
        try
        {
            await Dialogs.RunAsync(async () =>
            {
                if (cboVehicleType.SelectedValue is not string typeId)
                    throw new ServiceException("Hãy chọn loại xe.");
                var pass = await _service.RegisterAsync(txtName.Text, txtPhone.Text, txtCccd.Text,
                    txtPlate.Text, typeId, txtBrand.Text, Months);
                await RefreshPassesAsync();
                BtnClear_Click(null, EventArgs.Empty);
                MessageBox.Show($"Đã đăng ký vé MP-{pass.PassId:00} cho xe {pass.LicensePlate}.",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }
        finally { _saving = false; if (!IsDisposed) btnRegister.Enabled = true; }
    }

    private void BtnClear_Click(object? sender, EventArgs e)
    {
        txtName.Clear();
        txtPhone.Clear();
        txtCccd.Clear();
        txtPlate.Clear();
        txtBrand.Clear();
        txtStartDate.Text = DateTime.Today.ToString("dd/MM/yyyy");
    }
}
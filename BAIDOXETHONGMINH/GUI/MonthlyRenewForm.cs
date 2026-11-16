using BAIDOXETHONGMINH.Models;
namespace BAIDOXETHONGMINH.GUI;

public partial class MonthlyRenewForm : Form
{
    public MonthlyRenewForm()
    {
        InitializeComponent();

        RoundedUiHelper.AttachRoundedFrame(pnlLeftTable, 16, RoundedUiHelper.CardBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlSearchBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlRenewCard, 16, RoundedUiHelper.CardBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlRenewMonthsBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlRenewSummary, 12, Color.FromArgb(110, 231, 183));
        RoundedUiHelper.AttachRoundedFrame(btnConfirmRenew, 10, Color.FromArgb(5, 150, 105));

        RoundedUiHelper.AttachRoundedFrame(pnlUpdateCard, 16, RoundedUiHelper.CardBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlUpdPhoneBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlUpdBrandBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(btnSaveUpdate, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(btnCancelPass, 10, Color.FromArgb(248, 113, 113));

        SetupGrid();
    }

    private readonly MonthlyPassService _service = AppService.Monthly;
    private bool _saving;
    private int _searchVersion;
    private int _previewVersion;
    private MonthlyPassView? SelectedPass => dgvPasses.CurrentRow?.Tag as MonthlyPassView;
    private int Months => cboRenewMonths.SelectedIndex switch { 1 => 3, 2 => 6, _ => 1 };

    private void SetupGrid()
    {
        dgvPasses.Columns.Add("Code", "Mã Vé");
        dgvPasses.Columns.Add("Plate", "Biển số");
        dgvPasses.Columns.Add("Owner", "Chủ xe");
        dgvPasses.Columns.Add("EndDate", "Hạn hiện tại");
        dgvPasses.Columns.Add("Status", "Trạng thái");
        Load += async (_, _) => await Dialogs.RunAsync(async () =>
        {
            await AppService.InitializeAsync();
            await RefreshPassesAsync();
        });
        txtSearch.TextChanged += async (_, _) => await Dialogs.RunAsync(RefreshPassesAsync);
    }

    private async Task RefreshPassesAsync()
    {
        int version = ++_searchVersion;
        int? selectedId = SelectedPass?.PassId;
        var passes = await _service.GetViewsAsync(txtSearch.Text);
        if (IsDisposed || version != _searchVersion) return;
        dgvPasses.Rows.Clear();
        foreach (var pass in passes)
        {
            int index = dgvPasses.Rows.Add(pass.Code, pass.LicensePlate, pass.OwnerName, pass.EndDateText, pass.StatusText);
            dgvPasses.Rows[index].Tag = pass;
        }
        var row = dgvPasses.Rows.Cast<DataGridViewRow>().FirstOrDefault(r =>
            (r.Tag as MonthlyPassView)?.PassId == selectedId) ?? dgvPasses.Rows.Cast<DataGridViewRow>().FirstOrDefault();
        if (row != null) dgvPasses.CurrentCell = row.Cells[0];
        DgvPasses_SelectionChanged(null, EventArgs.Empty);
    }

    // Hàm hiển thị thông tin chi tiết của vé tháng đang chọn và khóa các nút/ô nhập khi chưa có dòng được chọn
    private void DgvPasses_SelectionChanged(object? sender, EventArgs e)
    {
        MonthlyPassView? pass = SelectedPass;
        lblSelPassCode.Text = pass == null ? "—" : $"{pass.Code} ({pass.LicensePlate})";
        lblSelOwner.Text = pass?.OwnerName ?? "—";
        lblSelOldEnd.Text = pass?.EndDateText ?? "—";
        txtUpdPhone.Text = pass?.Phone ?? string.Empty;
        txtUpdBrand.Text = pass?.Brand ?? string.Empty;
        txtUpdPhone.Enabled = !_saving && pass != null;
        txtUpdBrand.Enabled = !_saving && pass != null;
        btnConfirmRenew.Enabled = !_saving && pass != null && pass.Status != PassStatus.Cancelled;
        btnSaveUpdate.Enabled = !_saving && pass != null;
        btnCancelPass.Enabled = !_saving && pass != null && pass.Status != PassStatus.Cancelled;
        UpdateRenewPreview();
    }

    private async void UpdateRenewPreview()
    {
        int version = ++_previewVersion;
        var pass = SelectedPass;
        if (pass == null)
        {
            lblNewEndVal.Text = "—";
            lblRenewFeeVal.Text = "—";
            return;
        }
        lblNewEndVal.Text = Fmt.Date(MonthlyPassService.GetRenewEndDate(pass.EndDate, Months));
        try
        {
            decimal fee = await _service.GetFeeAsync(pass.VehicleTypeId, Months);
            if (!IsDisposed && version == _previewVersion) lblRenewFeeVal.Text = Fmt.Vnd(fee);
        }
        catch (Exception)
        {
            if (!IsDisposed && version == _previewVersion) lblRenewFeeVal.Text = "Chưa có bảng giá";
        }
    }

    private async Task SaveAsync(Func<Task> action, string message)
    {
        if (_saving || SelectedPass == null) return;
        _saving = true;
        btnConfirmRenew.Enabled = btnSaveUpdate.Enabled = btnCancelPass.Enabled = false;
        try
        {
            await Dialogs.RunAsync(async () =>
            {
                await action();
                await RefreshPassesAsync();
                MessageBox.Show(message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }
        finally
        {
            _saving = false;
            if (!IsDisposed) DgvPasses_SelectionChanged(null, EventArgs.Empty);
        }
    }

    private async void BtnConfirmRenew_Click(object? sender, EventArgs e)
    {
        var pass = SelectedPass;
        if (pass == null) return;
        int months = Months;
        await SaveAsync(async () => await _service.RenewAsync(pass.PassId, months), "Đã gia hạn vé tháng và ghi nhận tiền thu.");
    }

    private async void BtnSaveUpdate_Click(object? sender, EventArgs e)
    {
        var pass = SelectedPass;
        if (pass == null) return;
        string phone = txtUpdPhone.Text;
        string brand = txtUpdBrand.Text;
        await SaveAsync(() => _service.UpdateInfoAsync(pass.PassId, phone, brand), "Đã cập nhật thông tin vé tháng.");
    }

    // Hàm xác nhận trước khi hủy vĩnh viễn một vé tháng đang chọn
    private async void BtnCancelPass_Click(object? sender, EventArgs e)
    {
        MonthlyPassView? pass = SelectedPass;
        if (pass == null) return;

        DialogResult confirm = MessageBox.Show(
            $"Bạn có chắc chắn muốn hủy vé tháng {pass.Code} ({pass.LicensePlate}) không?",
            "Xác nhận hủy vé",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );
        if (confirm != DialogResult.Yes) return;

        await SaveAsync(() => _service.CancelAsync(pass.PassId), "Đã hủy vé tháng.");
    }
}
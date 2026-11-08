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

    private void SetupGrid()
    {
        dgvPasses.Columns.Add("Code", "Mã Vé");
        dgvPasses.Columns.Add("Plate", "Biển số");
        dgvPasses.Columns.Add("Owner", "Chủ xe");
        dgvPasses.Columns.Add("EndDate", "Hạn hiện tại");
        dgvPasses.Columns.Add("Status", "Trạng thái");

        dgvPasses.Rows.Add("MP-01", "51H-999.99", "Trần Minh Hoàng", "01/11/2026", "Sắp hết hạn (3 ngày)");
        dgvPasses.Rows.Add("MP-02", "59P2-345.67", "Nguyễn Thị Lan", "15/12/2026", "Còn hạn");
        dgvPasses.Rows.Add("MP-03", "59T1-882.10", "Lê Quốc Bảo", "01/10/2026", "Đã hết hạn");
    }

    private void DgvPasses_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvPasses.CurrentRow == null) return;
        string code = dgvPasses.CurrentRow.Cells[0].Value?.ToString() ?? "MP-01";
        string plate = dgvPasses.CurrentRow.Cells[1].Value?.ToString() ?? "";
        string owner = dgvPasses.CurrentRow.Cells[2].Value?.ToString() ?? "";
        string oldEnd = dgvPasses.CurrentRow.Cells[3].Value?.ToString() ?? "01/11/2026";

        lblSelPassCode.Text = $"{code} ({plate})";
        lblSelOwner.Text = owner;
        lblSelOldEnd.Text = oldEnd;
        UpdateRenewPreview();
    }

    private void UpdateRenewPreview()
    {
        int months = cboRenewMonths.SelectedIndex switch { 1 => 3, 2 => 6, _ => 1 };
        bool isCar = lblSelPassCode.Text.Contains("51H");
        decimal unit = isCar ? 1200000m : 150000m;
        lblRenewFeeVal.Text = $"{unit * months:N0} VNĐ";

        if (DateTime.TryParseExact(lblSelOldEnd.Text, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var dt))
        {
            lblNewEndVal.Text = dt.AddMonths(months).ToString("dd/MM/yyyy");
        }
    }

    private void BtnConfirmRenew_Click(object? sender, EventArgs e)
    {
        if (dgvPasses.CurrentRow != null)
        {
            dgvPasses.CurrentRow.Cells[3].Value = lblNewEndVal.Text;
            dgvPasses.CurrentRow.Cells[4].Value = "Còn hạn (Đã gia hạn)";
            lblSelOldEnd.Text = lblNewEndVal.Text;
        }
        MessageBox.Show($"Đã gia hạn thành công đến ngày {lblNewEndVal.Text}!", "Gia hạn Vé tháng", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}

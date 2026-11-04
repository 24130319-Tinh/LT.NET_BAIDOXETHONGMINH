namespace BAIDOXETHONGMINH.GUI;

public partial class SearchReportForm : Form
{
    public SearchReportForm()
    {
        InitializeComponent();

        RoundedUiHelper.AttachRoundedFrame(pnlKpi1, 14, RoundedUiHelper.CardBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlKpi2, 14, RoundedUiHelper.CardBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlKpi3, 14, RoundedUiHelper.CardBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlKpi4, 14, RoundedUiHelper.CardBorder);

        RoundedUiHelper.AttachRoundedFrame(pnlFilterCard, 14, RoundedUiHelper.CardBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlSearchInputBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlShiftComboBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(btnSearch, 10, Color.FromArgb(37, 99, 235));
        RoundedUiHelper.AttachRoundedFrame(btnExportCsv, 10, RoundedUiHelper.SlateBorder);

        RoundedUiHelper.AttachRoundedFrame(pnlGridCard, 16, RoundedUiHelper.CardBorder);

        SetupHistoryGrid();
    }

    private void SetupHistoryGrid()
    {
        dgvHistory.Columns.Add("CardCode", "Mã Thẻ");
        dgvHistory.Columns.Add("LicensePlate", "Biển số");
        dgvHistory.Columns.Add("TypeSlot", "Loại xe / Ô");
        dgvHistory.Columns.Add("TimeIn", "Giờ vào");
        dgvHistory.Columns.Add("TimeOut", "Giờ ra");
        dgvHistory.Columns.Add("Note", "Ghi chú / Hình thức");
        dgvHistory.Columns.Add("Total", "Tổng tiền");

        LoadDefaultRows();
    }

    private void LoadDefaultRows()
    {
        dgvHistory.Rows.Clear();
        dgvHistory.Rows.Add("THE002", "51G-456.78", "Ô tô (B-02)", "29/10 07:15", "Đang gửi", "Vé lượt", "—");
        dgvHistory.Rows.Add("THE004", "59C1-778.12", "Xe máy (A-03)", "29/10 06:40", "29/10 09:10", "Mất thẻ (CCCD: 079204...)", "55.000 đ");
        dgvHistory.Rows.Add("THE003", "51H-999.99", "Ô tô (B-01)", "29/10 07:00", "29/10 08:30", "Vé tháng hợp lệ", "0 đ");
        dgvHistory.Rows.Add("THE001", "59X3-112.23", "Xe máy (A-01)", "29/10 06:15", "29/10 08:00", "Vé lượt ban ngày", "5.000 đ");
    }

    private void BtnSearch_Click(object? sender, EventArgs e)
    {
        string kw = txtSearchPlate.Text.Trim().ToUpperInvariant();
        LoadDefaultRows();
        if (string.IsNullOrEmpty(kw)) return;

        for (int i = dgvHistory.Rows.Count - 1; i >= 0; i--)
        {
            string card = dgvHistory.Rows[i].Cells[0].Value?.ToString() ?? "";
            string plate = dgvHistory.Rows[i].Cells[1].Value?.ToString() ?? "";
            if (!card.Contains(kw, StringComparison.OrdinalIgnoreCase) &&
                !plate.Contains(kw, StringComparison.OrdinalIgnoreCase))
            {
                dgvHistory.Rows.RemoveAt(i);
            }
        }
    }

    private void BtnExportCsv_Click(object? sender, EventArgs e)
    {
        MessageBox.Show("Đã kết xuất báo cáo lịch sử & doanh thu ra file Excel (.csv) thành công!", "Xuất báo cáo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}

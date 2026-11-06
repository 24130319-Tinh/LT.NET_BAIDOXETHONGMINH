namespace BAIDOXETHONGMINH.GUI;

public partial class CustomerForm : Form
{
    public CustomerForm()
    {
        InitializeComponent();

        RoundedUiHelper.AttachRoundedFrame(pnlLeftForm, 16, RoundedUiHelper.CardBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlRightGrid, 16, RoundedUiHelper.CardBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlFullNameBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlPhoneBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlCccdBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(pnlSearchBox, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(btnAddCustomer, 10, Color.FromArgb(37, 99, 235));
        RoundedUiHelper.AttachRoundedFrame(btnUpdateCustomer, 10, RoundedUiHelper.SlateBorder);
        RoundedUiHelper.AttachRoundedFrame(btnDeleteCustomer, 10, Color.FromArgb(248, 113, 113));

        SetupCustomerGrid();
    }

    private void SetupCustomerGrid()
    {
        dgvCustomers.Columns.Add("CustomerId", "Mã KH");
        dgvCustomers.Columns.Add("FullName", "Họ và tên");
        dgvCustomers.Columns.Add("Phone", "Số điện thoại");
        dgvCustomers.Columns.Add("IdentityCard", "Số CCCD");
        dgvCustomers.Columns.Add("CreatedAt", "Ngày đăng ký");

        dgvCustomers.Rows.Add("KH-01", "Trần Minh Hoàng", "0908123456", "079204001234", "01/10/2026");
        dgvCustomers.Rows.Add("KH-02", "Nguyễn Thị Lan", "0912345678", "079204005678", "15/10/2026");
        dgvCustomers.Rows.Add("KH-03", "Lê Quốc Bảo", "0987654321", "079204009999", "01/09/2026");
    }

    private void DgvCustomers_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvCustomers.CurrentRow == null) return;
        txtFullName.Text = dgvCustomers.CurrentRow.Cells[1].Value?.ToString() ?? "";
        txtPhone.Text = dgvCustomers.CurrentRow.Cells[2].Value?.ToString() ?? "";
        txtCccd.Text = dgvCustomers.CurrentRow.Cells[3].Value?.ToString() ?? "";
    }

    private void BtnAddCustomer_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtPhone.Text))
        {
            MessageBox.Show("Vui lòng nhập Họ tên và Số điện thoại khách hàng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string id = $"KH-0{dgvCustomers.Rows.Count + 1}";
        dgvCustomers.Rows.Add(id, txtFullName.Text.Trim(), txtPhone.Text.Trim(), txtCccd.Text.Trim(), DateTime.Now.ToString("dd/MM/yyyy"));
        MessageBox.Show($"Đã thêm khách hàng {txtFullName.Text.Trim()} ({id}) thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnUpdateCustomer_Click(object? sender, EventArgs e)
    {
        if (dgvCustomers.CurrentRow == null) return;
        dgvCustomers.CurrentRow.Cells[1].Value = txtFullName.Text.Trim();
        dgvCustomers.CurrentRow.Cells[2].Value = txtPhone.Text.Trim();
        dgvCustomers.CurrentRow.Cells[3].Value = txtCccd.Text.Trim();
        MessageBox.Show("Đã cập nhật hồ sơ khách hàng!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnDeleteCustomer_Click(object? sender, EventArgs e)
    {
        if (dgvCustomers.CurrentRow == null) return;
        dgvCustomers.Rows.Remove(dgvCustomers.CurrentRow);
    }
}

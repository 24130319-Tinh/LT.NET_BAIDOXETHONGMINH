namespace BAIDOXETHONGMINH.GUI;
using System.IO;
using System.Text;
using System.Globalization;
using System.Linq;
using BAIDOXETHONGMINH.DTO.Entities;
using BAIDOXETHONGMINH.DAL.Repositories;
using BAIDOXETHONGMINH.DAL.Database;


public partial class SearchReportForm : Form
{
    private readonly ParkingRepository _repository = new ParkingRepository();
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
        LoadParkingStatistics();
        LoadShiftRevenueStatistics();
    }

    private void LoadDefaultRows()
{
    LoadParkingHistory("");
}

private void LoadParkingHistory(string keyword)
{
    try
    {
        List<ParkingSession> sessions =
            _repository.getParkingHistory(keyword, "", "", "");

        dgvHistory.Rows.Clear();

        foreach (ParkingSession s in sessions)
        {
            dgvHistory.Rows.Add(
                s.CardCode,
                s.LicensePlate,
                $"{s.VehicleTypeId} ({s.SlotCode})",
                s.CheckInTime.ToString("dd/MM HH:mm"),
                s.CheckOutTime?.ToString("dd/MM HH:mm") ?? "Đang gửi",
                s.Status,
                s.TotalAmount.ToString("N0") + " đ"
            );
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Không thể tải lịch sử xe:\n" + ex.Message,
            "Lỗi dữ liệu",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );
    }
}

private void LoadParkingStatistics()
{
    try
    {
        using var conn = SqliteDbContext.GetConnection();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            SELECT
                COUNT(*) AS TotalParking,
                COALESCE(SUM(CASE
                    WHEN VehicleTypeId = 'XEMAY' THEN 1
                    ELSE 0 END), 0) AS Motorbikes,
                COALESCE(SUM(CASE
                    WHEN VehicleTypeId = 'OTO' THEN 1
                    ELSE 0 END), 0) AS Cars
            FROM ParkingSessions
            WHERE Status = 'Parking'
              AND CheckOutTime IS NULL;
        ";

        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            long total = reader.GetInt64(0);
            long motorbikes = reader.GetInt64(1);
            long cars = reader.GetInt64(2);

            // Lấy hai Label hiển thị số liệu trong KPI đầu tiên
            var labels = pnlKpi1.Controls
                .OfType<Label>()
                .ToList();

            if (labels.Count >= 3)
            {
                labels[1].Text = $"{total} xe";
                labels[2].Text =
                    $"{motorbikes} Xe máy • {cars} Ô tô";
            }
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Không thể tải thống kê xe:\n" + ex.Message,
            "Lỗi thống kê",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );
    }
}

//NV6: Thống kê doanh thu ca trực
private void LoadShiftRevenueStatistics()
{
    try
    {
        using var conn = SqliteDbContext.GetConnection();
        using var cmd = conn.CreateCommand();

        // Lấy ca đang mở mới nhất
        cmd.CommandText = @"
            SELECT ShiftId, ShiftName
            FROM WorkShifts
            WHERE Status = 'Open'
            ORDER BY ShiftId DESC
            LIMIT 1;
        ";

        using var reader = cmd.ExecuteReader();

        long shiftId;
        string shiftName;

        if (reader.Read())
        {
            shiftId = reader.GetInt64(0);
            shiftName = reader.GetString(1);
        }
        else
        {
            UpdateRevenueKpi("Chưa có ca", "0 đ", "0 lượt xe ra");
            return;
        }

        reader.Close();

        // Lấy báo cáo doanh thu từ Repository
        var report = _repository.GetShiftRevenueReport(shiftId);

        decimal revenue = Convert.ToDecimal(report["RevenueNormal"]);
        int vehiclesOut = Convert.ToInt32(report["TotalVehiclesOut"]);

        UpdateRevenueKpi(
            $"Doanh thu Vé lượt ({shiftName})",
            $"{revenue:N0} đ",
            $"{vehiclesOut} lượt xe ra"
        );
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Không thể tải doanh thu ca trực:\n" + ex.Message,
            "Lỗi thống kê",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );
    }
}

private void UpdateRevenueKpi(
    string caption,
    string revenue,
    string vehicleCount)
{
    var labels = pnlKpi2.Controls
        .OfType<Label>()
        .ToList();

    if (labels.Count < 3)
        return;

    // Xác định Label theo vị trí, không phụ thuộc thứ tự Controls
    Label? titleLabel = labels.FirstOrDefault(l => l.Top == 10);
    Label? valueLabel = labels.FirstOrDefault(l => l.Top == 34);
    Label? countLabel = labels.FirstOrDefault(l => l.Top == 70);

    if (titleLabel != null) titleLabel.Text = caption;
    if (valueLabel != null) valueLabel.Text = revenue;
    if (countLabel != null) countLabel.Text = vehicleCount;
}




   private void BtnSearch_Click(object? sender, EventArgs e)
{
    string keyword = txtSearchPlate.Text.Trim();

    LoadParkingHistory(keyword);
    LoadParkingStatistics();
    LoadShiftRevenueStatistics();
}

    
private void BtnExportCsv_Click(object? sender, EventArgs e)
{
    try
    {
        // 1. Lấy danh sách mã ca trực từ SQLite
        var shiftIds = new List<long>();

        using (var conn = SqliteDbContext.GetConnection())
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT ShiftId FROM WorkShifts ORDER BY ShiftId DESC";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                shiftIds.Add(reader.GetInt64(0));
            }
        }

        if (shiftIds.Count == 0)
        {
            MessageBox.Show(
                "Chưa có ca trực nào để xuất báo cáo.",
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
            return;
        }

        // 2. Lấy doanh thu thực tế từ Repository
        var reports = shiftIds
            .Select(id => _repository.GetShiftRevenueReport(id))
            .Where(r => r.Count > 0)
            .ToList();

        // 3. Tạo nội dung CSV
        var csv = new StringBuilder();

        csv.AppendLine(
            "Ma ca,Ten ca,Bat dau,Ket thuc,Trang thai," +
            "So xe ra,Doanh thu ve luot,Tien phat mat the,Tong doanh thu"
        );

        string EscapeCsv(object? value)
        {
            string text = Convert.ToString(
                value,
                CultureInfo.InvariantCulture
            ) ?? "";

            // Tránh Excel diễn giải dữ liệu thành công thức
            if (text.Length > 0 &&
                "=+-@\t\r".Contains(text[0]))
            {
                text = "'" + text;
            }

            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }

        foreach (var r in reports)
        {
            object[] fields =
            {
                r["ShiftId"],
                r["ShiftName"],
                r["StartTime"],
                r["EndTime"],
                r["Status"],
                r["TotalVehiclesOut"],
                r["RevenueNormal"],
                r["RevenueLostCard"],
                r["TotalRevenue"]
            };

            csv.AppendLine(string.Join(",", fields.Select(EscapeCsv)));
        }

        // 4. Chọn vị trí lưu file
        using var dialog = new SaveFileDialog
        {
            Title = "Xuất báo cáo doanh thu",
            Filter = "CSV Files (*.csv)|*.csv",
            FileName = $"BaoCaoDoanhThu_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
            DefaultExt = "csv",
            AddExtension = true
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        // 5. Ghi file UTF-8 để Excel đọc tiếng Việt
        File.WriteAllText(
            dialog.FileName,
            csv.ToString(),
            new UTF8Encoding(true)
        );

        MessageBox.Show(
            $"Đã xuất báo cáo {reports.Count} ca trực!\n\n" +
            $"File: {dialog.FileName}",
            "Xuất báo cáo thành công",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Không thể xuất báo cáo:\n" + ex.Message,
            "Lỗi xuất CSV",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );
    }
}


 // NV6: Kiểm tra nút Kết thúc ca
private void BtnCloseShift_Click(object? sender, EventArgs e)
{
    try
    {
        // 1. Tìm ca trực đang mở
        long shiftId;

        using (var conn = SqliteDbContext.GetConnection())
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                SELECT ShiftId
                FROM WorkShifts
                WHERE Status = 'Open'
                ORDER BY ShiftId DESC
                LIMIT 1;
            ";

            object? result = cmd.ExecuteScalar();

            if (result == null || result == DBNull.Value)
            {
                MessageBox.Show(
                    "Hiện không có ca trực nào đang mở.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            shiftId = Convert.ToInt64(result);
        }

        // 2. Lấy doanh thu thực tế của ca
        var report = _repository.GetShiftRevenueReport(shiftId);

        if (report.Count == 0)
        {
            MessageBox.Show("Không tìm thấy báo cáo ca trực.");
            return;
        }

        decimal totalRevenue =
            Convert.ToDecimal(report["TotalRevenue"]);

        string shiftName =
            Convert.ToString(report["ShiftName"]) ?? "";

        int vehiclesOut =
            Convert.ToInt32(report["TotalVehiclesOut"]);

        // 3. Xác nhận trước khi đóng ca
        DialogResult confirm = MessageBox.Show(
            $"Ca trực: {shiftName}\n" +
            $"Số lượt xe ra: {vehiclesOut}\n" +
            $"Tổng doanh thu: {totalRevenue:N0} đ\n\n" +
            "Bạn có chắc chắn muốn kết thúc ca trực?",
            "Xác nhận kết thúc ca",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2
        );

        if (confirm != DialogResult.Yes)
            return;

        // 4. Gọi Repository cập nhật SQLite
        bool success = _repository.CloseShift(
            shiftId,
            totalRevenue
        );

        if (!success)
        {
            MessageBox.Show(
                "Không thể kết thúc ca. Ca có thể đã được đóng.",
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return;
        }

        // 5. Thông báo và làm mới dữ liệu
        MessageBox.Show(
            $"Đã kết thúc {shiftName} thành công!\n\n" +
            $"Tổng doanh thu: {totalRevenue:N0} đ",
            "Kết thúc ca",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );

        LoadParkingHistory(txtSearchPlate.Text.Trim());
        LoadParkingStatistics();
        LoadShiftRevenueStatistics();
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Lỗi kết thúc ca:\n" + ex.Message,
            "Lỗi",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );
    }
}
}


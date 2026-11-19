using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using BAIDOXETHONGMINH.DAL.Database;
using BAIDOXETHONGMINH.DAL.Repositories;
using BAIDOXETHONGMINH.DTO.Entities;
using Microsoft.Data.Sqlite;

namespace BAIDOXETHONGMINH.GUI;

public partial class SearchReportForm : Form
{
    private readonly ParkingRepository _repository = new ParkingRepository();

    public event Action? LogoutRequested;

    // Hàm khởi tạo SearchReportForm, thiết lập bo góc và nạp dữ liệu lịch sử cùng 4 thẻ thống kê KPI
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

        RoundedUiHelper.AttachRoundedFrame(pnlShiftActions, 14, RoundedUiHelper.CardBorder);
        RoundedUiHelper.AttachRoundedFrame(btnCloseShift, 10, Color.FromArgb(220, 38, 38));

        RoundedUiHelper.AttachRoundedFrame(pnlGridCard, 16, RoundedUiHelper.CardBorder);

        SetupHistoryGrid();
    }

    // Hàm cấu hình các cột cho bảng lịch sử ra vào và tải dữ liệu thống kê ban đầu
    private void SetupHistoryGrid()
    {
        dgvHistory.Columns.Add("CardCode", "Mã Thẻ");
        dgvHistory.Columns.Add("LicensePlate", "Biển số");
        dgvHistory.Columns.Add("TypeSlot", "Loại xe / Ô");
        dgvHistory.Columns.Add("TimeIn", "Giờ vào");
        dgvHistory.Columns.Add("TimeOut", "Giờ ra");
        dgvHistory.Columns.Add("Note", "Ghi chú / Hình thức");
        dgvHistory.Columns.Add("Total", "Tổng tiền");

        RefreshAllStatisticsAndHistory();
    }

    // Hàm làm mới toàn bộ bảng lịch sử và 4 thẻ KPI trên màn hình
    private void RefreshAllStatisticsAndHistory()
    {
        string keyword = txtSearchPlate.Text.Trim();
        LoadParkingHistory(keyword);
        LoadParkingStatistics();
        LoadShiftRevenueStatistics();
        LoadMonthlyPassAndIncidentStatistics();
    }

    // Hàm tải danh sách lịch sử xe ra vào từ SQLite, lọc theo ca trực và hiển thị tiếng Việt rõ ràng
    private void LoadParkingHistory(string keyword)
    {
        try
        {
            List<ParkingSession> sessions = _repository.getParkingHistory(keyword, "", "", "");
            Dictionary<int, string> lostCardNotes = _repository.GetLostCardNotesBySessionId();
            int selectedShiftIndex = cboShiftFilter.SelectedIndex;

            dgvHistory.Rows.Clear();

            foreach (ParkingSession s in sessions)
            {
                if (selectedShiftIndex > 0 && !MatchesShiftWindow(s, selectedShiftIndex))
                {
                    continue;
                }

                string vehicleTypeName = string.Equals(s.VehicleTypeId, "OTO", StringComparison.OrdinalIgnoreCase)
                    ? "Ô tô"
                    : "Xe máy";

                string timeOutDisplay = s.CheckOutTime.HasValue
                    ? s.CheckOutTime.Value.ToString("dd/MM HH:mm")
                    : "Đang gửi";

                string noteDisplay;
                if (lostCardNotes.TryGetValue(s.SessionId, out string? incidentDetail) &&
                    !string.IsNullOrWhiteSpace(incidentDetail))
                {
                    noteDisplay = incidentDetail;
                }
                else if (string.Equals(s.Status, "LostCard", StringComparison.OrdinalIgnoreCase) || s.PenaltyFee > 0m)
                {
                    noteDisplay = "Mất thẻ";
                }
                else if (s.IsMonthlyPass)
                {
                    noteDisplay = "Vé tháng hợp lệ";
                }
                else
                {
                    noteDisplay = "Vé lượt";
                }

                string totalDisplay = string.Equals(s.Status, "Parking", StringComparison.OrdinalIgnoreCase)
                    ? "—"
                    : $"{s.TotalAmount:N0} đ";

                dgvHistory.Rows.Add(
                    s.CardCode,
                    s.LicensePlate,
                    $"{vehicleTypeName} ({s.SlotCode})",
                    s.CheckInTime.ToString("dd/MM HH:mm"),
                    timeOutDisplay,
                    noteDisplay,
                    totalDisplay
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

    // Hàm kiểm tra một lượt gửi xe có thuộc khung giờ của ca trực đang chọn trên bộ lọc hay không
    private static bool MatchesShiftWindow(ParkingSession session, int shiftFilterIndex)
    {
        DateTime referenceTime = session.CheckOutTime ?? session.CheckInTime;
        int hour = referenceTime.Hour;

        if (shiftFilterIndex == 1)
        {
            return hour >= 6 && hour < 14;
        }

        if (shiftFilterIndex == 2)
        {
            return hour >= 14 && hour < 22;
        }

        if (shiftFilterIndex == 3)
        {
            return hour >= 22 || hour < 6;
        }

        return true;
    }

    // Hàm thống kê số lượng xe đang gửi trong bãi trên tổng sức chứa 96 ô và cập nhật thẻ KPI 1
    private void LoadParkingStatistics()
    {
        try
        {
            using SqliteConnection conn = SqliteDbContext.GetConnection();
            using SqliteCommand cmd = conn.CreateCommand();

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

            using SqliteDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                long total = reader.GetInt64(0);
                long motorbikes = reader.GetInt64(1);
                long cars = reader.GetInt64(2);

                (int AvailableSlots, int TotalSlots) capacity = _repository.GetSlotCapacitySummary();
                UpdateKpiCard(
                    pnlKpi1,
                    "Xe đang gửi trong bãi",
                    $"{total} / {capacity.TotalSlots} xe",
                    $"{motorbikes} Xe máy • {cars} Ô tô"
                );
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

    // Hàm thống kê doanh thu vé lượt của ca trực đang mở hoặc ca được chọn và cập nhật thẻ KPI 2
    private void LoadShiftRevenueStatistics()
    {
        try
        {
            using SqliteConnection conn = SqliteDbContext.GetConnection();

            long openShiftId = 0;
            string openShiftName = "Chưa mở ca";

            using (SqliteCommand openCmd = conn.CreateCommand())
            {
                openCmd.CommandText = @"
                    SELECT ShiftId, ShiftName
                    FROM WorkShifts
                    WHERE Status = 'Open'
                    ORDER BY ShiftId DESC
                    LIMIT 1;";

                using SqliteDataReader openReader = openCmd.ExecuteReader();
                if (openReader.Read())
                {
                    openShiftId = openReader.GetInt64(0);
                    openShiftName = openReader.GetString(1);
                }
            }

            if (openShiftId > 0)
            {
                Dictionary<string, object> openReport = _repository.GetShiftRevenueReport(openShiftId);
                decimal openTotalRev = openReport.ContainsKey("TotalRevenue")
                    ? Convert.ToDecimal(openReport["TotalRevenue"])
                    : 0m;
                int openVehiclesOut = openReport.ContainsKey("TotalVehiclesOut")
                    ? Convert.ToInt32(openReport["TotalVehiclesOut"])
                    : 0;

                lblShiftBanner.Text =
                    $"Ca trực đang mở: {openShiftName} (Mã ca #{openShiftId})  •  Xe đã ra: {openVehiclesOut} lượt  •  Tổng thu ca: {openTotalRev:N0} đ";
            }
            else
            {
                lblShiftBanner.Text = "Hiện không có ca trực nào đang mở.";
            }

            long targetShiftId = openShiftId;
            string targetShiftName = openShiftName;

            if (cboShiftFilter.SelectedIndex > 0)
            {
                string filterShiftName = ParkingRepository.NormalizeShiftShortName(
                    cboShiftFilter.SelectedItem?.ToString() ?? "Ca Sáng");

                using SqliteCommand filterCmd = conn.CreateCommand();
                filterCmd.CommandText = @"
                    SELECT ShiftId, ShiftName
                    FROM WorkShifts
                    WHERE ShiftName LIKE @ShiftPrefix || '%'
                    ORDER BY CASE WHEN Status = 'Open' THEN 0 ELSE 1 END, ShiftId DESC
                    LIMIT 1;";
                filterCmd.Parameters.AddWithValue("@ShiftPrefix", filterShiftName);

                using SqliteDataReader filterReader = filterCmd.ExecuteReader();
                if (filterReader.Read())
                {
                    targetShiftId = filterReader.GetInt64(0);
                    targetShiftName = filterReader.GetString(1);
                }
                else
                {
                    UpdateKpiCard(
                        pnlKpi2,
                        $"Doanh thu Vé lượt ({filterShiftName})",
                        "0 đ",
                        "0 lượt xe ra"
                    );
                    return;
                }
            }

            if (targetShiftId <= 0)
            {
                UpdateKpiCard(pnlKpi2, "Doanh thu Vé lượt (Chưa có ca)", "0 đ", "0 lượt xe ra");
                return;
            }

            Dictionary<string, object> report = _repository.GetShiftRevenueReport(targetShiftId);
            decimal revenue = report.ContainsKey("RevenueNormal")
                ? Convert.ToDecimal(report["RevenueNormal"])
                : 0m;
            int vehiclesOut = report.ContainsKey("TotalVehiclesOut")
                ? Convert.ToInt32(report["TotalVehiclesOut"])
                : 0;

            UpdateKpiCard(
                pnlKpi2,
                $"Doanh thu Vé lượt ({targetShiftName})",
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

    // Hàm tải số liệu thực tế cho thẻ KPI 3 (Doanh thu vé tháng hôm nay) và thẻ KPI 4 (Sự cố mất thẻ hôm nay)
    private void LoadMonthlyPassAndIncidentStatistics()
    {
        try
        {
            (decimal TotalRevenue, int NewPassCount, int RenewPassCount) monthly =
                _repository.GetTodayMonthlyPassRevenueSummary();

            UpdateKpiCard(
                pnlKpi3,
                "Doanh thu Vé tháng hôm nay",
                $"{monthly.TotalRevenue:N0} đ",
                $"{monthly.NewPassCount} đăng ký • {monthly.RenewPassCount} gia hạn"
            );

            (int IncidentCount, decimal TotalCompensation) incident =
                _repository.GetTodayLostCardIncidentSummary();

            UpdateKpiCard(
                pnlKpi4,
                "Sự cố mất thẻ hôm nay",
                $"{incident.IncidentCount} biên bản",
                $"Thu phạt: {incident.TotalCompensation:N0} đ"
            );
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Không thể tải thống kê vé tháng và sự cố:\n" + ex.Message,
                "Lỗi thống kê",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    // Hàm cập nhật tiêu đề, giá trị chính và dòng mô tả phụ cho một thẻ KPI bất kỳ theo tọa độ Label
    private static void UpdateKpiCard(Panel cardPanel, string caption, string bigValue, string subText)
    {
        List<Label> labels = cardPanel.Controls.OfType<Label>().ToList();
        if (labels.Count < 3)
        {
            return;
        }

        Label? titleLabel = labels.FirstOrDefault(l => l.Top == 10);
        Label? valueLabel = labels.FirstOrDefault(l => l.Top == 34);
        Label? subLabel = labels.FirstOrDefault(l => l.Top == 70);

        if (titleLabel != null)
        {
            titleLabel.Text = caption;
        }

        if (valueLabel != null)
        {
            valueLabel.Text = bigValue;
        }

        if (subLabel != null)
        {
            subLabel.Text = subText;
        }
    }

    // Hàm xử lý sự kiện khi người dùng nhấn phím Enter tại ô tìm kiếm biển số
    private void TxtSearchPlate_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            RefreshAllStatisticsAndHistory();
        }
    }

    // Hàm xử lý sự kiện khi người dùng thay đổi lựa chọn Ca trực trên ComboBox lọc
    private void CboShiftFilter_SelectedIndexChanged(object? sender, EventArgs e)
    {
        RefreshAllStatisticsAndHistory();
    }

    // Hàm xử lý sự kiện khi người dùng nhấn nút Tìm kiếm
    private void BtnSearch_Click(object? sender, EventArgs e)
    {
        RefreshAllStatisticsAndHistory();
    }

    // Hàm xử lý sự kiện khi người dùng nhấn nút Xuất Excel (.csv)
    private void BtnExportCsv_Click(object? sender, EventArgs e)
    {
        ExportShiftReportsToCsv();
    }

    // Hàm kết xuất báo cáo doanh thu tất cả ca trực ra file CSV chuẩn UTF-8 để mở bằng Excel
    private bool ExportShiftReportsToCsv()
    {
        try
        {
            List<long> shiftIds = new List<long>();

            using (SqliteConnection conn = SqliteDbContext.GetConnection())
            using (SqliteCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT ShiftId FROM WorkShifts ORDER BY ShiftId DESC";

                using SqliteDataReader reader = cmd.ExecuteReader();
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
                return false;
            }

            List<Dictionary<string, object>> reports = shiftIds
                .Select(id => _repository.GetShiftRevenueReport(id))
                .Where(r => r.Count > 0)
                .ToList();

            StringBuilder csv = new StringBuilder();
            csv.AppendLine(
                "Ma ca,Ten ca,Bat dau,Ket thuc,Trang thai," +
                "So xe ra,Doanh thu ve luot,Tien phat mat the,Tong doanh thu"
            );

            foreach (Dictionary<string, object> r in reports)
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

                csv.AppendLine(string.Join(",", fields.Select(EscapeCsvField)));
            }

            using SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Xuất báo cáo doanh thu ca trực ra file Excel (.csv)",
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"BaoCaoDoanhThu_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                DefaultExt = "csv",
                AddExtension = true
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return false;
            }

            File.WriteAllText(
                dialog.FileName,
                csv.ToString(),
                new UTF8Encoding(true)
            );

            MessageBox.Show(
                $"Đã xuất báo cáo {reports.Count} ca trực thành công!\n\nFile: {dialog.FileName}",
                "Xuất báo cáo Excel (.csv)",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Không thể xuất báo cáo:\n" + ex.Message,
                "Lỗi xuất CSV",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return false;
        }
    }

    // Hàm bọc chuỗi an toàn cho từng ô dữ liệu CSV để tránh lỗi công thức khi mở trong Excel
    private static string EscapeCsvField(object? value)
    {
        string text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
        if (text.Length > 0 && "=+-@\t\r".Contains(text[0]))
        {
            text = "'" + text;
        }

        return "\"" + text.Replace("\"", "\"\"") + "\"";
    }

    // Hàm xử lý khi nhấn Kết thúc ca: hỏi người dùng có xuất file Excel không, tổng kết ca trong CSDL rồi đăng xuất
    private void BtnCloseShift_Click(object? sender, EventArgs e)
    {
        try
        {
            long shiftId;

            using (SqliteConnection conn = SqliteDbContext.GetConnection())
            using (SqliteCommand cmd = conn.CreateCommand())
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

            Dictionary<string, object> report = _repository.GetShiftRevenueReport(shiftId);
            if (report.Count == 0)
            {
                MessageBox.Show("Không tìm thấy báo cáo ca trực.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal totalRevenue = Convert.ToDecimal(report["TotalRevenue"]);
            decimal revenueNormal = Convert.ToDecimal(report["RevenueNormal"]);
            decimal revenueLostCard = Convert.ToDecimal(report["RevenueLostCard"]);
            string shiftName = Convert.ToString(report["ShiftName"]) ?? "Ca trực";
            int vehiclesOut = Convert.ToInt32(report["TotalVehiclesOut"]);

            DialogResult exportChoice = MessageBox.Show(
                $"Thông tin tổng kết {shiftName} (Mã ca #{shiftId}):\n" +
                $"• Số lượt xe ra: {vehiclesOut} lượt\n" +
                $"• Doanh thu vé lượt: {revenueNormal:N0} đ\n" +
                $"• Tiền phạt mất thẻ: {revenueLostCard:N0} đ\n" +
                $"• Tổng doanh thu ca: {totalRevenue:N0} đ\n\n" +
                "Bạn có muốn xuất báo cáo ra file Excel (.csv) trước khi tổng kết ca và đăng xuất không?\n\n" +
                "• Chọn Yes: Tổng kết ca, xuất file Excel (.csv) rồi Đăng xuất\n" +
                "• Chọn No: Tổng kết ca rồi Đăng xuất ngay\n" +
                "• Chọn Cancel: Hủy thao tác, tiếp tục làm việc",
                "Kết thúc ca & Xuất báo cáo Excel",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1
            );

            if (exportChoice == DialogResult.Cancel)
            {
                return;
            }

            bool success = _repository.CloseShift(shiftId, totalRevenue);
            if (!success)
            {
                MessageBox.Show(
                    "Không thể kết thúc ca. Ca trực có thể đã được đóng trước đó.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            if (exportChoice == DialogResult.Yes)
            {
                ExportShiftReportsToCsv();
            }

            MessageBox.Show(
                $"Đã tổng kết {shiftName} thành công!\n" +
                $"Tổng doanh thu ca: {totalRevenue:N0} đ\n\n" +
                "Hệ thống sẽ đăng xuất và quay về màn hình Đăng nhập.",
                "Tổng kết ca & Đăng xuất",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            LogoutRequested?.Invoke();
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

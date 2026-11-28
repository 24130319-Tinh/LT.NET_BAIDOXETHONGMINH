using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace BAIDOXETHONGMINH.Services;

public sealed class ShiftExcelRow
{
    public long ShiftId { get; init; }
    public string ShiftName { get; init; } = string.Empty;
    public string StaffName { get; init; } = string.Empty;
    public string StartTime { get; init; } = string.Empty;
    public string EndTime { get; init; } = string.Empty;
    public string StatusText { get; init; } = string.Empty;
    public bool IsOpen { get; init; }
    public int VehiclesOut { get; init; }
    public decimal RevenueNormal { get; init; }
    public decimal RevenueMonthly { get; init; }
    public decimal RevenueLostCard { get; init; }
    public decimal TotalRevenue { get; init; }
}

public sealed class HistoryExcelRow
{
    public int Index { get; init; }
    public string CardCode { get; init; } = string.Empty;
    public string LicensePlate { get; init; } = string.Empty;
    public string VehicleTypeName { get; init; } = string.Empty;
    public string SlotCode { get; init; } = string.Empty;
    public string CheckInTime { get; init; } = string.Empty;
    public string CheckOutTime { get; init; } = string.Empty;
    public bool IsCurrentlyParking { get; init; }
    public string NoteText { get; init; } = string.Empty;
    public bool IsLostCard { get; init; }
    public decimal PenaltyFee { get; init; }
    public decimal TotalAmount { get; init; }
}

public static class ExcelReportExporter
{
    private static readonly XNamespace NsMain = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace NsRelDoc = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace NsPkgRel = "http://schemas.openxmlformats.org/package/2006/relationships";
    private static readonly XNamespace NsContentTypes = "http://schemas.openxmlformats.org/package/2006/content-types";

    private const int StyleDefault = 0;
    private const int StyleBannerTitle = 1;
    private const int StyleBannerSub = 2;
    private const int StyleKpiLabel = 3;
    private const int StyleKpiTextValue = 4;
    private const int StyleKpiMoneyValue = 5;
    private const int StyleTableHeader = 6;
    private const int StyleTextCenterEven = 7;
    private const int StyleTextCenterOdd = 8;
    private const int StyleTextLeftEven = 9;
    private const int StyleTextLeftOdd = 10;
    private const int StyleBoldCenterEven = 11;
    private const int StyleBoldCenterOdd = 12;
    private const int StyleIntEven = 13;
    private const int StyleIntOdd = 14;
    private const int StyleMoneyEven = 15;
    private const int StyleMoneyOdd = 16;
    private const int StyleMoneyBoldEven = 17;
    private const int StyleMoneyBoldOdd = 18;
    private const int StyleStatusOpen = 19;
    private const int StyleStatusClosed = 20;
    private const int StyleStatusAlertLeft = 21;
    private const int StyleTotalLabel = 22;
    private const int StyleTotalInt = 23;
    private const int StyleTotalMoney = 24;

    // Hàm xuất báo cáo doanh thu ca trực và lịch sử xe ra vào thành file Excel .xlsx chuẩn OpenXML hoàn toàn offline
    public static void ExportWorkbook(
        string filePath,
        string exportedBy,
        string shiftFilterLabel,
        IReadOnlyList<ShiftExcelRow> shiftRows,
        IReadOnlyList<HistoryExcelRow> historyRows)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        DateTime exportedAt = DateTime.Now;
        using FileStream stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create);

        WriteXmlEntry(archive, "[Content_Types].xml", BuildContentTypesXml());
        WriteXmlEntry(archive, "_rels/.rels", BuildRootRelsXml());
        WriteXmlEntry(archive, "xl/workbook.xml", BuildWorkbookXml());
        WriteXmlEntry(archive, "xl/_rels/workbook.xml.rels", BuildWorkbookRelsXml());
        WriteXmlEntry(archive, "xl/styles.xml", BuildStylesXml());
        WriteXmlEntry(archive, "xl/worksheets/sheet1.xml", BuildShiftRevenueSheetXml(shiftRows, exportedBy, exportedAt));
        WriteXmlEntry(archive, "xl/worksheets/sheet2.xml", BuildHistorySheetXml(historyRows, shiftFilterLabel, exportedBy, exportedAt));
    }

    // Hàm ghi một tài liệu XML chuẩn UTF-8 không BOM vào gói nén .xlsx
    private static void WriteXmlEntry(ZipArchive archive, string entryPath, XDocument document)
    {
        ZipArchiveEntry entry = archive.CreateEntry(entryPath, CompressionLevel.Optimal);
        using Stream entryStream = entry.Open();
        UTF8Encoding utf8NoBom = new UTF8Encoding(false);
        using StreamWriter writer = new StreamWriter(entryStream, utf8NoBom);
        document.Save(writer, SaveOptions.DisableFormatting);
    }

    // Hàm tạo cấu trúc [Content_Types].xml khai báo các thành phần của Workbook
    private static XDocument BuildContentTypesXml()
    {
        XElement root = new XElement(NsContentTypes + "Types",
            new XElement(NsContentTypes + "Default",
                new XAttribute("Extension", "rels"),
                new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
            new XElement(NsContentTypes + "Default",
                new XAttribute("Extension", "xml"),
                new XAttribute("ContentType", "application/xml")),
            new XElement(NsContentTypes + "Override",
                new XAttribute("PartName", "/xl/workbook.xml"),
                new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml")),
            new XElement(NsContentTypes + "Override",
                new XAttribute("PartName", "/xl/styles.xml"),
                new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml")),
            new XElement(NsContentTypes + "Override",
                new XAttribute("PartName", "/xl/worksheets/sheet1.xml"),
                new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml")),
            new XElement(NsContentTypes + "Override",
                new XAttribute("PartName", "/xl/worksheets/sheet2.xml"),
                new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"))
        );

        return new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), root);
    }

    // Hàm tạo file liên kết gốc _rels/.rels trỏ tới xl/workbook.xml
    private static XDocument BuildRootRelsXml()
    {
        XElement root = new XElement(NsPkgRel + "Relationships",
            new XElement(NsPkgRel + "Relationship",
                new XAttribute("Id", "rId1"),
                new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"),
                new XAttribute("Target", "xl/workbook.xml"))
        );

        return new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), root);
    }

    // Hàm tạo xl/workbook.xml chứa danh sách 2 Sheet báo cáo tiếng Việt
    private static XDocument BuildWorkbookXml()
    {
        XElement root = new XElement(NsMain + "workbook",
            new XAttribute(XNamespace.Xmlns + "r", NsRelDoc.NamespaceName),
            new XElement(NsMain + "sheets",
                new XElement(NsMain + "sheet",
                    new XAttribute("name", "Doanh thu ca trực"),
                    new XAttribute("sheetId", "1"),
                    new XAttribute(NsRelDoc + "id", "rId1")),
                new XElement(NsMain + "sheet",
                    new XAttribute("name", "Lịch sử xe ra vào"),
                    new XAttribute("sheetId", "2"),
                    new XAttribute(NsRelDoc + "id", "rId2"))
            )
        );

        return new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), root);
    }

    // Hàm tạo xl/_rels/workbook.xml.rels liên kết 2 Sheet và bảng định dạng styles.xml
    private static XDocument BuildWorkbookRelsXml()
    {
        XElement root = new XElement(NsPkgRel + "Relationships",
            new XElement(NsPkgRel + "Relationship",
                new XAttribute("Id", "rId1"),
                new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"),
                new XAttribute("Target", "worksheets/sheet1.xml")),
            new XElement(NsPkgRel + "Relationship",
                new XAttribute("Id", "rId2"),
                new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"),
                new XAttribute("Target", "worksheets/sheet2.xml")),
            new XElement(NsPkgRel + "Relationship",
                new XAttribute("Id", "rId3"),
                new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"),
                new XAttribute("Target", "styles.xml"))
        );

        return new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), root);
    }

    // Hàm tạo bảng định dạng màu sắc, font chữ, khung viền và định dạng tiền tệ VNĐ trong xl/styles.xml
    private static XDocument BuildStylesXml()
    {
        XElement numFmts = new XElement(NsMain + "numFmts",
            new XAttribute("count", "2"),
            new XElement(NsMain + "numFmt",
                new XAttribute("numFmtId", "164"),
                new XAttribute("formatCode", "#,##0 \"đ\"")),
            new XElement(NsMain + "numFmt",
                new XAttribute("numFmtId", "165"),
                new XAttribute("formatCode", "#,##0"))
        );

        XElement fonts = new XElement(NsMain + "fonts",
            new XAttribute("count", "11"),
            CreateFont(10.5, false, false, "FF0F172A"),
            CreateFont(14.0, true, false, "FFFFFFFF"),
            CreateFont(10.0, false, true, "FFE2E8F0"),
            CreateFont(9.5, true, false, "FF475569"),
            CreateFont(12.0, true, false, "FF1E3A8A"),
            CreateFont(10.5, true, false, "FFFFFFFF"),
            CreateFont(10.5, true, false, "FF0F172A"),
            CreateFont(10.0, true, false, "FF065F46"),
            CreateFont(10.0, true, false, "FF475569"),
            CreateFont(10.0, true, false, "FFBE123C"),
            CreateFont(11.0, true, false, "FF92400E")
        );

        XElement fills = new XElement(NsMain + "fills",
            new XAttribute("count", "12"),
            new XElement(NsMain + "fill", new XElement(NsMain + "patternFill", new XAttribute("patternType", "none"))),
            new XElement(NsMain + "fill", new XElement(NsMain + "patternFill", new XAttribute("patternType", "gray125"))),
            CreateSolidFill("FF1E3A8A"),
            CreateSolidFill("FF1D4ED8"),
            CreateSolidFill("FFEFF6FF"),
            CreateSolidFill("FF1E40AF"),
            CreateSolidFill("FFFFFFFF"),
            CreateSolidFill("FFF8FAFC"),
            CreateSolidFill("FFD1FAE5"),
            CreateSolidFill("FFE2E8F0"),
            CreateSolidFill("FFFFE4E6"),
            CreateSolidFill("FFFEF3C7")
        );

        XElement borders = new XElement(NsMain + "borders",
            new XAttribute("count", "5"),
            CreateBorder(null, null, null, null),
            CreateBorder("thin:FFCBD5E1", "thin:FFCBD5E1", "thin:FFCBD5E1", "thin:FFCBD5E1"),
            CreateBorder("thin:FF1E3A8A", "thin:FF1E3A8A", "thin:FF1E3A8A", "thin:FF1E3A8A"),
            CreateBorder("thin:FF93C5FD", "thin:FF93C5FD", "thin:FF93C5FD", "thin:FF93C5FD"),
            CreateBorder("thin:FFD97706", "thin:FFD97706", "thin:FFD97706", "double:FFB45309")
        );

        XElement cellStyleXfs = new XElement(NsMain + "cellStyleXfs",
            new XAttribute("count", "1"),
            new XElement(NsMain + "xf",
                new XAttribute("numFmtId", "0"),
                new XAttribute("fontId", "0"),
                new XAttribute("fillId", "0"),
                new XAttribute("borderId", "0"))
        );

        XElement cellXfs = new XElement(NsMain + "cellXfs",
            new XAttribute("count", "25"),
            CreateCellXf(0, 0, 0, 0, "left", false),
            CreateCellXf(0, 1, 2, 0, "center", false),
            CreateCellXf(0, 2, 3, 0, "center", false),
            CreateCellXf(0, 3, 4, 3, "center", false),
            CreateCellXf(0, 4, 4, 3, "center", false),
            CreateCellXf(164, 4, 4, 3, "center", false),
            CreateCellXf(0, 5, 5, 2, "center", true),
            CreateCellXf(0, 0, 6, 1, "center", false),
            CreateCellXf(0, 0, 7, 1, "center", false),
            CreateCellXf(0, 0, 6, 1, "left", false),
            CreateCellXf(0, 0, 7, 1, "left", false),
            CreateCellXf(0, 6, 6, 1, "center", false),
            CreateCellXf(0, 6, 7, 1, "center", false),
            CreateCellXf(165, 0, 6, 1, "center", false),
            CreateCellXf(165, 0, 7, 1, "center", false),
            CreateCellXf(164, 0, 6, 1, "right", false),
            CreateCellXf(164, 0, 7, 1, "right", false),
            CreateCellXf(164, 6, 6, 1, "right", false),
            CreateCellXf(164, 6, 7, 1, "right", false),
            CreateCellXf(0, 7, 8, 1, "center", false),
            CreateCellXf(0, 8, 9, 1, "center", false),
            CreateCellXf(0, 9, 10, 1, "left", false),
            CreateCellXf(0, 10, 11, 4, "right", false),
            CreateCellXf(165, 10, 11, 4, "center", false),
            CreateCellXf(164, 10, 11, 4, "right", false)
        );

        XElement cellStyles = new XElement(NsMain + "cellStyles",
            new XAttribute("count", "1"),
            new XElement(NsMain + "cellStyle",
                new XAttribute("name", "Normal"),
                new XAttribute("xfId", "0"),
                new XAttribute("builtinId", "0"))
        );

        XElement styleSheet = new XElement(NsMain + "styleSheet",
            numFmts,
            fonts,
            fills,
            borders,
            cellStyleXfs,
            cellXfs,
            cellStyles
        );

        return new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), styleSheet);
    }

    // Hàm dựng Sheet 1: Báo cáo tổng hợp doanh thu các ca trực kèm 4 ô KPI và dòng Tổng cộng SUM
    private static XDocument BuildShiftRevenueSheetXml(
        IReadOnlyList<ShiftExcelRow> shiftRows,
        string exportedBy,
        DateTime exportedAt)
    {
        int totalCarsOut = 0;
        decimal sumNormal = 0m;
        decimal sumMonthly = 0m;
        decimal sumLostCard = 0m;
        decimal sumTotal = 0m;

        for (int i = 0; i < shiftRows.Count; i++)
        {
            totalCarsOut += shiftRows[i].VehiclesOut;
            sumNormal += shiftRows[i].RevenueNormal;
            sumMonthly += shiftRows[i].RevenueMonthly;
            sumLostCard += shiftRows[i].RevenueLostCard;
            sumTotal += shiftRows[i].TotalRevenue;
        }

        List<XElement> rows = new List<XElement>();
        List<string> merges = new List<string>();

        rows.Add(CreateMergedBannerRow(
            1, 34.0, 11, StyleBannerTitle,
            "HỆ THỐNG QUẢN LÝ BÃI ĐỖ XE THÔNG MINH — BÁO CÁO DOANH THU CA TRỰC"));
        merges.Add("A1:K1");

        string subTitle =
            $"Ngày xuất: {exportedAt:dd/MM/yyyy HH:mm:ss}   |   Người lập báo cáo: {exportedBy}   |   Số lượng ca thống kê: {shiftRows.Count} ca";
        rows.Add(CreateMergedBannerRow(2, 22.0, 11, StyleBannerSub, subTitle));
        merges.Add("A2:K2");

        rows.Add(new XElement(NsMain + "row", new XAttribute("r", "3"), new XAttribute("ht", "8"), new XAttribute("customHeight", "1")));

        XElement kpiLabelRow = new XElement(NsMain + "row",
            new XAttribute("r", "4"),
            new XAttribute("ht", "20"),
            new XAttribute("customHeight", "1"),
            CreateTextCell("A4", "TỔNG LƯỢT XE ĐÃ RA", StyleKpiLabel),
            CreateEmptyStyledCell("B4", StyleKpiLabel),
            CreateTextCell("C4", "TỔNG THU VÉ LƯỢT", StyleKpiLabel),
            CreateEmptyStyledCell("D4", StyleKpiLabel),
            CreateEmptyStyledCell("E4", StyleKpiLabel),
            CreateTextCell("F4", "TỔNG THU VÉ THÁNG", StyleKpiLabel),
            CreateEmptyStyledCell("G4", StyleKpiLabel),
            CreateEmptyStyledCell("H4", StyleKpiLabel),
            CreateTextCell("I4", "TỔNG DOANH THU CÁC CA", StyleKpiLabel),
            CreateEmptyStyledCell("J4", StyleKpiLabel),
            CreateEmptyStyledCell("K4", StyleKpiLabel)
        );
        rows.Add(kpiLabelRow);
        merges.Add("A4:B4");
        merges.Add("C4:E4");
        merges.Add("F4:H4");
        merges.Add("I4:K4");

        XElement kpiValueRow = new XElement(NsMain + "row",
            new XAttribute("r", "5"),
            new XAttribute("ht", "26"),
            new XAttribute("customHeight", "1"),
            CreateTextCell("A5", $"{totalCarsOut:N0} lượt", StyleKpiTextValue),
            CreateEmptyStyledCell("B5", StyleKpiTextValue),
            CreateNumberCell("C5", sumNormal, StyleKpiMoneyValue),
            CreateEmptyStyledCell("D5", StyleKpiMoneyValue),
            CreateEmptyStyledCell("E5", StyleKpiMoneyValue),
            CreateNumberCell("F5", sumMonthly, StyleKpiMoneyValue),
            CreateEmptyStyledCell("G5", StyleKpiMoneyValue),
            CreateEmptyStyledCell("H5", StyleKpiMoneyValue),
            CreateNumberCell("I5", sumTotal, StyleKpiMoneyValue),
            CreateEmptyStyledCell("J5", StyleKpiMoneyValue),
            CreateEmptyStyledCell("K5", StyleKpiMoneyValue)
        );
        rows.Add(kpiValueRow);
        merges.Add("A5:B5");
        merges.Add("C5:E5");
        merges.Add("F5:H5");
        merges.Add("I5:K5");

        rows.Add(new XElement(NsMain + "row", new XAttribute("r", "6"), new XAttribute("ht", "10"), new XAttribute("customHeight", "1")));

        string[] headers = new string[]
        {
            "Mã ca",
            "Tên ca trực",
            "Nhân viên trực",
            "Giờ bắt đầu",
            "Giờ kết thúc",
            "Trạng thái",
            "Xe đã ra",
            "Thu vé lượt",
            "Thu vé tháng",
            "Phạt mất thẻ",
            "Tổng doanh thu"
        };
        rows.Add(CreateHeaderRow(7, 28.0, headers));

        int startDataRow = 8;
        for (int i = 0; i < shiftRows.Count; i++)
        {
            ShiftExcelRow item = shiftRows[i];
            int r = startDataRow + i;
            bool isEven = (i % 2) == 0;

            int sCenter = isEven ? StyleTextCenterEven : StyleTextCenterOdd;
            int sLeft = isEven ? StyleTextLeftEven : StyleTextLeftOdd;
            int sBoldCenter = isEven ? StyleBoldCenterEven : StyleBoldCenterOdd;
            int sInt = isEven ? StyleIntEven : StyleIntOdd;
            int sMoney = isEven ? StyleMoneyEven : StyleMoneyOdd;
            int sMoneyBold = isEven ? StyleMoneyBoldEven : StyleMoneyBoldOdd;
            int sStatus = item.IsOpen ? StyleStatusOpen : StyleStatusClosed;

            XElement dataRow = new XElement(NsMain + "row",
                new XAttribute("r", r.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("ht", "22"),
                new XAttribute("customHeight", "1"),
                CreateTextCell($"A{r}", $"#{item.ShiftId}", sBoldCenter),
                CreateTextCell($"B{r}", item.ShiftName, sCenter),
                CreateTextCell($"C{r}", item.StaffName, sLeft),
                CreateTextCell($"D{r}", item.StartTime, sCenter),
                CreateTextCell($"E{r}", item.EndTime, sCenter),
                CreateTextCell($"F{r}", item.StatusText, sStatus),
                CreateNumberCell($"G{r}", item.VehiclesOut, sInt),
                CreateNumberCell($"H{r}", item.RevenueNormal, sMoney),
                CreateNumberCell($"I{r}", item.RevenueMonthly, sMoney),
                CreateNumberCell($"J{r}", item.RevenueLostCard, sMoney),
                CreateNumberCell($"K{r}", item.TotalRevenue, sMoneyBold)
            );
            rows.Add(dataRow);
        }

        string? autoFilterRef = null;
        if (shiftRows.Count > 0)
        {
            int lastDataRow = startDataRow + shiftRows.Count - 1;
            autoFilterRef = $"A7:K{lastDataRow}";
            int totalRowIdx = lastDataRow + 1;

            XElement totalRow = new XElement(NsMain + "row",
                new XAttribute("r", totalRowIdx.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("ht", "26"),
                new XAttribute("customHeight", "1"),
                CreateTextCell($"A{totalRowIdx}", $"TỔNG CỘNG ({shiftRows.Count} CA TRỰC)", StyleTotalLabel),
                CreateEmptyStyledCell($"B{totalRowIdx}", StyleTotalLabel),
                CreateEmptyStyledCell($"C{totalRowIdx}", StyleTotalLabel),
                CreateEmptyStyledCell($"D{totalRowIdx}", StyleTotalLabel),
                CreateEmptyStyledCell($"E{totalRowIdx}", StyleTotalLabel),
                CreateEmptyStyledCell($"F{totalRowIdx}", StyleTotalLabel),
                CreateFormulaCell($"G{totalRowIdx}", $"SUM(G{startDataRow}:G{lastDataRow})", totalCarsOut, StyleTotalInt),
                CreateFormulaCell($"H{totalRowIdx}", $"SUM(H{startDataRow}:H{lastDataRow})", sumNormal, StyleTotalMoney),
                CreateFormulaCell($"I{totalRowIdx}", $"SUM(I{startDataRow}:I{lastDataRow})", sumMonthly, StyleTotalMoney),
                CreateFormulaCell($"J{totalRowIdx}", $"SUM(J{startDataRow}:J{lastDataRow})", sumLostCard, StyleTotalMoney),
                CreateFormulaCell($"K{totalRowIdx}", $"SUM(K{startDataRow}:K{lastDataRow})", sumTotal, StyleTotalMoney)
            );
            rows.Add(totalRow);
            merges.Add($"A{totalRowIdx}:F{totalRowIdx}");
        }

        double[] colWidths = new double[] { 10, 15, 22, 20, 20, 15, 13, 17, 17, 17, 20 };
        return BuildWorksheetDocument(rows, colWidths, merges, freezeRows: 7, tabSelected: true, autoFilterRef: autoFilterRef);
    }

    // Hàm dựng Sheet 2: Bảng kê chi tiết lịch sử xe ra vào và sự cố mất thẻ
    private static XDocument BuildHistorySheetXml(
        IReadOnlyList<HistoryExcelRow> historyRows,
        string shiftFilterLabel,
        string exportedBy,
        DateTime exportedAt)
    {
        decimal sumPenalty = 0m;
        decimal sumTotalAmount = 0m;
        for (int i = 0; i < historyRows.Count; i++)
        {
            sumPenalty += historyRows[i].PenaltyFee;
            sumTotalAmount += historyRows[i].TotalAmount;
        }

        List<XElement> rows = new List<XElement>();
        List<string> merges = new List<string>();

        rows.Add(CreateMergedBannerRow(
            1, 34.0, 10, StyleBannerTitle,
            "BẢNG KÊ CHI TIẾT LỊCH SỬ XE RA / VÀO BÃI & SỰ CỐ MẤT THẺ"));
        merges.Add("A1:J1");

        string subTitle =
            $"Ngày xuất: {exportedAt:dd/MM/yyyy HH:mm:ss}   |   Người xuất: {exportedBy}   |   Bộ lọc: {shiftFilterLabel}   |   Tổng số lượt: {historyRows.Count}";
        rows.Add(CreateMergedBannerRow(2, 22.0, 10, StyleBannerSub, subTitle));
        merges.Add("A2:J2");

        rows.Add(new XElement(NsMain + "row", new XAttribute("r", "3"), new XAttribute("ht", "10"), new XAttribute("customHeight", "1")));

        string[] headers = new string[]
        {
            "STT",
            "Mã thẻ",
            "Biển số xe",
            "Loại xe",
            "Ô đỗ",
            "Giờ vào bãi",
            "Giờ ra bãi",
            "Hình thức / Ghi chú",
            "Tiền phạt",
            "Tổng thanh toán"
        };
        rows.Add(CreateHeaderRow(4, 28.0, headers));

        int startDataRow = 5;
        for (int i = 0; i < historyRows.Count; i++)
        {
            HistoryExcelRow item = historyRows[i];
            int r = startDataRow + i;
            bool isEven = (i % 2) == 0;

            int sCenter = isEven ? StyleTextCenterEven : StyleTextCenterOdd;
            int sLeft = isEven ? StyleTextLeftEven : StyleTextLeftOdd;
            int sBoldCenter = isEven ? StyleBoldCenterEven : StyleBoldCenterOdd;
            int sInt = isEven ? StyleIntEven : StyleIntOdd;
            int sMoney = isEven ? StyleMoneyEven : StyleMoneyOdd;
            int sMoneyBold = isEven ? StyleMoneyBoldEven : StyleMoneyBoldOdd;
            int sOutTime = item.IsCurrentlyParking ? StyleStatusOpen : sCenter;
            int sNote = item.IsLostCard ? StyleStatusAlertLeft : sLeft;

            XElement dataRow = new XElement(NsMain + "row",
                new XAttribute("r", r.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("ht", "22"),
                new XAttribute("customHeight", "1"),
                CreateNumberCell($"A{r}", item.Index, sInt),
                CreateTextCell($"B{r}", item.CardCode, sCenter),
                CreateTextCell($"C{r}", item.LicensePlate, sBoldCenter),
                CreateTextCell($"D{r}", item.VehicleTypeName, sCenter),
                CreateTextCell($"E{r}", item.SlotCode, sCenter),
                CreateTextCell($"F{r}", item.CheckInTime, sCenter),
                CreateTextCell($"G{r}", item.CheckOutTime, sOutTime),
                CreateTextCell($"H{r}", item.NoteText, sNote),
                CreateNumberCell($"I{r}", item.PenaltyFee, sMoney),
                CreateNumberCell($"J{r}", item.TotalAmount, sMoneyBold)
            );
            rows.Add(dataRow);
        }

        string? autoFilterRef = null;
        if (historyRows.Count > 0)
        {
            int lastDataRow = startDataRow + historyRows.Count - 1;
            autoFilterRef = $"A4:J{lastDataRow}";
            int totalRowIdx = lastDataRow + 1;

            XElement totalRow = new XElement(NsMain + "row",
                new XAttribute("r", totalRowIdx.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("ht", "26"),
                new XAttribute("customHeight", "1"),
                CreateTextCell($"A{totalRowIdx}", $"TỔNG CỘNG ({historyRows.Count} LƯỢT XE)", StyleTotalLabel),
                CreateEmptyStyledCell($"B{totalRowIdx}", StyleTotalLabel),
                CreateEmptyStyledCell($"C{totalRowIdx}", StyleTotalLabel),
                CreateEmptyStyledCell($"D{totalRowIdx}", StyleTotalLabel),
                CreateEmptyStyledCell($"E{totalRowIdx}", StyleTotalLabel),
                CreateEmptyStyledCell($"F{totalRowIdx}", StyleTotalLabel),
                CreateEmptyStyledCell($"G{totalRowIdx}", StyleTotalLabel),
                CreateEmptyStyledCell($"H{totalRowIdx}", StyleTotalLabel),
                CreateFormulaCell($"I{totalRowIdx}", $"SUM(I{startDataRow}:I{lastDataRow})", sumPenalty, StyleTotalMoney),
                CreateFormulaCell($"J{totalRowIdx}", $"SUM(J{startDataRow}:J{lastDataRow})", sumTotalAmount, StyleTotalMoney)
            );
            rows.Add(totalRow);
            merges.Add($"A{totalRowIdx}:H{totalRowIdx}");
        }

        double[] colWidths = new double[] { 8, 13, 17, 13, 10, 19, 19, 30, 16, 19 };
        return BuildWorksheetDocument(rows, colWidths, merges, freezeRows: 4, tabSelected: false, autoFilterRef: autoFilterRef);
    }

    // Hàm đóng gói toàn bộ thẻ XML của một Worksheet theo đúng thứ tự chuẩn ECMA-376 OpenXML
    private static XDocument BuildWorksheetDocument(
        IReadOnlyList<XElement> rows,
        double[] colWidths,
        IReadOnlyList<string> merges,
        int freezeRows,
        bool tabSelected,
        string? autoFilterRef)
    {
        XElement sheetPr = new XElement(NsMain + "sheetPr",
            new XElement(NsMain + "pageSetUpPr", new XAttribute("fitToPage", "1"))
        );

        XElement sheetView = new XElement(NsMain + "sheetView",
            new XAttribute("tabSelected", tabSelected ? "1" : "0"),
            new XAttribute("workbookViewId", "0"),
            new XElement(NsMain + "pane",
                new XAttribute("ySplit", freezeRows.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("topLeftCell", $"A{freezeRows + 1}"),
                new XAttribute("activePane", "bottomLeft"),
                new XAttribute("state", "frozen"))
        );

        XElement sheetViews = new XElement(NsMain + "sheetViews", sheetView);
        XElement sheetFormatPr = new XElement(NsMain + "sheetFormatPr", new XAttribute("defaultRowHeight", "20"));

        XElement cols = new XElement(NsMain + "cols");
        for (int i = 0; i < colWidths.Length; i++)
        {
            int colIdx = i + 1;
            cols.Add(new XElement(NsMain + "col",
                new XAttribute("min", colIdx.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("max", colIdx.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("width", colWidths[i].ToString("0.##", CultureInfo.InvariantCulture)),
                new XAttribute("customWidth", "1")));
        }

        XElement sheetData = new XElement(NsMain + "sheetData", rows);

        List<object> worksheetChildren = new List<object>
        {
            sheetPr,
            sheetViews,
            sheetFormatPr,
            cols,
            sheetData
        };

        if (!string.IsNullOrWhiteSpace(autoFilterRef))
        {
            worksheetChildren.Add(new XElement(NsMain + "autoFilter", new XAttribute("ref", autoFilterRef)));
        }

        if (merges.Count > 0)
        {
            XElement mergeCells = new XElement(NsMain + "mergeCells",
                new XAttribute("count", merges.Count.ToString(CultureInfo.InvariantCulture)));
            for (int i = 0; i < merges.Count; i++)
            {
                mergeCells.Add(new XElement(NsMain + "mergeCell", new XAttribute("ref", merges[i])));
            }
            worksheetChildren.Add(mergeCells);
        }

        worksheetChildren.Add(new XElement(NsMain + "pageMargins",
            new XAttribute("left", "0.4"),
            new XAttribute("right", "0.4"),
            new XAttribute("top", "0.5"),
            new XAttribute("bottom", "0.5"),
            new XAttribute("header", "0.25"),
            new XAttribute("footer", "0.25")));

        worksheetChildren.Add(new XElement(NsMain + "pageSetup",
            new XAttribute("paperSize", "9"),
            new XAttribute("orientation", "landscape"),
            new XAttribute("fitToWidth", "1"),
            new XAttribute("fitToHeight", "0")));

        XElement worksheet = new XElement(NsMain + "worksheet", worksheetChildren.ToArray());
        return new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), worksheet);
    }

    // Hàm tạo hàng tiêu đề Banner gộp ô trải dài hết chiều ngang bảng
    private static XElement CreateMergedBannerRow(int rowNumber, double heightPt, int colCount, int styleIndex, string text)
    {
        XElement row = new XElement(NsMain + "row",
            new XAttribute("r", rowNumber.ToString(CultureInfo.InvariantCulture)),
            new XAttribute("ht", heightPt.ToString("0.#", CultureInfo.InvariantCulture)),
            new XAttribute("customHeight", "1")
        );

        for (int c = 0; c < colCount; c++)
        {
            string colLetter = GetColumnLetter(c + 1);
            string cellRef = $"{colLetter}{rowNumber}";
            if (c == 0)
            {
                row.Add(CreateTextCell(cellRef, text, styleIndex));
            }
            else
            {
                row.Add(CreateEmptyStyledCell(cellRef, styleIndex));
            }
        }

        return row;
    }

    // Hàm tạo hàng Header cột nền xanh chữ trắng in đậm
    private static XElement CreateHeaderRow(int rowNumber, double heightPt, string[] titles)
    {
        XElement row = new XElement(NsMain + "row",
            new XAttribute("r", rowNumber.ToString(CultureInfo.InvariantCulture)),
            new XAttribute("ht", heightPt.ToString("0.#", CultureInfo.InvariantCulture)),
            new XAttribute("customHeight", "1")
        );

        for (int c = 0; c < titles.Length; c++)
        {
            string colLetter = GetColumnLetter(c + 1);
            row.Add(CreateTextCell($"{colLetter}{rowNumber}", titles[c], StyleTableHeader));
        }

        return row;
    }

    // Hàm tạo ô chuỗi văn bản inlineStr an toàn với tiếng Việt Unicode
    private static XElement CreateTextCell(string cellRef, string? value, int styleIndex)
    {
        string cleanText = SanitizeXmlText(value ?? string.Empty);
        XElement textEl = new XElement(NsMain + "t", cleanText);
        if (cleanText.Length > 0 && (char.IsWhiteSpace(cleanText[0]) || char.IsWhiteSpace(cleanText[^1]) || cleanText.Contains("  ")))
        {
            textEl.SetAttributeValue(XNamespace.Xml + "space", "preserve");
        }

        return new XElement(NsMain + "c",
            new XAttribute("r", cellRef),
            new XAttribute("s", styleIndex.ToString(CultureInfo.InvariantCulture)),
            new XAttribute("t", "inlineStr"),
            new XElement(NsMain + "is", textEl)
        );
    }

    // Hàm tạo ô số thực/tiền tệ chuẩn để Excel tính toán công thức trực tiếp
    private static XElement CreateNumberCell(string cellRef, decimal value, int styleIndex)
    {
        return new XElement(NsMain + "c",
            new XAttribute("r", cellRef),
            new XAttribute("s", styleIndex.ToString(CultureInfo.InvariantCulture)),
            new XElement(NsMain + "v", value.ToString(CultureInfo.InvariantCulture))
        );
    }

    // Hàm tạo ô công thức (như SUM) kèm giá trị tính sẵn cho trình xem nhanh
    private static XElement CreateFormulaCell(string cellRef, string formula, decimal cachedValue, int styleIndex)
    {
        return new XElement(NsMain + "c",
            new XAttribute("r", cellRef),
            new XAttribute("s", styleIndex.ToString(CultureInfo.InvariantCulture)),
            new XElement(NsMain + "f", formula),
            new XElement(NsMain + "v", cachedValue.ToString(CultureInfo.InvariantCulture))
        );
    }

    // Hàm tạo ô rỗng giữ nguyên khung viền và màu nền trong vùng gộp ô
    private static XElement CreateEmptyStyledCell(string cellRef, int styleIndex)
    {
        return new XElement(NsMain + "c",
            new XAttribute("r", cellRef),
            new XAttribute("s", styleIndex.ToString(CultureInfo.InvariantCulture))
        );
    }

    // Hàm tạo định nghĩa Font chữ theo chuẩn OpenXML
    private static XElement CreateFont(double sizePt, bool bold, bool italic, string argbHex)
    {
        List<object> children = new List<object>();
        if (bold)
        {
            children.Add(new XElement(NsMain + "b"));
        }
        if (italic)
        {
            children.Add(new XElement(NsMain + "i"));
        }
        children.Add(new XElement(NsMain + "sz", new XAttribute("val", sizePt.ToString("0.#", CultureInfo.InvariantCulture))));
        children.Add(new XElement(NsMain + "color", new XAttribute("rgb", argbHex)));
        children.Add(new XElement(NsMain + "name", new XAttribute("val", "Segoe UI")));
        children.Add(new XElement(NsMain + "family", new XAttribute("val", "2")));

        return new XElement(NsMain + "font", children.ToArray());
    }

    // Hàm tạo định nghĩa màu nền đặc (Solid Fill) cho ô Excel
    private static XElement CreateSolidFill(string argbHex)
    {
        return new XElement(NsMain + "fill",
            new XElement(NsMain + "patternFill",
                new XAttribute("patternType", "solid"),
                new XElement(NsMain + "fgColor", new XAttribute("rgb", argbHex)),
                new XElement(NsMain + "bgColor", new XAttribute("indexed", "64"))
            )
        );
    }

    // Hàm tạo định nghĩa khung viền 4 cạnh theo thứ tự chuẩn left-right-top-bottom-diagonal
    private static XElement CreateBorder(string? leftSpec, string? rightSpec, string? topSpec, string? bottomSpec)
    {
        return new XElement(NsMain + "border",
            CreateBorderEdge("left", leftSpec),
            CreateBorderEdge("right", rightSpec),
            CreateBorderEdge("top", topSpec),
            CreateBorderEdge("bottom", bottomSpec),
            new XElement(NsMain + "diagonal")
        );
    }

    // Hàm tạo một cạnh viền đơn lẻ từ chuỗi cấu hình dạng "style:ARGB"
    private static XElement CreateBorderEdge(string edgeName, string? spec)
    {
        if (string.IsNullOrWhiteSpace(spec))
        {
            return new XElement(NsMain + edgeName);
        }

        string[] parts = spec.Split(':');
        string style = parts[0];
        string color = parts.Length > 1 ? parts[1] : "FFCBD5E1";

        return new XElement(NsMain + edgeName,
            new XAttribute("style", style),
            new XElement(NsMain + "color", new XAttribute("rgb", color))
        );
    }

    // Hàm tạo một kiểu định dạng ô (xf) kết hợp số, font, nền, viền và căn lề
    private static XElement CreateCellXf(int numFmtId, int fontId, int fillId, int borderId, string horizontalAlign, bool wrapText)
    {
        XElement alignment = new XElement(NsMain + "alignment",
            new XAttribute("horizontal", horizontalAlign),
            new XAttribute("vertical", "center")
        );

        if (wrapText)
        {
            alignment.SetAttributeValue("wrapText", "1");
        }

        return new XElement(NsMain + "xf",
            new XAttribute("numFmtId", numFmtId.ToString(CultureInfo.InvariantCulture)),
            new XAttribute("fontId", fontId.ToString(CultureInfo.InvariantCulture)),
            new XAttribute("fillId", fillId.ToString(CultureInfo.InvariantCulture)),
            new XAttribute("borderId", borderId.ToString(CultureInfo.InvariantCulture)),
            new XAttribute("xfId", "0"),
            new XAttribute("applyNumberFormat", numFmtId > 0 ? "1" : "0"),
            new XAttribute("applyFont", "1"),
            new XAttribute("applyFill", "1"),
            new XAttribute("applyBorder", borderId > 0 ? "1" : "0"),
            new XAttribute("applyAlignment", "1"),
            alignment
        );
    }

    // Hàm chuyển chỉ số cột (1 -> A, 2 -> B, ..., 27 -> AA) sang ký hiệu cột của Excel
    private static string GetColumnLetter(int columnNumber)
    {
        int dividend = columnNumber;
        StringBuilder columnName = new StringBuilder();

        while (dividend > 0)
        {
            int modulo = (dividend - 1) % 26;
            columnName.Insert(0, Convert.ToChar('A' + modulo));
            dividend = (dividend - modulo) / 26;
        }

        return columnName.ToString();
    }

    // Hàm lọc bỏ các ký tự điều khiển không hợp lệ trong chuẩn XML 1.0
    private static string SanitizeXmlText(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        StringBuilder sb = new StringBuilder(input.Length);
        for (int i = 0; i < input.Length; i++)
        {
            char ch = input[i];
            if (ch == '\t' || ch == '\n' || ch == '\r' || (ch >= 0x20 && ch <= 0xD7FF) || (ch >= 0xE000 && ch <= 0xFFFD))
            {
                sb.Append(ch);
            }
        }

        return sb.ToString();
    }
}

using BAIDOXETHONGMINH.DAL.Repositories;
using BAIDOXETHONGMINH.DTO.Entities;
using BAIDOXETHONGMINH.DTO.Models;

namespace BAIDOXETHONGMINH.Services;

/// <summary>
/// Tầng xử lý logic nghiệp vụ cho Nghiệp vụ 1 (Xe Vào Bãi) & Nghiệp vụ 2 (Xe Ra Bãi & Tính Tiền)
/// </summary>
public class ParkingService
{
    private readonly ParkingRepository _parkingRepository;
    private readonly FeeCalculationService _feeCalculationService;

    public ParkingService()
    {
        _parkingRepository = new ParkingRepository();
        _feeCalculationService = new FeeCalculationService();
    }

    public ParkingService(ParkingRepository parkingRepository)
    {
        _parkingRepository = parkingRepository;
        _feeCalculationService = new FeeCalculationService();
    }

    public ParkingService(ParkingRepository parkingRepository, FeeCalculationService feeCalculationService)
    {
        _parkingRepository = parkingRepository;
        _feeCalculationService = feeCalculationService;
    }

    /// <summary>
    /// Đảm bảo danh mục cơ bản (Loại xe, Ô đỗ, Thẻ xe, Bảng giá) đã sẵn sàng khi chạy thử nghiệm
    /// </summary>
    public void EnsureCatalogForTesting()
    {
        _parkingRepository.SeedBaseCatalogIfEmpty();
    }

    /// <summary>
    /// Lấy danh sách tất cả loại phương tiện (XEMAY, OTO)
    /// </summary>
    public List<VehicleType> GetVehicleTypes()
    {
        return _parkingRepository.GetVehicleTypes();
    }

    /// <summary>
    /// Lấy danh sách các thẻ xe đang rảnh (Status = 'Available')
    /// </summary>
    public List<ParkingCard> GetAvailableCards()
    {
        return _parkingRepository.GetAvailableCards();
    }

    /// <summary>
    /// Lấy danh sách các ô đỗ còn trống theo mã loại xe
    /// </summary>
    public List<ParkingSlot> GetAvailableSlots(string vehicleTypeId)
    {
        if (string.IsNullOrWhiteSpace(vehicleTypeId))
        {
            return new List<ParkingSlot>();
        }

        return _parkingRepository.GetAvailableSlotsByVehicleType(vehicleTypeId.Trim());
    }

    /// <summary>
    /// Lấy số lượng ô đỗ còn trống trên tổng số ô đỗ của bãi
    /// </summary>
    public (int AvailableSlots, int TotalSlots) GetSlotCapacitySummary()
    {
        return _parkingRepository.GetSlotCapacitySummary();
    }

    /// <summary>
    /// Kiểm tra xem biển số xe có thuộc Vé tháng đang hoạt động và còn hạn hay không
    /// </summary>
    public MonthlyPass? GetActiveMonthlyPass(string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
        {
            return null;
        }

        return _parkingRepository.GetActiveMonthlyPassByPlate(licensePlate.Trim());
    }

    /// <summary>
    /// Xử lý nghiệp vụ Cho Xe Vào Bãi (Nghiệp vụ 1 - Check-in):
    /// Kiểm tra hợp lệ đầu vào, chống trùng biển số đang đỗ, lưu ảnh vào Assets/SessionImages
    /// và ghi nhận lượt gửi xuống CSDL.
    /// </summary>
    public (bool IsSuccess, string Message) CheckInVehicle(
        string licensePlate,
        string vehicleTypeId,
        string cardCode,
        string slotCode,
        string? sourceImagePath,
        int checkInUserId = 1)
    {
        string cleanedPlate = FormatLicensePlate(licensePlate);
        if (string.IsNullOrEmpty(cleanedPlate))
        {
            return (false, "Vui lòng nhập Biển số xe trước khi cho xe vào bãi!");
        }

        if (string.IsNullOrWhiteSpace(sourceImagePath) || !File.Exists(sourceImagePath))
        {
            return (false, "Vui lòng bấm 'Chọn hình từ máy...' để chọn ảnh xe vào bãi!");
        }

        if (string.IsNullOrWhiteSpace(vehicleTypeId))
        {
            return (false, "Vui lòng chọn Loại phương tiện!");
        }

        if (string.IsNullOrWhiteSpace(cardCode))
        {
            return (false, "Hiện không có Mã thẻ trống được chọn. Vui lòng kiểm tra lại thẻ xe!");
        }

        if (string.IsNullOrWhiteSpace(slotCode))
        {
            return (false, "Khu vực này đã hết Ô đỗ trống hoặc bạn chưa chọn vị trí đỗ!");
        }

        // Kiểm tra chống trùng biển số đang gửi trong bãi
        bool isAlreadyParked = _parkingRepository.IsLicensePlateCurrentlyParked(cleanedPlate);
        if (isAlreadyParked)
        {
            return (false, $"Xe mang biển số '{cleanedPlate}' hiện đang ở trong bãi (chưa làm thủ tục ra)!");
        }

        // Kiểm tra vé tháng hợp lệ
        MonthlyPass? activePass = _parkingRepository.GetActiveMonthlyPassByPlate(cleanedPlate);
        bool hasMonthlyPass = activePass != null;

        // Sao chép ảnh vào thư mục Assets/SessionImages
        string savedImagePath = SaveCheckInImageToAssets(sourceImagePath, cleanedPlate);

        ParkingSession session = new ParkingSession
        {
            CardCode = cardCode.Trim(),
            LicensePlate = cleanedPlate,
            VehicleTypeId = vehicleTypeId.Trim(),
            SlotCode = slotCode.Trim(),
            CheckInTime = DateTime.Now,
            CheckInImagePath = string.IsNullOrEmpty(savedImagePath) ? sourceImagePath : savedImagePath,
            CheckInUserId = checkInUserId <= 0 ? 1 : checkInUserId,
            IsMonthlyPass = hasMonthlyPass,
            ParkingFee = 0m,
            PenaltyFee = 0m,
            TotalAmount = 0m,
            Status = "Parking"
        };

        bool inserted = _parkingRepository.InsertCheckInSession(session);
        if (!inserted)
        {
            return (false, "Không thể lưu lượt xe vào CSDL (Thẻ hoặc Ô đỗ vừa được sử dụng). Vui lòng thử lại!");
        }

        string passLabel = hasMonthlyPass ? "Khách Vé Tháng (Miễn phí lượt)" : "Khách Vãng Lai (Vé lượt)";
        return (
            true,
            $"Đã cho xe vào bãi thành công!\n\n" +
            $"• Biển số: {session.LicensePlate}\n" +
            $"• Mã thẻ: {session.CardCode}  |  Ô đỗ: {session.SlotCode}\n" +
            $"• Giờ vào: {session.CheckInTime:dd/MM/yyyy HH:mm:ss}\n" +
            $"• Phân loại: {passLabel}"
        );
    }

    /// <summary>
    /// Sao chép file ảnh xe người dùng chọn từ OpenFileDialog vào thư mục Assets/SessionImages
    /// và trả về đường dẫn lưu trong CSDL.
    /// </summary>
    public string SaveCheckInImageToAssets(string sourceImagePath, string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(sourceImagePath) || !File.Exists(sourceImagePath))
        {
            return string.Empty;
        }

        string sessionImagesDir = ResolveSessionImagesDirectory();
        Directory.CreateDirectory(sessionImagesDir);

        string safePlate = SanitizeFileNameToken(licensePlate);
        string extension = Path.GetExtension(sourceImagePath);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".jpg";
        }

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string destFileName = $"IN_{safePlate}_{timestamp}{extension}";
        string destFullPath = Path.Combine(sessionImagesDir, destFileName);

        File.Copy(sourceImagePath, destFullPath, true);
        return destFullPath;
    }

    /// <summary>
    /// Lấy danh sách tất cả lượt xe đang gửi trong bãi (Status = 'Parking')
    /// </summary>
    public List<ParkingSession> GetActiveParkingSessions()
    {
        return _parkingRepository.GetAllActiveParkingSessions();
    }

    /// <summary>
    /// Lấy danh sách các mô hình xem trước (CheckOutPreviewModel) của toàn bộ xe đang trong bãi
    /// để hiển thị trực tiếp lên ComboBox tìm kiếm ở Làn Ra.
    /// </summary>
    public List<CheckOutPreviewModel> GetActiveCheckOutPreviews(bool isLostCard = false)
    {
        List<ParkingSession> activeSessions = _parkingRepository.GetAllActiveParkingSessions();
        List<CheckOutPreviewModel> previews = new List<CheckOutPreviewModel>();

        foreach (ParkingSession session in activeSessions)
        {
            CheckOutPreviewModel preview = BuildCheckOutPreview(session, isLostCard);
            previews.Add(preview);
        }

        return previews;
    }

    /// <summary>
    /// Tra cứu thông tin xe lúc vào và tự động tính toán chi tiết số tiền cần thu ở Làn Ra
    /// theo Mã thẻ hoặc Biển số xe.
    /// </summary>
    public CheckOutPreviewModel? PreviewCheckOut(string cardOrPlate, bool isLostCard)
    {
        if (string.IsNullOrWhiteSpace(cardOrPlate))
        {
            return null;
        }

        ParkingSession? activeSession = _parkingRepository.GetActiveSessionByCardOrPlate(cardOrPlate.Trim());
        if (activeSession == null)
        {
            return null;
        }

        return BuildCheckOutPreview(activeSession, isLostCard);
    }

    /// <summary>
    /// Xác nhận thu tiền và cho xe ra bãi (Nghiệp vụ 2 - Check-out):
    /// Kiểm tra hợp lệ (nếu tích mất thẻ bắt buộc phải nhập số CCCD biên bản),
    /// lưu kết quả vào ParkingSessions, PaymentTransactions, IncidentReports và giải phóng Ô đỗ + Thẻ.
    /// </summary>
    public (bool IsSuccess, string Message) ConfirmCheckOut(
        string cardOrPlate,
        bool isLostCard,
        string? customerIdentityCard = null,
        string? customerName = null,
        string? customerPhone = null,
        string? incidentNote = null,
        int checkOutUserId = 1)
    {
        if (string.IsNullOrWhiteSpace(cardOrPlate))
        {
            return (false, "Vui lòng chọn hoặc nhập Mã thẻ / Biển số xe cần làm thủ tục ra bãi!");
        }

        ParkingSession? activeSession = _parkingRepository.GetActiveSessionByCardOrPlate(cardOrPlate.Trim());
        if (activeSession == null)
        {
            return (false, "Không tìm thấy xe đang đỗ trong bãi khớp với Mã thẻ hoặc Biển số này!");
        }

        if (isLostCard && string.IsNullOrWhiteSpace(customerIdentityCard))
        {
            return (false, "Khách làm mất thẻ bắt buộc phải nhập số CCCD/CMND vào biên bản trước khi xác nhận cho xe ra!");
        }

        CheckOutPreviewModel preview = BuildCheckOutPreview(activeSession, isLostCard);

        activeSession.CheckOutTime = preview.CheckOutTime;
        activeSession.CheckOutUserId = checkOutUserId <= 0 ? 1 : checkOutUserId;
        activeSession.IsMonthlyPass = preview.FeeResult.IsMonthlyPass;
        activeSession.ParkingFee = preview.FeeResult.ParkingFee;
        activeSession.PenaltyFee = preview.FeeResult.PenaltyFee;
        activeSession.TotalAmount = preview.FeeResult.TotalAmount;
        activeSession.Status = "Completed";

        IncidentReport? incidentReport = null;
        if (isLostCard)
        {
            string cleanedIdentityCard = customerIdentityCard!.Trim();
            string cleanedCustomerName = string.IsNullOrWhiteSpace(customerName) ? "Khách gửi xe" : customerName.Trim();
            string cleanedPhone = string.IsNullOrWhiteSpace(customerPhone) ? string.Empty : customerPhone.Trim();
            string cleanedNote = string.IsNullOrWhiteSpace(incidentNote)
                ? $"Báo mất thẻ {activeSession.CardCode} tại cổng ra (CCCD: {cleanedIdentityCard})"
                : incidentNote.Trim();

            incidentReport = new IncidentReport
            {
                SessionId = activeSession.SessionId,
                CardCode = activeSession.CardCode,
                LicensePlate = activeSession.LicensePlate,
                CustomerName = cleanedCustomerName,
                CustomerPhone = cleanedPhone,
                CustomerIdentityCard = cleanedIdentityCard,
                IncidentType = "LostCard",
                CompensationFee = preview.FeeResult.PenaltyFee,
                Note = cleanedNote,
                HandledByUserId = checkOutUserId <= 0 ? 1 : checkOutUserId,
                CreatedAt = preview.CheckOutTime
            };
        }

        bool completed = _parkingRepository.CompleteCheckOutSession(
            activeSession,
            isLostCard,
            incidentReport,
            checkOutUserId
        );

        if (!completed)
        {
            return (false, "Không thể hoàn tất thủ tục trả xe trong CSDL. Vui lòng kiểm tra và thử lại!");
        }

        string cardStateMessage = isLostCard
            ? $"Thẻ {activeSession.CardCode} đã chuyển sang trạng thái Mất ('Lost') & lưu biên bản."
            : $"Thẻ {activeSession.CardCode} và Ô {activeSession.SlotCode} đã được giải phóng.";

        return (
            true,
            $"Đã cho xe ra bãi thành công!\n\n" +
            $"• Biển số: {activeSession.LicensePlate}  |  Ô đỗ: {activeSession.SlotCode}\n" +
            $"• Thời gian đỗ: {preview.FeeResult.DurationText}\n" +
            $"• Tiền gửi xe: {preview.FeeResult.ParkingFee:N0} VNĐ  |  Tiền phạt: {preview.FeeResult.PenaltyFee:N0} VNĐ\n" +
            $"• TỔNG THU: {preview.FeeResult.TotalAmount:N0} VNĐ\n" +
            $"• {cardStateMessage}"
        );
    }

    private CheckOutPreviewModel BuildCheckOutPreview(ParkingSession session, bool isLostCard)
    {
        DateTime checkOutTime = DateTime.Now;
        PricingRule? pricingRule = _parkingRepository.GetPricingRuleByVehicleType(session.VehicleTypeId);
        MonthlyPass? activePass = _parkingRepository.GetActiveMonthlyPassByPlate(session.LicensePlate);

        FeeCalculationResult feeResult = _feeCalculationService.CalculateFee(
            session.CheckInTime,
            checkOutTime,
            session.VehicleTypeId,
            pricingRule,
            activePass,
            isLostCard
        );

        bool isCar = string.Equals(session.VehicleTypeId.Trim(), "OTO", StringComparison.OrdinalIgnoreCase);
        string vehicleTypeName = isCar ? "Ô tô" : "Xe máy";
        string passShortLabel = feeResult.IsMonthlyPass ? "Vé tháng" : "Vé lượt";

        return new CheckOutPreviewModel
        {
            SessionId = session.SessionId,
            CardCode = session.CardCode,
            LicensePlate = session.LicensePlate,
            VehicleTypeId = session.VehicleTypeId,
            VehicleTypeName = vehicleTypeName,
            SlotCode = session.SlotCode,
            CheckInTime = session.CheckInTime,
            CheckOutTime = checkOutTime,
            CheckInImagePath = session.CheckInImagePath,
            DisplaySearchText = $"Mã thẻ: {session.CardCode} — Biển số {session.LicensePlate} ({vehicleTypeName} • Ô {session.SlotCode} • {passShortLabel})",
            FeeResult = feeResult
        };
    }

    /// <summary>
    /// Chuẩn hóa biển số xe (cắt khoảng trắng thừa ở 2 đầu và viết hoa)
    /// </summary>
    public static string FormatLicensePlate(string rawPlate)
    {
        if (string.IsNullOrWhiteSpace(rawPlate))
        {
            return string.Empty;
        }

        return rawPlate.Trim().ToUpperInvariant();
    }

    private static string ResolveSessionImagesDirectory()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        DirectoryInfo? currentDir = new DirectoryInfo(baseDir);

        while (currentDir != null)
        {
            string projectAssetsPath = Path.Combine(currentDir.FullName, "Assets");
            if (Directory.Exists(projectAssetsPath))
            {
                return Path.Combine(projectAssetsPath, "SessionImages");
            }

            string nestedAssetsPath = Path.Combine(currentDir.FullName, "BAIDOXETHONGMINH", "Assets");
            if (Directory.Exists(nestedAssetsPath))
            {
                return Path.Combine(nestedAssetsPath, "SessionImages");
            }

            currentDir = currentDir.Parent;
        }

        return Path.Combine(baseDir, "Assets", "SessionImages");
    }

    private static string SanitizeFileNameToken(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return "UNKNOWN";
        }

        char[] invalidChars = Path.GetInvalidFileNameChars();
        string cleaned = rawText.Trim().Replace(" ", string.Empty).Replace("-", string.Empty).Replace(".", string.Empty).ToUpperInvariant();

        foreach (char invalidChar in invalidChars)
        {
            cleaned = cleaned.Replace(invalidChar.ToString(), string.Empty);
        }

        return string.IsNullOrWhiteSpace(cleaned) ? "UNKNOWN" : cleaned;
    }
}

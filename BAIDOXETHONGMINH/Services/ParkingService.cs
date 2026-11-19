using BAIDOXETHONGMINH.DAL.Repositories;
using BAIDOXETHONGMINH.DTO.Entities;
using BAIDOXETHONGMINH.DTO.Models;

namespace BAIDOXETHONGMINH.Services;

public class ParkingService
{
    private readonly ParkingRepository _parkingRepository;
    private readonly FeeCalculationService _feeCalculationService;

    // Hàm khởi tạo mặc định của ParkingService
    public ParkingService()
    {
        _parkingRepository = new ParkingRepository();
        _feeCalculationService = new FeeCalculationService();
    }

    // Hàm khởi tạo ParkingService với đối tượng ParkingRepository truyền vào
    public ParkingService(ParkingRepository parkingRepository)
    {
        _parkingRepository = parkingRepository;
        _feeCalculationService = new FeeCalculationService();
    }

    // Hàm khởi tạo ParkingService với đầy đủ ParkingRepository và FeeCalculationService
    public ParkingService(ParkingRepository parkingRepository, FeeCalculationService feeCalculationService)
    {
        _parkingRepository = parkingRepository;
        _feeCalculationService = feeCalculationService;
    }

    // Hàm đảm bảo danh mục cơ bản (Loại xe, Ô đỗ, Thẻ xe, Bảng giá) đã sẵn sàng khi chạy thử nghiệm
    public void EnsureCatalogForTesting()
    {
        _parkingRepository.SeedBaseCatalogIfEmpty();
    }

    // Hàm lấy danh sách tất cả loại phương tiện (XEMAY, OTO)
    public List<VehicleType> GetVehicleTypes()
    {
        return _parkingRepository.GetVehicleTypes();
    }

    // Hàm lấy danh sách các thẻ xe đang rảnh (Status = 'Available')
    public List<ParkingCard> GetAvailableCards()
    {
        return _parkingRepository.GetAvailableCards();
    }

    // Hàm lấy danh sách các ô đỗ còn trống theo mã loại phương tiện
    public List<ParkingSlot> GetAvailableSlots(string vehicleTypeId)
    {
        if (string.IsNullOrWhiteSpace(vehicleTypeId))
        {
            return new List<ParkingSlot>();
        }

        return _parkingRepository.GetAvailableSlotsByVehicleType(vehicleTypeId.Trim());
    }

    // Hàm lấy số lượng ô đỗ còn trống trên tổng số ô đỗ của bãi
    public (int AvailableSlots, int TotalSlots) GetSlotCapacitySummary()
    {
        return _parkingRepository.GetSlotCapacitySummary();
    }

    // Hàm kiểm tra biển số xe có thuộc Vé tháng đang hoạt động và còn hạn hay không
    public MonthlyPass? GetActiveMonthlyPass(string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
        {
            return null;
        }

        return _parkingRepository.GetActiveMonthlyPassByPlate(licensePlate.Trim());
    }

    // Hàm xử lý nghiệp vụ Cho Xe Vào Bãi (Nghiệp vụ 1 - Check-in), kiểm tra hợp lệ, lưu ảnh và ghi CSDL
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

        bool isAlreadyParked = _parkingRepository.IsLicensePlateCurrentlyParked(cleanedPlate);
        if (isAlreadyParked)
        {
            return (false, $"Xe mang biển số '{cleanedPlate}' hiện đang ở trong bãi (chưa làm thủ tục ra)!");
        }

        MonthlyPass? activePass = _parkingRepository.GetActiveMonthlyPassByPlate(cleanedPlate);
        bool hasMonthlyPass = activePass != null;

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
            return (false, "Không thể ghi nhận lượt xe vào bãi (Thẻ hoặc Ô đỗ vừa được sử dụng). Vui lòng thử lại!");
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

    // Hàm sao chép file ảnh xe lúc vào sang thư mục Assets/SessionImages và trả về đường dẫn lưu trữ
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

    // Hàm lấy danh sách tất cả lượt xe đang gửi trong bãi (Status = 'Parking')
    public List<ParkingSession> GetActiveParkingSessions()
    {
        return _parkingRepository.GetAllActiveParkingSessions();
    }

    // Hàm lấy danh sách các mô hình xem trước của toàn bộ xe đang đỗ để hiển thị lên ComboBox Làn Ra
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

    // Hàm tra cứu thông tin xe lúc vào và tự động tính toán chi tiết số tiền cần thu theo Mã thẻ hoặc Biển số
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

    // Hàm xác nhận thu tiền, lưu biên bản mất thẻ (nếu có) và giải phóng chỗ đỗ khi cho xe ra bãi
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
            return (false, "Không thể hoàn tất thủ tục trả xe trên hệ thống. Vui lòng kiểm tra và thử lại!");
        }

        string cardStateMessage = isLostCard
            ? $"Thẻ {activeSession.CardCode} đã được khóa do báo mất và lưu biên bản."
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

    // Hàm tạo đối tượng CheckOutPreviewModel chứa thông tin lượt đỗ và kết quả tính phí tại thời điểm hiện tại
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

    // Hàm chuẩn hóa chuỗi biển số xe (cắt khoảng trắng thừa ở 2 đầu và chuyển thành chữ in hoa)
    public static string FormatLicensePlate(string rawPlate)
    {
        if (string.IsNullOrWhiteSpace(rawPlate))
        {
            return string.Empty;
        }

        return rawPlate.Trim().ToUpperInvariant();
    }

    // Hàm xác định đường dẫn tới thư mục Assets/SessionImages để lưu hình ảnh xe vào bãi
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

    // Hàm loại bỏ các ký tự đặc biệt khỏi biển số để tạo tên file ảnh hợp lệ
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

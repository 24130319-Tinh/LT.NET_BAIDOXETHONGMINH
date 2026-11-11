using BAIDOXETHONGMINH.DAL.Repositories;
using BAIDOXETHONGMINH.DTO.Entities;

namespace BAIDOXETHONGMINH.Services;

/// <summary>
/// Tầng xử lý logic nghiệp vụ cho Nghiệp vụ 1 (Xe Vào Bãi) & Nghiệp vụ 2 (Xe Ra Bãi & Tính Tiền)
/// </summary>
public class ParkingService
{
    private readonly ParkingRepository _parkingRepository;

    public ParkingService()
    {
        _parkingRepository = new ParkingRepository();
    }

    public ParkingService(ParkingRepository parkingRepository)
    {
        _parkingRepository = parkingRepository;
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

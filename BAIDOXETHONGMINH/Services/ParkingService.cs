using BAIDOXETHONGMINH.DAL.Repositories;

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

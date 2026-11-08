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
}

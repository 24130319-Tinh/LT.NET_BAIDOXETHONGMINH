namespace BAIDOXETHONGMINH.DTO.Entities;

/// <summary>
/// Bảng VehicleTypes: Danh mục loại phương tiện (VD: XEMAY, OTO)
/// </summary>
public class VehicleType
{
    public string VehicleTypeId { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

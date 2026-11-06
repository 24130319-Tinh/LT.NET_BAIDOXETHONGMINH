using BAIDOXETHONGMINH.DTO.Entities;

namespace BAIDOXETHONGMINH.Models;

/// <summary>Model hiển thị một khu cùng các ô của nó trên sơ đồ 2D (không phải bảng CSDL).</summary>
public class ZoneView
{
    public ParkingZone Zone { get; set; } = new();
    public List<ParkingSlot> Slots { get; set; } = new();

    public int Total { get; set; }
    public int Available { get; set; }
    public int Occupied { get; set; }
    public int Maintenance { get; set; }
}

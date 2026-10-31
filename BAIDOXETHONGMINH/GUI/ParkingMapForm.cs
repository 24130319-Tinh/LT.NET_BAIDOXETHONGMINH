using BAIDOXETHONGMINH.GUI;

namespace BAIDOXETHONGMINH.GUI;





public class ParkingMapForm : Form
{
    private static readonly Color AvailableColor = Color.FromArgb(165, 214, 167);
    private static readonly Color OccupiedColor = Color.FromArgb(239, 154, 154);
    private static readonly Color MaintenanceColor = Color.FromArgb(255, 224, 130);
    private readonly ParkingMapService _map = AppService.Map;
}
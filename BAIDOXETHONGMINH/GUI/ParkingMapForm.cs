using BAIDOXETHONGMINH.GUI;
using BAIDOXETHONGMINH.Services;

namespace BAIDOXETHONGMINH.GUI;
using BAIDOXETHONGMINH.Models;
using BAIDOXETHONGMINH.DTO.Entities;




public class ParkingMapForm : Form
{
    private static readonly Color AvailableColor = Color.FromArgb(165, 214, 167);
    private static readonly Color OccupiedColor = Color.FromArgb(239, 154, 154);
    private static readonly Color MaintenanceColor = Color.FromArgb(255, 224, 130);
    private readonly ParkingMapService _map = AppService.Map;
    private readonly PricingService _pricing = AppService.Pricing;

    private readonly FlowLayoutPanel _zonesPanel = new()
    {
        Dock = DockStyle.Fill,
        AutoScroll = true,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        Padding = new Padding(10)
    };

    private readonly Label _labelSubtitle = new()
    {
        AutoSize = true,
        ForeColor = Color.Gray,
        Left = 12,
        Top = 34
    };

    private readonly Label _labelSlotCode = new()
    {
        AutoSize = true,
        Left = 14,
        Top = 26
    };

    private readonly Label _labelSlotStaus = new()
    {
        AutoSize = true, Left = 14,
        Top = 52
    };
    private readonly Label _labelSlotPlate = new()
    {
        AutoSize = true,
        Left = 14,
        Top = 78
    };

    private readonly Button _btnMaintenance = new()
    {
        Text = "Đổi trạng thái Bảo Trì / Mở rộng",
        Left = 14,
        Top = 100,
        Width = 262,
        Height = 31
    };
    private readonly Label _lblPricing = new()
    {
        AutoSize = true,
        Left = 4,
        Top = 4
    };
    private readonly Dictionary<string, Button> _slotButtons = new();
    private List<ZoneView> _zones = new();
    private List<VehicleType> _types = new();
    private string? _selectedCode;

    public ParkingMapForm()
    {
        Text = "Sơ đồ bãi đỗ xe & Bảng giá";
        Size = new Size(1000, 680);
        MinimumSize = new Size(760, 480);
        StartPosition = FormStartPosition.CenterScreen;
        BuildLayout();
        
        _btnMaintenance.Click += async (_, _) => await ToggleMaintenanceAsync();

        
        _map.MapChanged += OnMapChanged;                   
        FormClosed += (_, _) => _map.MapChanged -= OnMapChanged;
        ResizeEnd += async (_, _) => await RenderAsync();
        Load += async (_, _) =>
        {
            if (await Dialogs.RunAsync(AppService.InitializeAsync)) await RenderAsync();
        };
        
    }
    


    private void BuildLayout()
    {
        
    }

    private async Task ToggleMaintenanceAsync()
    {
        string? code = _selectedCode;
        if (code == null) return;
        await Dialogs.RunAsync(() => _map.ToggleMaintenanceAsync(code));
    }
    private async Task RenderAsync()
    {
        try
        {
            _zones = await _map.GetMapAsync();
            _types = await _pricing.GetVehicleTypesAsync();
            var rules = await _pricing.GetAllAsync();

            ShowZones();
            ShowPricing(rules);
            ShowSlotInfo();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không tải được dữ liệu: " + ex.Message, "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowSlotInfo()
    {
        throw new NotImplementedException();
    }

    private void ShowPricing(object rules)
    {
        throw new NotImplementedException();
    }

    private void ShowZones()
    {
        throw new NotImplementedException();
    }

    private void OnMapChanged()
    {
        
    }
    

}
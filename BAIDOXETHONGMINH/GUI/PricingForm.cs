using BAIDOXETHONGMINH.DTO.Entities;
using BAIDOXETHONGMINH.Services;

namespace BAIDOXETHONGMINH.GUI;

public class PricingForm : Form
{
    private readonly PricingService _pricing = AppService.Pricing;

    private readonly ComboBox _cboType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _dayTurn = MakeNumber();
    private readonly NumericUpDown _overnight = MakeNumber();
    private readonly NumericUpDown _hourly = MakeNumber();
    private readonly NumericUpDown _monthly = MakeNumber();
    private readonly NumericUpDown _penalty = MakeNumber();
    private bool _loading;

    // Hàm khởi tạo Form cấu hình bảng giá và khóa cố định mức phạt mất thẻ 50.000đ
    public PricingForm()
    {
        Text = "Cấu hình bảng giá";
        ClientSize = new Size(400, 310);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        _penalty.Enabled = false;
        _penalty.Value = FeeCalculationService.UnifiedLostCardPenalty;

        (string Label, Control Input)[] rows = new (string Label, Control Input)[]
        {
            ("Loại xe", _cboType),
            ("Giá lượt ban ngày (đ)", _dayTurn),
            ("Giá lượt qua đêm (đ)", _overnight),
            ("Giá theo giờ (đ)", _hourly),
            ("Giá vé tháng (đ)", _monthly),
            ("Phạt mất thẻ cố định (đ)", _penalty)
        };

        int y = 16;
        foreach ((string label, Control input) in rows)
        {
            Controls.Add(new Label { Text = label, Left = 16, Top = y + 4, Width = 170 });
            input.Left = 195;
            input.Top = y;
            input.Width = 185;
            Controls.Add(input);
            y += 38;
        }

        Button save = new Button { Text = "Lưu bảng giá", Left = 195, Top = y + 8, Width = 185, Height = 32 };
        save.Click += async (_, _) => await SaveAsync();
        Controls.Add(save);

        _cboType.SelectedIndexChanged += async (_, _) => await Dialogs.RunAsync(LoadRuleAsync);
        Load += async (_, _) => await LoadTypesAsync();
    }

    // Hàm tạo nhanh ô nhập số tiền chuẩn cho bảng giá
    private static NumericUpDown MakeNumber()
    {
        return new NumericUpDown
        {
            Maximum = 100_000_000,
            Increment = 1000,
            ThousandsSeparator = true,
            DecimalPlaces = 0
        };
    }

    // Hàm nạp danh sách loại phương tiện lên ComboBox và tải bảng giá tương ứng
    private async Task LoadTypesAsync()
    {
        await Dialogs.RunAsync(async () =>
        {
            await AppService.InitializeAsync();
            List<VehicleType> types = await _pricing.GetVehicleTypesAsync();

            _loading = true;
            _cboType.DisplayMember = "TypeName";
            _cboType.ValueMember = "VehicleTypeId";
            _cboType.DataSource = types;
            _loading = false;

            await LoadRuleAsync();
        });
    }

    // Hàm nạp cấu hình giá hiện tại theo loại phương tiện đang chọn
    private async Task LoadRuleAsync()
    {
        string? typeId = _cboType.SelectedValue as string;
        if (_loading || typeId == null) return;

        PricingRule? rule = await _pricing.GetByVehicleTypeAsync(typeId);
        _dayTurn.Value = rule?.DayTurnPrice ?? 0;
        _overnight.Value = rule?.OvernightPrice ?? 0;
        _hourly.Value = rule?.HourlyPrice ?? 0;
        _monthly.Value = rule?.MonthlyPassPrice ?? 0;
        _penalty.Value = FeeCalculationService.UnifiedLostCardPenalty;
    }

    // Hàm lưu cấu hình bảng giá xuống CSDL với tiền phạt mất thẻ cố định 50.000đ
    private async Task SaveAsync()
    {
        string? typeId = _cboType.SelectedValue as string;
        if (typeId == null) return;

        PricingRule rule = new PricingRule
        {
            VehicleTypeId = typeId,
            DayTurnPrice = _dayTurn.Value,
            OvernightPrice = _overnight.Value,
            HourlyPrice = _hourly.Value,
            MonthlyPassPrice = _monthly.Value,
            LostCardPenalty = FeeCalculationService.UnifiedLostCardPenalty
        };
        if (await Dialogs.RunAsync(() => _pricing.SaveAsync(rule)))
            MessageBox.Show("Đã lưu bảng giá.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}

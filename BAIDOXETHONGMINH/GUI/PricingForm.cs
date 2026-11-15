using BAIDOXETHONGMINH.DTO.Entities;

namespace BAIDOXETHONGMINH.GUI;

public class PricingForm : Form
{
    private readonly PricingService _pricing = AppService.Pricing;

    private readonly ComboBox _cboType = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _dayTurn = MakeNumber();
    private readonly NumericUpDown _overnight = MakeNumber();
    private readonly NumericUpDown _hourly = MakeNumber();
    private readonly NumericUpDown _monthly = MakeNumber();
    private readonly NumericUpDown _penalty = MakeNumber();
    private bool _loading;

    public PricingForm()
    {
        Text = "Cấu hình bảng giá";
        ClientSize = new Size(380, 310);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var rows = new (string Label, Control Input)[]
        {
            ("Loại xe", _cboType),
            ("Giá lượt ban ngày (đ)", _dayTurn),
            ("Giá lượt qua đêm (đ)", _overnight),
            ("Giá theo giờ (đ)", _hourly),
            ("Giá vé tháng (đ)", _monthly),
            ("Phạt mất thẻ (đ)", _penalty)
        };

        int y = 16;
        foreach (var (label, input) in rows)
        {
            Controls.Add(new Label { Text = label, Left = 16, Top = y + 4, Width = 160 });
            input.Left = 185;
            input.Top = y;
            input.Width = 175;
            Controls.Add(input);
            y += 38;
        }

        var save = new Button { Text = "Lưu bảng giá", Left = 185, Top = y + 8, Width = 175, Height = 32 };
        save.Click += async (_, _) => await SaveAsync();
        Controls.Add(save);

        _cboType.SelectedIndexChanged += async (_, _) => await Dialogs.RunAsync(LoadRuleAsync);
        Load += async (_, _) => await LoadTypesAsync();
    }

    private static NumericUpDown MakeNumber() => new()
    {
        Maximum = 100_000_000,
        Increment = 1000,
        ThousandsSeparator = true,
        DecimalPlaces = 0
    };

    private async Task LoadTypesAsync()
    {
        await Dialogs.RunAsync(async () =>
        {
            await AppService.InitializeAsync();
            var types = await _pricing.GetVehicleTypesAsync();

            _loading = true;
            _cboType.DataSource = types;
            _cboType.DisplayMember = "TypeName";
            _cboType.ValueMember = "VehicleTypeId";
            _loading = false;

            await LoadRuleAsync();
        });
    }

    private async Task LoadRuleAsync()
    {
        string? typeId = _cboType.SelectedValue as string;
        if (_loading || typeId == null) return;

        var rule = await _pricing.GetByVehicleTypeAsync(typeId);
        _dayTurn.Value = rule?.DayTurnPrice ?? 0;
        _overnight.Value = rule?.OvernightPrice ?? 0;
        _hourly.Value = rule?.HourlyPrice ?? 0;
        _monthly.Value = rule?.MonthlyPassPrice ?? 0;
        _penalty.Value = rule?.LostCardPenalty ?? 0;
    }

    private async Task SaveAsync()
    {
        string? typeId = _cboType.SelectedValue as string;
        if (typeId == null) return;

        var rule = new PricingRule
        {
            VehicleTypeId = typeId,
            DayTurnPrice = _dayTurn.Value,
            OvernightPrice = _overnight.Value,
            HourlyPrice = _hourly.Value,
            MonthlyPassPrice = _monthly.Value,
            LostCardPenalty = _penalty.Value
        };
        if (await Dialogs.RunAsync(() => _pricing.SaveAsync(rule)))
            MessageBox.Show("Đã lưu bảng giá.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}

namespace BAIDOXETHONGMINH.GUI;

/// <summary>NV3: quản lý thẻ xe (thêm, khóa, mở khóa, báo mất, tìm lại, xóa). Mở từ ParkingMapForm.</summary>
public class ParkingCardForm : Form
{
    // Một dòng hiển thị trên bảng (record)
    private record CardRow(string CardCode, string LicensePlate, string StatusText);

    private readonly ParkingCardService _cards = AppService.Card;

    private readonly TextBox _txtSearch = new() { Left = 12, Top = 12, Width = 220, PlaceholderText = "Tìm mã thẻ hoặc biển số..." };
    private readonly TextBox _txtNewCode = new() { Left = 440, Top = 12, Width = 130, PlaceholderText = "Mã thẻ mới" };
    private readonly DataGridView _grid = new()
    {
        Left = 12,
        Top = 46,
        Width = 676,
        Height = 330,
        Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        ReadOnly = true,
        AllowUserToAddRows = false,
        RowHeadersVisible = false,
        MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoGenerateColumns = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        BackgroundColor = SystemColors.Window
    };

    public ParkingCardForm()
    {
        Text = "Quản lý thẻ xe";
        ClientSize = new Size(700, 430);
        MinimumSize = new Size(716, 469);
        StartPosition = FormStartPosition.CenterParent;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã thẻ", DataPropertyName = "CardCode" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Biển số đang gắn", DataPropertyName = "LicensePlate" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", DataPropertyName = "StatusText" });

        var btnAdd = MakeButton("Thêm thẻ", 580, 11, async () => await AddAsync());
        Controls.AddRange(new Control[] { _txtSearch, _txtNewCode, btnAdd, _grid });

        int x = 12;
        foreach (var (text, action) in new (string, Func<string, Task>)[]
        {
            ("Khóa thẻ", _cards.LockAsync),
            ("Mở khóa", _cards.UnlockAsync),
            ("Báo mất", _cards.ReportLostAsync),
            ("Tìm lại thẻ", _cards.RecoverAsync),
            ("Xóa thẻ", _cards.DeleteAsync)
        })
        {
            var act = action;
            var button = MakeButton(text, x, 388, async () => await ActAsync(act));
            button.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Controls.Add(button);
            x += 112;
        }

        _txtSearch.TextChanged += async (_, _) => await LoadAsync();
        Load += async (_, _) =>
        {
            if (await Dialogs.RunAsync(AppService.InitializeAsync)) await LoadAsync();
        };
    }

    private static Button MakeButton(string text, int left, int top, Func<Task> onClick)
    {
        var button = new Button { Text = text, Left = left, Top = top, Width = 104, Height = 30 };
        button.Click += async (_, _) => await onClick();
        return button;
    }

    private async Task LoadAsync()
    {
        await Dialogs.RunAsync(async () =>
        {
            var cards = await _cards.SearchAsync(_txtSearch.Text);
            _grid.DataSource = cards
                .Select(c => new CardRow(c.CardCode, c.LicensePlate ?? "", Fmt.CardText(c.Status)))
                .ToList();
        });
    }

    private async Task AddAsync()
    {
        string code = _txtNewCode.Text;
        if (await Dialogs.RunAsync(() => _cards.AddAsync(code)))
        {
            _txtNewCode.Clear();
            await LoadAsync();
        }
    }

    private async Task ActAsync(Func<string, Task> action)
    {
        if (_grid.CurrentRow?.DataBoundItem is not CardRow row)
        {
            MessageBox.Show("Hãy chọn một thẻ trong bảng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (await Dialogs.RunAsync(() => action(row.CardCode)))
            await LoadAsync();
    }
}

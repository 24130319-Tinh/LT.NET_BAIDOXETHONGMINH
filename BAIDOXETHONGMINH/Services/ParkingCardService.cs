using BAIDOXETHONGMINH.DTO.Entities;

namespace BAIDOXETHONGMINH.GUI;

/// <summary>NV3: quản lý thẻ xe. NV1, NV2 gọi IssueAsync / ReturnAsync / ReportLostAsync.</summary>
public class ParkingCardService
{
    private readonly ParkingRepository _repo = new();

    // Trạng thái nào được chuyển sang trạng thái nào
    private static readonly Dictionary<string, string[]> Allowed = new()
    {
        [CardStatus.Available] = new[] { CardStatus.InUse, CardStatus.Locked, CardStatus.Lost },
        [CardStatus.InUse] = new[] { CardStatus.Available, CardStatus.Lost },
        [CardStatus.Lost] = new[] { CardStatus.Available },
        [CardStatus.Locked] = new[] { CardStatus.Available, CardStatus.Lost }
    };

    /// <summary>Tìm theo mã thẻ hoặc biển số (để trống = lấy tất cả).</summary>
    public async Task<List<ParkingCard>> SearchAsync(string keyword)
    {
        var cards = await _repo.GetCardsAsync();
        keyword = (keyword ?? "").Trim();
        if (keyword.Length == 0) return cards;

        string plateKey = PlateHelper.Key(keyword);
        return cards.Where(c =>
            c.CardCode.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            (plateKey.Length > 0 && PlateHelper.Key(c.LicensePlate).Contains(plateKey))).ToList();
    }

    public async Task AddAsync(string cardCode)
    {
        cardCode = (cardCode ?? "").Trim().ToUpperInvariant();
        if (cardCode.Length == 0) throw new ServiceException("Mã thẻ không được để trống.");
        if (await _repo.GetCardAsync(cardCode) != null) throw new ServiceException($"Thẻ '{cardCode}' đã tồn tại.");

        await _repo.AddCardAsync(new ParkingCard { CardCode = cardCode, Status = CardStatus.Available });
    }

    /// <summary>Cấp thẻ cho xe: Available -> InUse, gắn biển số.</summary>
    public Task IssueAsync(string cardCode, string licensePlate)
    {
        string plate = PlateHelper.Clean(licensePlate);
        if (plate.Length == 0) throw new ServiceException("Biển số không hợp lệ.");
        return ChangeStatusAsync(cardCode, CardStatus.InUse, plate);
    }

    /// <summary>Thu hồi thẻ khi xe ra: InUse -> Available.</summary>
    public Task ReturnAsync(string cardCode) => ChangeStatusAsync(cardCode, CardStatus.Available, null);

    public Task ReportLostAsync(string cardCode) => ChangeStatusAsync(cardCode, CardStatus.Lost, null, keepPlate: true);

    /// <summary>Tìm lại thẻ đã báo mất: Lost -> Available.</summary>
    public Task RecoverAsync(string cardCode) => ChangeStatusAsync(cardCode, CardStatus.Available, null);

    public Task LockAsync(string cardCode) => ChangeStatusAsync(cardCode, CardStatus.Locked, null, keepPlate: true);

    public Task UnlockAsync(string cardCode) => ChangeStatusAsync(cardCode, CardStatus.Available, null);

    public async Task DeleteAsync(string cardCode)
    {
        var card = await GetRequiredAsync(cardCode);
        if (card.Status == CardStatus.InUse) throw new ServiceException("Không xóa được thẻ đang cấp cho xe.");
        await _repo.DeleteCardAsync(card.CardCode);
    }

    private async Task ChangeStatusAsync(string cardCode, string newStatus, string? plate, bool keepPlate = false)
    {
        var card = await GetRequiredAsync(cardCode);
        if (!Allowed.TryGetValue(card.Status, out var next) || !next.Contains(newStatus))
            throw new ServiceException($"Thẻ '{card.CardCode}' đang ở trạng thái '{Fmt.CardText(card.Status)}', không thể chuyển sang '{Fmt.CardText(newStatus)}'.");

        card.Status = newStatus;
        if (!keepPlate) card.LicensePlate = plate;
        await _repo.UpdateCardAsync(card);
    }

    private async Task<ParkingCard> GetRequiredAsync(string cardCode)
    {
        string code = (cardCode ?? "").Trim().ToUpperInvariant();
        var card = await _repo.GetCardAsync(code);
        if (card == null) throw new ServiceException($"Không tìm thấy thẻ '{cardCode}'.");
        return card;
    }
}

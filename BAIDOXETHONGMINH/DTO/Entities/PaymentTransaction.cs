namespace BAIDOXETHONGMINH.DTO.Entities;

public class PaymentTransaction
{
    public int TransactionId { get; set; }
    public string TransactionType { get; set; } = "ParkingFee";
    public int? ReferenceId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public int CollectedByUserId { get; set; }
    public int? ShiftId { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

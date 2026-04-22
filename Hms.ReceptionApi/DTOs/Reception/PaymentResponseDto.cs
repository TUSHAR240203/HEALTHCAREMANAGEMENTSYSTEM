namespace Hms.ReceptionApi.DTOs.Reception;

public class PaymentResponseDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMode { get; set; } = default!;
    public DateTime PaidAtUtc { get; set; }
}
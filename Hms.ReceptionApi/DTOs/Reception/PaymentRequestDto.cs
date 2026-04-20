namespace Hms.ReceptionApi.DTOs.Reception;

public class PaymentRequestDto
{
    public decimal Amount { get; set; }
    public string PaymentMode { get; set; } = default!;
}
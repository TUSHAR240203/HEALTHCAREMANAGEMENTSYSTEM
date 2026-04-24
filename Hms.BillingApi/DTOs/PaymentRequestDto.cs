namespace Hms.BillingApi.DTOs.Billing;

public class PaymentRequestDto
{
    public decimal Amount { get; set; }
    public string PaymentMode { get; set; } = default!;
}
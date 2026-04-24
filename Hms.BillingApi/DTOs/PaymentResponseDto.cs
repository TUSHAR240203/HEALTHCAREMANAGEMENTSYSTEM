namespace Hms.BillingApi.DTOs.Billing;

public class PaymentResponseDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = default!; // ✅ FIXED

    public DateTime PaidAtUtc { get; set; }
}
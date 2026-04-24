namespace Hms.BillingApi.Entities;

public class Payment : BaseEntity
{
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }

    // ✅ renamed
    public string PaymentMethod { get; set; } = default!;

    public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;

    public Invoice Invoice { get; set; } = default!;
}
namespace Hms.BillingApi.Entities;

public class InvoiceItem : BaseEntity
{
    public int InvoiceId { get; set; }
    public string ServiceName { get; set; } = default!;
    public decimal Amount { get; set; }

    public Invoice Invoice { get; set; } = default!;
}
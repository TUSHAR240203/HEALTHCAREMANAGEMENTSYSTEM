namespace Hms.BillingApi.Entities;

public class InvoiceItem
{
    public int Id { get; set; }

    public string ServiceName { get; set; } = default!;

    // ✅ ADD THESE
    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = default!;
}
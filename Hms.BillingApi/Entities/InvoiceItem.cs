namespace Hms.BillingApi.Entities;

public class InvoiceItem
{
    public int Id { get; set; }

    /// <summary>Reference to ServiceCatalog. Null for Consultation items (auto-added from DoctorsApi).</summary>
    public int? ServiceId { get; set; }

    public string ServiceName { get; set; } = default!;

    /// <summary>Consultation | Test | Medicine</summary>
    public string Type { get; set; } = "Consultation";

    public decimal Price { get; set; }
    public int Quantity { get; set; }

    /// <summary>Computed: Price * Quantity. Persisted for query performance.</summary>
    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = default!;
}
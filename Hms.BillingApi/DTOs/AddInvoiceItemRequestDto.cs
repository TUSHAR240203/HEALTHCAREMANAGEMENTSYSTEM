namespace Hms.BillingApi.DTOs.Billing;

public class AddInvoiceItemRequestDto
{
    /// <summary>Must reference an active ServiceCatalog entry. Price is fetched from catalog — never from this request.</summary>
    public int ServiceId { get; set; }

    public int Quantity { get; set; }
}
namespace Hms.BillingApi.DTOs.Billing;

public class InvoiceItemResponseDto
{
    public int Id { get; set; }
    public string ServiceName { get; set; } = default!;

    public decimal Price { get; set; }   // ✅ FIXED
    public int Quantity { get; set; }    // ✅ ADDED
}
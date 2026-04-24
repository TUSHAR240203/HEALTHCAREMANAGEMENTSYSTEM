namespace Hms.BillingApi.DTOs.Billing;

public class AddInvoiceItemRequestDto
{
    public string ServiceName { get; set; } = default!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
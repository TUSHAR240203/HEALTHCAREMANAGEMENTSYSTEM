namespace Hms.BillingApi.DTOs.Billing;

public class AddInvoiceItemRequestDto
{
    public string ServiceName { get; set; } = default!;
    public decimal Amount { get; set; }
}
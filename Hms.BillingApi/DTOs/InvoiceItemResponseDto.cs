namespace Hms.BillingApi.DTOs.Billing;

public class InvoiceItemResponseDto
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = default!;
    public string Type { get; set; } = default!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}
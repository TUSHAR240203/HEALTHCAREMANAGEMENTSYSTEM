namespace Hms.ReceptionApi.DTOs.Reception;

public class AddInvoiceItemRequestDto
{
    public string ServiceName { get; set; } = default!;
    public decimal Amount { get; set; }
}
namespace Hms.ReceptionApi.DTOs.Reception;

public class InvoiceItemResponseDto
{
    public int Id { get; set; }
    public string ServiceName { get; set; } = default!;
    public decimal Amount { get; set; }
}
namespace Hms.BillingApi.DTOs.Billing;

public class InvoiceResponseDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string UHID { get; set; } = default!;
    public int AppointmentId { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public string Status { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; }

    public List<InvoiceItemResponseDto> Items { get; set; } = new();
    public List<PaymentResponseDto> Payments { get; set; } = new();
}


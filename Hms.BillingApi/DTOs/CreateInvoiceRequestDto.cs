namespace Hms.BillingApi.DTOs.Billing;

public class CreateInvoiceRequestDto
{
    public int PatientId { get; set; }
    public string UHID { get; set; } = default!;
    public int AppointmentId { get; set; }
    public decimal ConsultationFee { get; set; }

    // ✅ added
    public List<AddInvoiceItemRequestDto> Items { get; set; } = new();
}
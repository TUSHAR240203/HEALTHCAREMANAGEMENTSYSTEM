namespace Hms.ReceptionApi.DTOs.Reception;

public class CreateInvoiceRequestDto
{
    public int PatientId { get; set; }
    public int AppointmentId { get; set; }
    public decimal ConsultationFee { get; set; }
}
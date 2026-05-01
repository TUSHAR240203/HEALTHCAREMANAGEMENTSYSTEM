namespace Hms.BillingApi.DTOs.Billing;

/// <summary>
/// Payload sent by AppointmentsApi when an appointment is marked Completed.
/// ConsultationFee is NOT included here — it is fetched from DoctorsApi inside BillingService.
/// </summary>
public class CreateFromAppointmentRequestDto
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string UHID { get; set; } = default!;
    public int DoctorId { get; set; }
}

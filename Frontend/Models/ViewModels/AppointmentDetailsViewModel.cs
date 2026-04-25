using Frontend.Models.Api;
using Frontend.Models.Doctors;
using Frontend.Models.Patients;

namespace Frontend.Models.ViewModels;

public class AppointmentDetailsViewModel
{
    public AppointmentResponseDto Appointment { get; set; } = new();
    public DoctorResponseDto? Doctor { get; set; }
    public PatientResponseDto? Patient { get; set; }
    public RescheduleAppointmentRequestDto Reschedule { get; set; } = new();
    public string SelectedSlot { get; set; } = string.Empty;
    public List<PatientSlotOption> FreeSlots { get; set; } = new();
    public CancelAppointmentRequestDto Cancel { get; set; } = new();
    public CompleteAppointmentRequestDto Complete { get; set; } = new();
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
}

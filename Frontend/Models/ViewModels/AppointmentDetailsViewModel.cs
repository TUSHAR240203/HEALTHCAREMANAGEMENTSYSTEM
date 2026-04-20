using Frontend.Models.Api;
//using Hms.AppointmentsMvc.Models.Api;

namespace Frontend.Models.ViewModels;

public class AppointmentDetailsViewModel
{
    public AppointmentResponseDto Appointment { get; set; } = new();
    public RescheduleAppointmentRequestDto Reschedule { get; set; } = new();
    public CancelAppointmentRequestDto Cancel { get; set; } = new();
    public CompleteAppointmentRequestDto Complete { get; set; } = new();
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
}
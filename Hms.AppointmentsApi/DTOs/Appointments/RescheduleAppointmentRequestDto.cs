namespace Hms.AppointmentsApi.DTOs.Appointments;

public class RescheduleAppointmentRequestDto
{
    public DateOnly NewAppointmentDate { get; set; }
    public TimeOnly NewSlotStartTime { get; set; }
    public TimeOnly NewSlotEndTime { get; set; }
    public string? Reason { get; set; }
}
using Hms.AppointmentsApi.Enums;

namespace Hms.AppointmentsApi.DTOs.Appointments;

public class AppointmentSearchRequestDto
{
    public int? PatientId { get; set; }
    public int? DoctorId { get; set; }
    public int? DepartmentId { get; set; }
    public DateOnly? AppointmentDate { get; set; }
    public AppointmentStatus? Status { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
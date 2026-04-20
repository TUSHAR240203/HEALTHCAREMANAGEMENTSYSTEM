using System.ComponentModel.DataAnnotations;

namespace Frontend.Models.Api;

public class CreateAppointmentRequestDto
{
    [Required]
    public int PatientId { get; set; }

    [Required]
    public string UHID { get; set; } = string.Empty;

    [Required]
    public int DoctorId { get; set; }

    public string? DoctorName { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    public string? DepartmentName { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateOnly AppointmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required]
    [DataType(DataType.Time)]
    public TimeOnly SlotStartTime { get; set; } = new(9, 0);

    [Required]
    [DataType(DataType.Time)]
    public TimeOnly SlotEndTime { get; set; } = new(9, 30);

    [Required]
    public string VisitType { get; set; } = string.Empty;

    public string? ReasonForVisit { get; set; }
    public bool IsTeleConsultation { get; set; }
}
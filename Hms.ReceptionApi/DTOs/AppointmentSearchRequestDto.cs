namespace Hms.ReceptionApi.DTOs;

public class AppointmentSearchRequestDto
{
    public int? PatientId { get; set; }

    public int? DoctorId { get; set; }

    public int? DepartmentId { get; set; }

    public DateOnly? AppointmentDate { get; set; }

    public string? Status { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 100;
}

public class AppointmentSearchResponseDto
{
    public List<AppointmentSummaryDto> Appointments { get; set; } = new();

    public int TotalCount { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }
}

public class AppointmentSummaryDto
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public string? UHID { get; set; }

    public string? PatientName { get; set; }

    public int DoctorId { get; set; }

    public string? DoctorName { get; set; }

    public int DepartmentId { get; set; }

    public string? DepartmentName { get; set; }

    public DateOnly AppointmentDate { get; set; }

    public TimeOnly SlotStartTime { get; set; }

    public TimeOnly SlotEndTime { get; set; }

    public string? Status { get; set; }
}
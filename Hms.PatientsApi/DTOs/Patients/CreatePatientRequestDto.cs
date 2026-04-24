using Hms.PatientsApi.Enums;

namespace Hms.PatientsApi.DTOs.Patients;

public class CreatePatientRequestDto
{
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string MobileNumber { get; set; } = default!;
    public string? Email { get; set; }
    public bool PortalAccessEnabled { get; set; }
}
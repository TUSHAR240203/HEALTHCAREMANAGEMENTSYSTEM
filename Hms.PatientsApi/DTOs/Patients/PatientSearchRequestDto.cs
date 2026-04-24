namespace Hms.PatientsApi.DTOs.Patients;

public class PatientSearchRequestDto
{
    public string? UHID { get; set; }
    public string? MobileNumber { get; set; }
    public string? Name { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
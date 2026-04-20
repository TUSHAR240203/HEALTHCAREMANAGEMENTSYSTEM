namespace Hms.PatientsApi.DTOs.Patients;

public class PatientSearchResponseDto
{
    public int TotalCount { get; set; }
    public List<PatientResponseDto> Patients { get; set; } = new();
}
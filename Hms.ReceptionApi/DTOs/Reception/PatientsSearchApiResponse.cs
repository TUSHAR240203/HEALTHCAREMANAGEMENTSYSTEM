namespace Hms.ReceptionApi.DTOs.Reception;

public class PatientsSearchApiResponse
{
    public int TotalCount { get; set; }
    public List<PatientApiResponse> Patients { get; set; } = new();
}
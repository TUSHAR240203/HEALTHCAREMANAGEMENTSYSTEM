namespace Hms.ReceptionApi.DTOs.Reception;

public class ReceptionPatientSearchResponseDto
{
    public int TotalCount { get; set; }
    public List<ReceptionPatientSummaryDto> Patients { get; set; } = new();
}
public class PatientSearchResponseDto
{
    public List<PatientApiResponse> Patients { get; set; } = new();
}
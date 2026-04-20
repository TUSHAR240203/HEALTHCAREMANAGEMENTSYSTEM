namespace Hms.ReceptionApi.DTOs.Reception;

public class ReceptionPatientSearchResponseDto
{
    public int TotalCount { get; set; }
    public List<ReceptionPatientSummaryDto> Patients { get; set; } = new();
}
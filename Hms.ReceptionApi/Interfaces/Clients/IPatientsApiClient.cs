using Hms.ReceptionApi.DTOs.Reception;

namespace Hms.ReceptionApi.Interfaces.Clients;

public interface IPatientsApiClient
{
    Task<ReceptionPatientSearchResponseDto> SearchPatientsAsync(ReceptionPatientSearchRequestDto request);
    Task<ReceptionPatientSummaryDto?> GetPatientSummaryAsync(int patientId);
    Task<ReceptionPatientSummaryDto> RegisterPatientAsync(RegisterPatientByReceptionRequestDto request);
}
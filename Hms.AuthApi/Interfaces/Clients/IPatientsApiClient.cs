using Hms.AuthApi.Clients;

namespace Hms.AuthApi.Interfaces.Clients;

public interface IPatientsApiClient
{
    Task<PatientApiResponse?> GetPatientByIdAsync(int patientId);
}
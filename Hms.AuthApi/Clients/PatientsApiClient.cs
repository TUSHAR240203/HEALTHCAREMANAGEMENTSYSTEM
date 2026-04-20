using System.Net;
using System.Net.Http.Json;
using Hms.AuthApi.Interfaces.Clients;

namespace Hms.AuthApi.Clients;

public class PatientsApiClient : IPatientsApiClient
{
    private readonly HttpClient _httpClient;

    public PatientsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PatientApiResponse?> GetPatientByIdAsync(int patientId)
    {
        var response = await _httpClient.GetAsync($"/api/patients/{patientId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to fetch patient. Details: {error}");
        }

        return await response.Content.ReadFromJsonAsync<PatientApiResponse>();
    }
}
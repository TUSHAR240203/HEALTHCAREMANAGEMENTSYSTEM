using System.Net.Http.Json;
using Hms.ReceptionApi.Interfaces.Clients;

namespace Hms.ReceptionApi.Clients;

public class AuthApiClient : IAuthApiClient
{
    private readonly HttpClient _httpClient;

    public AuthApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendPortalActivationAsync(int patientId)
    {
        var request = new
        {
            patientId = patientId
        };

        var response = await _httpClient.PostAsJsonAsync("/api/auth/patient/send-portal-activation", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to send portal activation. Details: {error}");
        }
    }
}
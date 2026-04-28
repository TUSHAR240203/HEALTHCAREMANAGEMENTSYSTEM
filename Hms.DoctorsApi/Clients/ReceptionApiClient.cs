using Hms.DoctorsApi.DTOs.Queue;
using Hms.DoctorsApi.Interfaces.Clients;
using System.Net;
using System.Net.Http.Json;

namespace Hms.DoctorsApi.Clients;

public class ReceptionApiClient : IReceptionApiClient
{
    private readonly HttpClient _httpClient;

    public ReceptionApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DoctorQueueResponseDto?> GetDoctorQueueAsync(int doctorId, DateOnly date)
    {
        var response = await _httpClient.GetAsync($"/api/reception/queue/doctor/{doctorId}?date={date:yyyy-MM-dd}");
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to fetch doctor queue. Details: {error}");
        }

        return await response.Content.ReadFromJsonAsync<DoctorQueueResponseDto>();
    }

    public async Task<DoctorQueueCurrentResponseDto?> GetDoctorCurrentQueueAsync(int doctorId, DateOnly date)
    {
        var response = await _httpClient.GetAsync($"/api/reception/queue/doctor/{doctorId}/current?date={date:yyyy-MM-dd}");
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to fetch current doctor queue item. Details: {error}");
        }

        return await response.Content.ReadFromJsonAsync<DoctorQueueCurrentResponseDto>();
    }

    public async Task StartQueueTokenAsync(int queueTokenId)
    {
        var response = await _httpClient.PutAsync($"/api/reception/queue/token/{queueTokenId}/start", null);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to start queue token. Details: {error}");
        }
    }

    public async Task CompleteQueueTokenAsync(int queueTokenId, string? notes)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/reception/queue/token/{queueTokenId}/complete", new { notes });
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to complete queue token. Details: {error}");
        }
    }
}

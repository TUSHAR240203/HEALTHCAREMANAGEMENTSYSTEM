using Hms.ReceptionApi.DTOs.Doctors;
using Hms.ReceptionApi.Interfaces.Clients;
using System.Net;
using System.Net.Http.Json;

namespace Hms.ReceptionApi.Clients;

public class DoctorsApiClient : IDoctorsApiClient
{
    private readonly HttpClient _httpClient;

    public DoctorsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<DoctorSummaryDto>> SearchDoctorsAsync(DoctorSearchRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/doctors/search", request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to search doctors. Details: {error}");
        }
        return await response.Content.ReadFromJsonAsync<List<DoctorSummaryDto>>() ?? new List<DoctorSummaryDto>();
    }

    public async Task<DoctorSummaryDto?> GetDoctorByIdAsync(int doctorId)
    {
        var response = await _httpClient.GetAsync($"/api/doctors/{doctorId}");
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to fetch doctor. Details: {error}");
        }
        return await response.Content.ReadFromJsonAsync<DoctorSummaryDto>();
    }

    public async Task<DoctorAvailabilityResponseDto?> GetAvailableSlotsAsync(int doctorId, DateOnly date, bool isTeleConsultation)
    {
        var response = await _httpClient.GetAsync($"/api/doctors/{doctorId}/available-slots?date={date:yyyy-MM-dd}&isTeleConsultation={isTeleConsultation.ToString().ToLowerInvariant()}");
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to fetch doctor availability. Details: {error}");
        }
        return await response.Content.ReadFromJsonAsync<DoctorAvailabilityResponseDto>();
    }
}

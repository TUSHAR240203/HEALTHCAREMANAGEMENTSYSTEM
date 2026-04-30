using System.Net.Http.Json;
using Hms.AppointmentsApi.DTOs.Doctors;
using Hms.AppointmentsApi.Interfaces.Clients;

namespace Hms.AppointmentsApi.Clients;

public class DoctorsApiClient : IDoctorsApiClient
{
    private readonly HttpClient _httpClient;

    public DoctorsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DoctorAvailabilityResponseDto?> GetAvailabilityAsync(
        int doctorId,
        DateOnly date,
        bool? isTeleConsultation)
    {
        var url = $"api/doctors/{doctorId}/available-slots?date={date:yyyy-MM-dd}";

        if (isTeleConsultation.HasValue)
            url += $"&isTeleConsultation={isTeleConsultation.Value.ToString().ToLowerInvariant()}";

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return null;

        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<DoctorAvailabilityResponseDto>>();

        return envelope?.Data;
    }

    private sealed class ApiEnvelope<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
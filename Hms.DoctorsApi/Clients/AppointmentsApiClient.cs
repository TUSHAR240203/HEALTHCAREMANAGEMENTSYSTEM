using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hms.DoctorsApi.DTOs.Appointments;
using Hms.DoctorsApi.Interfaces.Clients;

namespace Hms.DoctorsApi.Clients;

public class AppointmentsApiClient : IAppointmentsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public AppointmentsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;

        _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task<List<AppointmentResponseDto>> GetByDoctorIdAsync(int doctorId)
    {
        var response = await _httpClient.GetAsync($"api/appointments/doctor/{doctorId}");

        if (!response.IsSuccessStatusCode)
            return new List<AppointmentResponseDto>();

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<List<AppointmentResponseDto>>>(_jsonOptions);

        return envelope?.Data ?? new List<AppointmentResponseDto>();
    }

    public async Task<AppointmentResponseDto?> StartAppointmentAsync(int appointmentId)
    {
        var response = await _httpClient.PostAsync($"api/appointments/{appointmentId}/start", null);

        if (!response.IsSuccessStatusCode)
            return null;

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<AppointmentResponseDto>>(_jsonOptions);

        return envelope?.Data;
    }

    public async Task<AppointmentResponseDto?> CompleteAppointmentAsync(
        int appointmentId,
        CompleteAppointmentRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"api/appointments/{appointmentId}/complete",
            request,
            _jsonOptions);

        if (!response.IsSuccessStatusCode)
            return null;

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<AppointmentResponseDto>>(_jsonOptions);

        return envelope?.Data;
    }

    public async Task<AppointmentResponseDto?> AddAppointmentNotesAsync(
        int appointmentId,
        UpdateAppointmentNotesRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/appointments/{appointmentId}/notes",
            request,
            _jsonOptions);

        if (!response.IsSuccessStatusCode)
            return null;

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<AppointmentResponseDto>>(_jsonOptions);

        return envelope?.Data;
    }

    private sealed class ApiEnvelope<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public object? Errors { get; set; }
    }
}
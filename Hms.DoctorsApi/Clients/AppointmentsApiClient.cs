using Hms.DoctorsApi.DTOs.Appointments;
using Hms.DoctorsApi.Interfaces.Clients;
using System.Net;
using System.Net.Http.Json;

namespace Hms.DoctorsApi.Clients;

public class AppointmentsApiClient : IAppointmentsApiClient
{
    private readonly HttpClient _httpClient;

    public AppointmentsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<AppointmentResponseDto>> GetByDoctorIdAsync(int doctorId)
    {
        var response = await _httpClient.GetAsync($"/api/appointments/doctor/{doctorId}");
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to fetch doctor appointments. Details: {error}");
        }

        return await response.Content.ReadFromJsonAsync<List<AppointmentResponseDto>>() ?? new List<AppointmentResponseDto>();
    }

    public async Task<AppointmentResponseDto?> StartAppointmentAsync(int appointmentId)
    {
        var response = await _httpClient.PutAsync($"/api/appointments/{appointmentId}/start", null);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to start appointment. Details: {error}");
        }

        return await response.Content.ReadFromJsonAsync<AppointmentResponseDto>();
    }

    public async Task<AppointmentResponseDto?> CompleteAppointmentAsync(int appointmentId, CompleteAppointmentRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/appointments/{appointmentId}/complete", request);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to complete appointment. Details: {error}");
        }

        return await response.Content.ReadFromJsonAsync<AppointmentResponseDto>();
    }

    public async Task<AppointmentResponseDto?> AddAppointmentNotesAsync(int appointmentId, UpdateAppointmentNotesRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/appointments/{appointmentId}/notes", request);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to update appointment notes. Details: {error}");
        }

        return await response.Content.ReadFromJsonAsync<AppointmentResponseDto>();
    }
}

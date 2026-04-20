using Hms.ReceptionApi.DTOs;
//using Hms.ReceptionApi.DTOs.Clients;
using Hms.ReceptionApi.DTOs.Reception;
using Hms.ReceptionApi.Interfaces.Clients;
using System.Net.Http.Json;

namespace Hms.ReceptionApi.Clients;

public class AppointmentsApiClient : IAppointmentsApiClient
{
    private readonly HttpClient _httpClient;

    public AppointmentsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BookAppointmentResponseDto> BookAppointmentAsync(AppointmentCreateRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/appointments", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to book appointment. Details: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<BookAppointmentResponseDto>();

        return result ?? throw new InvalidOperationException("Unable to parse appointment booking response.");
    }

    public async Task<BookAppointmentResponseDto> RescheduleAppointmentAsync(int appointmentId, RescheduleAppointmentRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/appointments/{appointmentId}/reschedule", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to reschedule appointment. Details: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<BookAppointmentResponseDto>();

        return result ?? throw new InvalidOperationException("Unable to parse appointment reschedule response.");
    }

    public async Task<BookAppointmentResponseDto> CancelAppointmentAsync(int appointmentId, CancelAppointmentRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/appointments/{appointmentId}/cancel", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to cancel appointment. Details: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<BookAppointmentResponseDto>();

        return result ?? throw new InvalidOperationException("Unable to parse appointment cancel response.");
    }
}
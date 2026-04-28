using Hms.ReceptionApi.DTOs;
using Hms.ReceptionApi.DTOs.Common;
//using Hms.ReceptionApi.DTOs.Appointments;
using Hms.ReceptionApi.DTOs.Reception;
using Hms.ReceptionApi.Interfaces.Clients;
using System.Net.Http.Json;
using System.Text.Json;

namespace Hms.ReceptionApi.Clients;

public class AppointmentsApiClient : IAppointmentsApiClient
{
    private readonly HttpClient _httpClient;

    public AppointmentsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BookAppointmentResponseDto> BookAppointmentAsync(
        AppointmentCreateRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/api/appointments",
            request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Failed to book appointment. Details: {error}");
        }

        var wrapper =
            await response.Content.ReadFromJsonAsync<ApiResponse<BookAppointmentResponseDto>>();

        if (wrapper?.Data == null)
        {
            throw new InvalidOperationException(
                "Unable to parse appointment booking response.");
        }

        return wrapper.Data;
    }

    public async Task<BookAppointmentResponseDto> RescheduleAppointmentAsync(
        int appointmentId,
        RescheduleAppointmentRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"/api/appointments/{appointmentId}/reschedule",
            request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Failed to reschedule appointment. Details: {error}");
        }

        var wrapper =
            await response.Content.ReadFromJsonAsync<ApiResponse<BookAppointmentResponseDto>>();

        if (wrapper?.Data == null)
        {
            throw new InvalidOperationException(
                "Unable to parse appointment reschedule response.");
        }

        return wrapper.Data;
    }

    public async Task<BookAppointmentResponseDto> CancelAppointmentAsync(
        int appointmentId,
        CancelAppointmentRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"/api/appointments/{appointmentId}/cancel",
            request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Failed to cancel appointment. Details: {error}");
        }

        var wrapper =
            await response.Content.ReadFromJsonAsync<ApiResponse<BookAppointmentResponseDto>>();

        if (wrapper?.Data == null)
        {
            throw new InvalidOperationException(
                "Unable to parse appointment cancel response.");
        }

        return wrapper.Data;
    }

    public async Task<AppointmentSearchResponseDto> SearchAsync(AppointmentSearchRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/appointments/search",
            request);

        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Appointments API search failed. Status: {(int)response.StatusCode}. Body: {body}");
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return new AppointmentSearchResponseDto();
        }

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        using var document = System.Text.Json.JsonDocument.Parse(body);
        var root = document.RootElement;

        // Case 1: { success, message, data: { appointments: [...] } }
        if (root.TryGetProperty("data", out var data))
        {
            if (data.ValueKind == System.Text.Json.JsonValueKind.Null)
                return new AppointmentSearchResponseDto();

            return data.Deserialize<AppointmentSearchResponseDto>(options)
                   ?? new AppointmentSearchResponseDto();
        }

        // Case 2: { appointments: [...] }
        return root.Deserialize<AppointmentSearchResponseDto>(options)
               ?? new AppointmentSearchResponseDto();
    }
}
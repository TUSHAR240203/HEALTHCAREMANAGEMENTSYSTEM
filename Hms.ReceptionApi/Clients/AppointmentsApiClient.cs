using Hms.ReceptionApi.DTOs;
using Hms.ReceptionApi.DTOs.Common;
using Hms.ReceptionApi.DTOs.Reception;
using Hms.ReceptionApi.Interfaces.Clients;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Hms.ReceptionApi.Clients;

public class AppointmentsApiClient : IAppointmentsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AppointmentsApiClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<BookAppointmentResponseDto> BookAppointmentAsync(
        AppointmentCreateRequestDto request)
    {
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "api/appointments"
        );

        AddBearerToken(httpRequest);
        httpRequest.Content = JsonContent.Create(request);

        using var response = await _httpClient.SendAsync(httpRequest);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Failed to book appointment. Status: {(int)response.StatusCode}. Details: {error}");
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
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Put,
            $"api/appointments/{appointmentId}/reschedule"
        );

        AddBearerToken(httpRequest);
        httpRequest.Content = JsonContent.Create(request);

        using var response = await _httpClient.SendAsync(httpRequest);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Failed to reschedule appointment. Status: {(int)response.StatusCode}. Details: {error}");
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
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Put,
            $"api/appointments/{appointmentId}/cancel"
        );

        AddBearerToken(httpRequest);
        httpRequest.Content = JsonContent.Create(request);

        using var response = await _httpClient.SendAsync(httpRequest);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Failed to cancel appointment. Status: {(int)response.StatusCode}. Details: {error}");
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

    public async Task<AppointmentSearchResponseDto> SearchAsync(
        AppointmentSearchRequestDto request)
    {
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "api/appointments/search"
        );

        AddBearerToken(httpRequest);
        httpRequest.Content = JsonContent.Create(request);

        using var response = await _httpClient.SendAsync(httpRequest);

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

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        if (root.TryGetProperty("data", out var data))
        {
            if (data.ValueKind == JsonValueKind.Null)
                return new AppointmentSearchResponseDto();

            return data.Deserialize<AppointmentSearchResponseDto>(options)
                   ?? new AppointmentSearchResponseDto();
        }

        return root.Deserialize<AppointmentSearchResponseDto>(options)
               ?? new AppointmentSearchResponseDto();
    }

    private void AddBearerToken(HttpRequestMessage request)
    {
        var authHeader =
            _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authHeader))
            return;

        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader["Bearer ".Length..].Trim();

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hms.AppointmentsApi.Interfaces.Clients;

namespace Hms.AppointmentsApi.Clients;

public class BillingApiClient : IBillingApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BillingApiClient> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BillingApiClient(
        HttpClient httpClient,
        ILogger<BillingApiClient> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Sends a completed-appointment event to BillingApi.
    /// The BillingApi endpoint is idempotent, so retrying this call will not create duplicate invoices.
    /// Throws on failure so the outbox processor can retry instead of incorrectly marking the record processed.
    /// </summary>
    public async Task NotifyAppointmentCompletedAsync(
        int appointmentId,
        int patientId,
        string uhid,
        int doctorId)
    {
        var payload = new
        {
            AppointmentId = appointmentId,
            PatientId = patientId,
            UHID = uhid,
            DoctorId = doctorId
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/billing/create-from-appointment");

        AddBearerToken(request);

        request.Content = JsonContent.Create(payload);

        using var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();

            _logger.LogWarning(
                "BillingApi failed for AppointmentId={AppointmentId}. Status={StatusCode}. Body={Body}",
                appointmentId,
                response.StatusCode,
                body);

            throw new HttpRequestException(
                $"BillingApi invoice creation failed for AppointmentId={appointmentId}. " +
                $"Status={(int)response.StatusCode} {response.ReasonPhrase}. {body}");
        }

        _logger.LogInformation(
            "BillingApi created/returned invoice for AppointmentId={AppointmentId}",
            appointmentId);
    }

    private void AddBearerToken(HttpRequestMessage request)
    {
        var authHeader =
            _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authHeader))
            return;

        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return;

        var token = authHeader["Bearer ".Length..].Trim();

        if (string.IsNullOrWhiteSpace(token))
            return;

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
}
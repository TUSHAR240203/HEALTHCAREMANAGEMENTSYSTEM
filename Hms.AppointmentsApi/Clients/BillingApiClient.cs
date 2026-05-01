using System.Net.Http.Json;
using Hms.AppointmentsApi.Interfaces.Clients;

namespace Hms.AppointmentsApi.Clients;

public class BillingApiClient : IBillingApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BillingApiClient> _logger;

    public BillingApiClient(HttpClient httpClient, ILogger<BillingApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Fire-and-forget: POST to BillingApi to create invoice from appointment.
    /// Appointment completion is NEVER rolled back if this call fails.
    /// </summary>
    public async Task NotifyAppointmentCompletedAsync(int appointmentId, int patientId, string uhid, int doctorId)
    {
        try
        {
            var payload = new
            {
                AppointmentId = appointmentId,
                PatientId = patientId,
                UHID = uhid,
                DoctorId = doctorId
            };

            var response = await _httpClient.PostAsJsonAsync("api/billing/create-from-appointment", payload);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "BillingApi returned {StatusCode} for AppointmentId={AppointmentId}. Invoice may need manual creation.",
                    response.StatusCode, appointmentId);
            }
            else
            {
                _logger.LogInformation(
                    "BillingApi notified successfully for AppointmentId={AppointmentId}", appointmentId);
            }
        }
        catch (Exception ex)
        {
            // ⚠️ Fire-and-forget: log but never throw. Appointment is already saved.
            _logger.LogError(ex,
                "Failed to notify BillingApi for AppointmentId={AppointmentId}. Invoice will need to be created manually.",
                appointmentId);
        }
    }
}

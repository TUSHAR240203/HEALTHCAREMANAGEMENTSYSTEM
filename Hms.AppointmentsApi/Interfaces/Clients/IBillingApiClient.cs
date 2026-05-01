namespace Hms.AppointmentsApi.Interfaces.Clients;

/// <summary>
/// Outbound client for notifying BillingApi when an appointment is completed.
/// </summary>
public interface IBillingApiClient
{
    /// <summary>
    /// Fire-and-forget: tells BillingApi to create an invoice for the completed appointment.
    /// If the call fails, logs the error but does NOT throw.
    /// </summary>
    Task NotifyAppointmentCompletedAsync(int appointmentId, int patientId, string uhid, int doctorId);
}

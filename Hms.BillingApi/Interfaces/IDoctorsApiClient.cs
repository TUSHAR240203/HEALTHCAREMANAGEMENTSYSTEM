namespace Hms.BillingApi.Interfaces;

/// <summary>
/// Abstraction for fetching doctor data from Hms.DoctorsApi.
/// Only ConsultationFee is exposed — no other doctor data is needed by BillingApi.
/// </summary>
public interface IDoctorsApiClient
{
    /// <summary>
    /// Fetches the consultation fee for the given doctor.
    /// Returns null if the doctor is not found or the remote call fails.
    /// </summary>
    Task<decimal?> GetConsultationFeeAsync(int doctorId);
}

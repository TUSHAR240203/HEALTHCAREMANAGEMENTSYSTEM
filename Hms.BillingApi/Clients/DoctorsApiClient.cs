using System.Net.Http.Json;
using Hms.BillingApi.Interfaces;

namespace Hms.BillingApi.Clients;

public class DoctorsApiClient : IDoctorsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DoctorsApiClient> _logger;

    public DoctorsApiClient(HttpClient httpClient, ILogger<DoctorsApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<decimal?> GetConsultationFeeAsync(int doctorId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/doctors/{doctorId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("DoctorsApi returned {StatusCode} for doctorId={DoctorId}", response.StatusCode, doctorId);
                return null;
            }

            // ✅ FIX: direct DTO read (no envelope)
            var doctor = await response.Content.ReadFromJsonAsync<DoctorDto>();

            return doctor?.ConsultationFee;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch consultation fee for doctorId={DoctorId}", doctorId);
            return null;
        }
    }

    // ✅ NEW DTO (matches actual response)
    private sealed class DoctorDto
    {
        public int Id { get; set; }
        public decimal ConsultationFee { get; set; }
    }
}
using System.Net.Http.Headers;
using System.Text.Json;
using Hms.BillingApi.Interfaces;

namespace Hms.BillingApi.Services;

public class DoctorsApiClient : IDoctorsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<DoctorsApiClient> _logger;

    public DoctorsApiClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<DoctorsApiClient> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<decimal?> GetConsultationFeeAsync(int doctorId)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/doctors/{doctorId}"
        );

        AddBearerToken(request);

        using var response = await _httpClient.SendAsync(request);

        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "DoctorsApi fee lookup failed. DoctorId={DoctorId}, Status={Status}, Body={Body}",
                doctorId,
                (int)response.StatusCode,
                body);

            return null;
        }

        if (string.IsNullOrWhiteSpace(body))
            return null;

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        // Supports wrapped response: { success, data: { consultationFee: 650 } }
        if (root.TryGetProperty("data", out var data) && data.ValueKind != JsonValueKind.Null)
        {
            if (TryReadFee(data, out var wrappedFee))
                return wrappedFee;
        }

        // Supports raw response: { consultationFee: 650 }
        if (TryReadFee(root, out var fee))
            return fee;

        return null;
    }

    private static bool TryReadFee(JsonElement element, out decimal fee)
    {
        fee = 0;

        if (element.TryGetProperty("consultationFee", out var camelFee) &&
            camelFee.TryGetDecimal(out fee))
        {
            return true;
        }

        if (element.TryGetProperty("ConsultationFee", out var pascalFee) &&
            pascalFee.TryGetDecimal(out fee))
        {
            return true;
        }

        return false;
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

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
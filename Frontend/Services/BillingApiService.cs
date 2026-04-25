using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Frontend.Models.Billing;

namespace Frontend.Services
{
    public class BillingApiService : IBillingApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public BillingApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<InvoiceResponseDto?> CreateInvoiceAsync(CreateInvoiceRequestDto request)
        {
            return await PostAsync<CreateInvoiceRequestDto, InvoiceResponseDto>("gateway/billing/invoice", request);
        }

        public async Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int invoiceId)
        {
            return await GetAsync<InvoiceResponseDto>($"gateway/billing/invoice/{invoiceId}", true);
        }

        public async Task<List<InvoiceResponseDto>> GetInvoicesByPatientIdAsync(int patientId)
        {
            return await GetAsync<List<InvoiceResponseDto>>($"gateway/billing/patient/{patientId}/invoices")
                   ?? new List<InvoiceResponseDto>();
        }

        public async Task<InvoiceResponseDto?> AddInvoiceItemAsync(int invoiceId, AddInvoiceItemRequestDto request)
        {
            return await PostAsync<AddInvoiceItemRequestDto, InvoiceResponseDto>($"gateway/billing/{invoiceId}/item", request);
        }

        public async Task<InvoiceResponseDto?> AddPaymentAsync(int invoiceId, PaymentRequestDto request)
        {
            return await PostAsync<PaymentRequestDto, InvoiceResponseDto>($"gateway/billing/{invoiceId}/payment", request);
        }

        private async Task<TResponse?> GetAsync<TResponse>(string url, bool allowNotFound = false)
        {
            using var response = await _httpClient.GetAsync(url);
            return await ReadResponseAsync<TResponse>(response, allowNotFound);
        }

        private async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest request)
        {
            using var content = CreateJsonContent(request);
            using var response = await _httpClient.PostAsync(url, content);
            return await ReadResponseAsync<TResponse>(response, false);
        }

        private StringContent CreateJsonContent<TRequest>(TRequest request)
        {
            return new StringContent(JsonSerializer.Serialize(request, _jsonOptions), Encoding.UTF8, "application/json");
        }

        private async Task<TResponse?> ReadResponseAsync<TResponse>(HttpResponseMessage response, bool allowNotFound)
        {
            if (allowNotFound && response.StatusCode == HttpStatusCode.NotFound)
                return default;

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new ApiException(ExtractMessage(body, $"API request failed with status code {(int)response.StatusCode}."), (int)response.StatusCode);

            if (string.IsNullOrWhiteSpace(body))
                return default;

            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;
                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var data))
                    return data.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined ? default : data.Deserialize<TResponse>(_jsonOptions);
            }
            catch (JsonException)
            {
            }

            return JsonSerializer.Deserialize<TResponse>(body, _jsonOptions);
        }

        private static string ExtractMessage(string body, string fallback)
        {
            if (string.IsNullOrWhiteSpace(body)) return fallback;
            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;
                if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
                    return message.GetString() ?? fallback;
                if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                    return title.GetString() ?? fallback;
                if (root.TryGetProperty("errors", out var errors)) return errors.ToString();
            }
            catch { }
            return body;
        }
    }
}

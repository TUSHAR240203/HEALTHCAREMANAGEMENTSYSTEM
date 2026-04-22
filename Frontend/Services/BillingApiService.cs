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
            return await PostAsync<CreateInvoiceRequestDto, InvoiceResponseDto>(
                "gateway/billing/invoice", request);
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
            return await PostAsync<AddInvoiceItemRequestDto, InvoiceResponseDto>(
                $"gateway/billing/invoice/{invoiceId}/items", request);
        }

        public async Task<InvoiceResponseDto?> AddPaymentAsync(int invoiceId, PaymentRequestDto request)
        {
            return await PostAsync<PaymentRequestDto, InvoiceResponseDto>(
                $"gateway/billing/invoice/{invoiceId}/pay", request);
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
            return new StringContent(
                JsonSerializer.Serialize(request, _jsonOptions),
                Encoding.UTF8,
                "application/json");
        }

        private async Task<TResponse?> ReadResponseAsync<TResponse>(HttpResponseMessage response, bool allowNotFound)
        {
            if (allowNotFound && response.StatusCode == HttpStatusCode.NotFound)
                return default;

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(
                    string.IsNullOrWhiteSpace(body)
                        ? $"API request failed with status code {(int)response.StatusCode}."
                        : body,
                    (int)response.StatusCode);
            }

            if (string.IsNullOrWhiteSpace(body))
                return default;

            return JsonSerializer.Deserialize<TResponse>(body, _jsonOptions);
        }
    }
}

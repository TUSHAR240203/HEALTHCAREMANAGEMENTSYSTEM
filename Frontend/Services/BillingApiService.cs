using Frontend.Models.Api;
using Frontend.Models.Billing;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Frontend.Services
{
    public class BillingApiService : IBillingApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions;

        public BillingApiService(
            HttpClient httpClient,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;

            var baseUrl =
                configuration["ApiSettings:ApiGatewayBaseUrl"] ??
                configuration["ApiSettings:BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException(
                    "API Gateway base URL is missing. Add ApiSettings:ApiGatewayBaseUrl or ApiSettings:BaseUrl in appsettings.json.");
            }

            _httpClient.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<InvoiceResponseDto?> CreateInvoiceAsync(CreateInvoiceRequestDto request)
        {
            return await PostWrappedAsync<InvoiceResponseDto>("gateway/billing/invoice", request);
        }

        public async Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int invoiceId)
        {
            return await GetWrappedAsync<InvoiceResponseDto>($"gateway/billing/invoice/{invoiceId}");
        }

        public async Task<InvoiceResponseDto?> GetInvoiceByAppointmentIdAsync(int appointmentId)
        {
            return await GetWrappedAsync<InvoiceResponseDto>(
                $"gateway/billing/appointment/{appointmentId}/invoice");
        }

        public async Task<List<InvoiceResponseDto>> GetInvoicesByPatientIdAsync(int patientId)
        {
            return await GetWrappedAsync<List<InvoiceResponseDto>>(
                       $"gateway/billing/patient/{patientId}/invoices")
                   ?? new List<InvoiceResponseDto>();
        }

        public async Task<InvoiceResponseDto?> AddInvoiceItemAsync(
            int invoiceId,
            AddInvoiceItemRequestDto request)
        {
            return await PostWrappedAsync<InvoiceResponseDto>(
                $"gateway/billing/{invoiceId}/item",
                request);
        }

        public async Task<InvoiceResponseDto?> AddPaymentAsync(
            int invoiceId,
            PaymentRequestDto request)
        {
            return await PostWrappedAsync<InvoiceResponseDto>(
                $"gateway/billing/{invoiceId}/payment",
                request);
        }

        public async Task<List<ServiceCatalogResponseDto>> GetServiceCatalogAsync()
        {
            return await GetWrappedAsync<List<ServiceCatalogResponseDto>>("gateway/billing/services")
                   ?? new List<ServiceCatalogResponseDto>();
        }

        public async Task<FinanceSummaryDto?> GetFinanceSummaryAsync()
        {
            return await GetRawAsync<FinanceSummaryDto>("gateway/finance/summary");
        }

        public async Task<PagedResultDto<InvoiceResponseDto>> GetFinanceInvoicesAsync(
            int pageNumber = 1,
            int pageSize = 50)
        {
            var result = await GetRawAsync<PagedResultDto<InvoiceResponseDto>>(
                $"gateway/finance/invoices?pageNumber={pageNumber}&pageSize={pageSize}");

            return result ?? new PagedResultDto<InvoiceResponseDto>
            {
                Items = new List<InvoiceResponseDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 0,
                TotalPages = 0
            };
        }

        private async Task<T?> GetWrappedAsync<T>(string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            AddBearerToken(request);

            using var response = await _httpClient.SendAsync(request);
            return await ReadWrappedResponseAsync<T>(response);
        }

        private async Task<T?> PostWrappedAsync<T>(string url, object body)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            AddBearerToken(request);

            var json = JsonSerializer.Serialize(body, _jsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request);
            return await ReadWrappedResponseAsync<T>(response);
        }

        private async Task<T?> GetRawAsync<T>(string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            AddBearerToken(request);

            using var response = await _httpClient.SendAsync(request);
            return await ReadRawResponseAsync<T>(response);
        }

        private async Task<T?> ReadWrappedResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            ThrowIfFailed(response, content);

            if (string.IsNullOrWhiteSpace(content))
                return default;

            var wrapped = JsonSerializer.Deserialize<ApiResponseDto<T>>(content, _jsonOptions);

            if (wrapped != null)
                return wrapped.Data;

            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }

        private async Task<T?> ReadRawResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            ThrowIfFailed(response, content);

            if (string.IsNullOrWhiteSpace(content))
                return default;

            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }

        private static void ThrowIfFailed(HttpResponseMessage response, string content)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new ApiException(
                    "Gateway rejected the request. Your login token is missing or expired. Please logout and login again.",
                    (int)response.StatusCode);
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new ApiException(
                    "You are not authorized to access this Billing/Finance API. Admin or Receptionist role is required.",
                    (int)response.StatusCode);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(
                    string.IsNullOrWhiteSpace(content)
                        ? $"Gateway request failed with status {(int)response.StatusCode}."
                        : content,
                    (int)response.StatusCode);
            }
        }

        private void AddBearerToken(HttpRequestMessage request)
        {
            var session = _httpContextAccessor.HttpContext?.Session;

            var token =
                session?.GetString("JwtToken") ??
                session?.GetString("AccessToken") ??
                session?.GetString("Token") ??
                session?.GetString("AuthToken") ??
                session?.GetString("BearerToken");

            if (string.IsNullOrWhiteSpace(token))
                return;

            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = token["Bearer ".Length..];

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
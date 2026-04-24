using System.Net;
using System.Net.Http.Json;
using Hms.ReceptionApi.DTOs.Reception;
using Hms.ReceptionApi.Interfaces.Clients;

namespace Hms.ReceptionApi.Clients;

public class BillingApiClient : IBillingApiClient
{
    private readonly HttpClient _httpClient;

    public BillingApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<InvoiceResponseDto> CreateInvoiceAsync(object request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/billing/invoice", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to create invoice. Details: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<InvoiceResponseDto>();
        return result ?? throw new InvalidOperationException("Unable to parse invoice create response.");
    }

    public async Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int invoiceId)
    {
        var response = await _httpClient.GetAsync($"/api/billing/invoice/{invoiceId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to fetch invoice. Details: {error}");
        }

        return await response.Content.ReadFromJsonAsync<InvoiceResponseDto>();
    }

    public async Task<List<InvoiceResponseDto>> GetInvoicesByPatientIdAsync(int patientId)
    {
        var response = await _httpClient.GetAsync($"/api/billing/patient/{patientId}/invoices");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to fetch patient invoices. Details: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<List<InvoiceResponseDto>>();
        return result ?? new List<InvoiceResponseDto>();
    }

    public async Task<InvoiceResponseDto> AddInvoiceItemAsync(int invoiceId, AddInvoiceItemRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync($"/api/billing/invoice/{invoiceId}/items", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to add invoice item. Details: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<InvoiceResponseDto>();
        return result ?? throw new InvalidOperationException("Unable to parse add item response.");
    }

    public async Task<InvoiceResponseDto> AddPaymentAsync(int invoiceId, PaymentRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync($"/api/billing/invoice/{invoiceId}/pay", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to add payment. Details: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<InvoiceResponseDto>();
        return result ?? throw new InvalidOperationException("Unable to parse payment response.");
    }
}
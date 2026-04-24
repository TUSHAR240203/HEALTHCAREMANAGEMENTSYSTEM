using Hms.BillingApi.DTOs.Billing;

namespace Hms.BillingApi.Interfaces.Services;

public interface IBillingService
{
    Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceRequestDto request);
    Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int invoiceId);
    Task<List<InvoiceResponseDto>> GetInvoicesByPatientIdAsync(int patientId);
    Task<InvoiceResponseDto?> AddInvoiceItemAsync(int invoiceId, AddInvoiceItemRequestDto request);
    Task<InvoiceResponseDto?> AddPaymentAsync(int invoiceId, PaymentRequestDto request);
}
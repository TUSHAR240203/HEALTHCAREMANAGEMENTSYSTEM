using Hms.BillingApi.DTOs.Billing;

namespace Hms.BillingApi.Interfaces;

public interface IBillingService
{
    Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceRequestDto dto);

    Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int invoiceId);

    Task<List<InvoiceResponseDto>> GetInvoicesByPatientIdAsync(int patientId);

    Task<InvoiceResponseDto> AddInvoiceItemAsync(int invoiceId, AddInvoiceItemRequestDto dto);

    Task<InvoiceResponseDto> AddPaymentAsync(int invoiceId, PaymentRequestDto dto);
}
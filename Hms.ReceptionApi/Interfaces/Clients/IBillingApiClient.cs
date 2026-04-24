using Hms.ReceptionApi.DTOs.Reception;

namespace Hms.ReceptionApi.Interfaces.Clients;

public interface IBillingApiClient
{
    Task<InvoiceResponseDto> CreateInvoiceAsync(object request);
    Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int invoiceId);
    Task<List<InvoiceResponseDto>> GetInvoicesByPatientIdAsync(int patientId);
    Task<InvoiceResponseDto> AddInvoiceItemAsync(int invoiceId, AddInvoiceItemRequestDto request);
    Task<InvoiceResponseDto> AddPaymentAsync(int invoiceId, PaymentRequestDto request);
}
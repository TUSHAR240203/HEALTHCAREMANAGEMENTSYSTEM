using Frontend.Models.Billing;

namespace Frontend.Services
{
    public interface IBillingApiService
    {
        Task<InvoiceResponseDto?> CreateInvoiceAsync(CreateInvoiceRequestDto request);
        Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int invoiceId);
        Task<List<InvoiceResponseDto>> GetInvoicesByPatientIdAsync(int patientId);
        Task<InvoiceResponseDto?> AddInvoiceItemAsync(int invoiceId, AddInvoiceItemRequestDto request);
        Task<InvoiceResponseDto?> AddPaymentAsync(int invoiceId, PaymentRequestDto request);
    }
}

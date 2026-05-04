using Frontend.Models.Billing;

namespace Frontend.Services
{
    public interface IBillingApiService
    {
        Task<InvoiceResponseDto?> CreateInvoiceAsync(CreateInvoiceRequestDto request);
        Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int invoiceId);
        Task<InvoiceResponseDto?> GetInvoiceByAppointmentIdAsync(int appointmentId);
        Task<List<InvoiceResponseDto>> GetInvoicesByPatientIdAsync(int patientId);
        Task<InvoiceResponseDto?> AddInvoiceItemAsync(int invoiceId, AddInvoiceItemRequestDto request);
        Task<InvoiceResponseDto?> AddPaymentAsync(int invoiceId, PaymentRequestDto request);
        Task<List<ServiceCatalogResponseDto>> GetServiceCatalogAsync();

        Task<FinanceSummaryDto?> GetFinanceSummaryAsync();
        Task<PagedResultDto<InvoiceResponseDto>> GetFinanceInvoicesAsync(int pageNumber = 1, int pageSize = 50);
    }
}
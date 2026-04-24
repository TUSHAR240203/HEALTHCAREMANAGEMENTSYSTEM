using Hms.BillingApi.Entities;

namespace Hms.BillingApi.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice> CreateInvoiceAsync(Invoice invoice);

    Task<Invoice?> GetInvoiceByIdAsync(int invoiceId);

    Task<List<Invoice>> GetInvoicesByPatientIdAsync(int patientId);

    Task<Invoice> AddInvoiceItemAsync(int invoiceId, InvoiceItem item);

    Task<Invoice> AddPaymentAsync(int invoiceId, Payment payment);

    Task UpdateInvoiceAsync(Invoice invoice);
}

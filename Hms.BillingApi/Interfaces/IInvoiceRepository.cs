using Hms.BillingApi.Entities;

namespace Hms.BillingApi.Interfaces.Repository;

public interface IInvoiceRepository
{
    Task AddInvoiceAsync(Invoice invoice);
    Task<Invoice?> GetInvoiceByIdAsync(int invoiceId);
    Task<List<Invoice>> GetInvoicesByPatientIdAsync(int patientId);
    Task AddInvoiceItemAsync(InvoiceItem item);
    Task AddPaymentAsync(Payment payment);
    Task UpdateInvoiceAsync(Invoice invoice);
    Task SaveChangesAsync();
}
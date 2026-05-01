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

    /// <summary>Returns the invoice linked to this appointment, or null if none exists yet.</summary>
    Task<Invoice?> GetByAppointmentIdAsync(int appointmentId);

    /// <summary>Returns the latest open (IsClosed = false) invoice for the patient. Used for no-appointment item adds.</summary>
    Task<Invoice?> GetActiveInvoiceByPatientIdAsync(int patientId);
}

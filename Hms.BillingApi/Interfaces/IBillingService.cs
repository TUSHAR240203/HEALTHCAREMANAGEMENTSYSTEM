using Hms.BillingApi.DTOs.Billing;

namespace Hms.BillingApi.Interfaces;

public interface IBillingService
{
    Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceRequestDto dto);

    /// <summary>
    /// Called when an appointment is marked Completed.
    /// Fetches consultation fee from DoctorsApi and creates an invoice automatically.
    /// Idempotent — returns the existing invoice if one already exists for the appointment.
    /// </summary>
    Task<InvoiceResponseDto> CreateFromAppointmentAsync(CreateFromAppointmentRequestDto dto);

    Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int invoiceId);

    /// <summary>Returns the invoice generated for the completed appointment, or null if billing has not created it yet.</summary>
    Task<InvoiceResponseDto?> GetInvoiceByAppointmentIdAsync(int appointmentId);

    Task<List<InvoiceResponseDto>> GetInvoicesByPatientIdAsync(int patientId);

    Task<InvoiceResponseDto> AddInvoiceItemAsync(int invoiceId, AddInvoiceItemRequestDto dto);

    Task<InvoiceResponseDto> AddPaymentAsync(int invoiceId, PaymentRequestDto dto);

    /// <summary>
    /// Returns the latest open invoice for a patient (no-appointment use case).
    /// Returns null if no open invoice exists.
    /// </summary>
    Task<InvoiceResponseDto?> GetActiveInvoiceAsync(int patientId);

    /// <summary>Returns all active services from the catalog. Used by receptionist UI to pick a service.</summary>
    Task<List<ServiceCatalogResponseDto>> GetServiceCatalogAsync();
}

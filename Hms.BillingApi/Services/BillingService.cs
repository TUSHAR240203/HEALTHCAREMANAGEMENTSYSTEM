using Hms.BillingApi.DTOs.Billing;
using Hms.BillingApi.Entities;
using Hms.BillingApi.Interfaces.Repository;
using Hms.BillingApi.Interfaces.Services;

namespace Hms.BillingApi.Services;

public class BillingService : IBillingService
{
    private readonly IInvoiceRepository _invoiceRepository;

    public BillingService(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceRequestDto request)
    {
        ValidateCreateInvoice(request);

        var invoice = new Invoice
        {
            PatientId = request.PatientId,
            UHID = request.UHID.Trim(),
            AppointmentId = request.AppointmentId,
            TotalAmount = request.ConsultationFee,
            PaidAmount = 0,
            BalanceAmount = request.ConsultationFee,
            Status = request.ConsultationFee == 0 ? "Paid" : "Pending"
        };

        await _invoiceRepository.AddInvoiceAsync(invoice);
        await _invoiceRepository.SaveChangesAsync();

        if (request.ConsultationFee > 0)
        {
            var consultationItem = new InvoiceItem
            {
                InvoiceId = invoice.Id,
                ServiceName = "Consultation Fee",
                Amount = request.ConsultationFee
            };

            await _invoiceRepository.AddInvoiceItemAsync(consultationItem);
            await _invoiceRepository.SaveChangesAsync();
        }

        var saved = await _invoiceRepository.GetInvoiceByIdAsync(invoice.Id);
        return MapInvoice(saved!);
    }

    public async Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int invoiceId)
    {
        if (invoiceId <= 0)
            throw new ArgumentException("Invalid invoice id.");

        var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);
        return invoice == null ? null : MapInvoice(invoice);
    }

    public async Task<List<InvoiceResponseDto>> GetInvoicesByPatientIdAsync(int patientId)
    {
        if (patientId <= 0)
            throw new ArgumentException("Invalid patient id.");

        var invoices = await _invoiceRepository.GetInvoicesByPatientIdAsync(patientId);
        return invoices.Select(MapInvoice).ToList();
    }

    public async Task<InvoiceResponseDto?> AddInvoiceItemAsync(int invoiceId, AddInvoiceItemRequestDto request)
    {
        if (invoiceId <= 0)
            throw new ArgumentException("Invalid invoice id.");

        if (string.IsNullOrWhiteSpace(request.ServiceName))
            throw new ArgumentException("ServiceName is required.");

        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);
        if (invoice == null)
            return null;

        var item = new InvoiceItem
        {
            InvoiceId = invoiceId,
            ServiceName = request.ServiceName.Trim(),
            Amount = request.Amount
        };

        await _invoiceRepository.AddInvoiceItemAsync(item);

        invoice.TotalAmount += request.Amount;
        invoice.BalanceAmount = invoice.TotalAmount - invoice.PaidAmount;
        invoice.Status = invoice.BalanceAmount <= 0 ? "Paid" : "Pending";
        invoice.UpdatedAtUtc = DateTime.UtcNow;

        await _invoiceRepository.UpdateInvoiceAsync(invoice);
        await _invoiceRepository.SaveChangesAsync();

        var updated = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);
        return MapInvoice(updated!);
    }

    public async Task<InvoiceResponseDto?> AddPaymentAsync(int invoiceId, PaymentRequestDto request)
    {
        if (invoiceId <= 0)
            throw new ArgumentException("Invalid invoice id.");

        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.PaymentMode))
            throw new ArgumentException("PaymentMode is required.");

        var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);
        if (invoice == null)
            return null;

        if (invoice.Status == "Paid")
            throw new InvalidOperationException("Invoice is already fully paid.");

        var payment = new Payment
        {
            InvoiceId = invoiceId,
            Amount = request.Amount,
            PaymentMode = request.PaymentMode.Trim(),
            PaidAtUtc = DateTime.UtcNow
        };

        await _invoiceRepository.AddPaymentAsync(payment);

        invoice.PaidAmount += request.Amount;
        invoice.BalanceAmount = invoice.TotalAmount - invoice.PaidAmount;

        if (invoice.BalanceAmount <= 0)
        {
            invoice.BalanceAmount = 0;
            invoice.Status = "Paid";
        }
        else
        {
            invoice.Status = "Partial";
        }

        invoice.UpdatedAtUtc = DateTime.UtcNow;

        await _invoiceRepository.UpdateInvoiceAsync(invoice);
        await _invoiceRepository.SaveChangesAsync();

        var updated = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);
        return MapInvoice(updated!);
    }

    private static void ValidateCreateInvoice(CreateInvoiceRequestDto request)
    {
        if (request.PatientId <= 0)
            throw new ArgumentException("PatientId is required.");

        if (string.IsNullOrWhiteSpace(request.UHID))
            throw new ArgumentException("UHID is required.");

        if (request.AppointmentId <= 0)
            throw new ArgumentException("AppointmentId is required.");

        if (request.ConsultationFee < 0)
            throw new ArgumentException("ConsultationFee cannot be negative.");
    }

    private static InvoiceResponseDto MapInvoice(Invoice invoice)
    {
        return new InvoiceResponseDto
        {
            Id = invoice.Id,
            PatientId = invoice.PatientId,
            UHID = invoice.UHID,
            AppointmentId = invoice.AppointmentId,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.PaidAmount,
            BalanceAmount = invoice.BalanceAmount,
            Status = invoice.Status,
            CreatedAtUtc = invoice.CreatedAtUtc,
            Items = invoice.Items
                .Where(x => !x.IsDeleted)
                .Select(x => new InvoiceItemResponseDto
                {
                    Id = x.Id,
                    ServiceName = x.ServiceName,
                    Amount = x.Amount
                }).ToList(),
            Payments = invoice.Payments
                .Where(x => !x.IsDeleted)
                .Select(x => new PaymentResponseDto
                {
                    Id = x.Id,
                    Amount = x.Amount,
                    PaymentMode = x.PaymentMode,
                    PaidAtUtc = x.PaidAtUtc
                }).ToList()
        };
    }
}
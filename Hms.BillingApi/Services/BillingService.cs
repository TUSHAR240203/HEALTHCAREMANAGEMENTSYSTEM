using AutoMapper;
using Hms.BillingApi.DTOs.Billing;
using Hms.BillingApi.Entities;
using Hms.BillingApi.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hms.BillingApi.Services;

public class BillingService : IBillingService
{
    private readonly IInvoiceRepository _repo;
    private readonly IServiceCatalogRepository _catalogRepo;
    private readonly IMapper _mapper;
    private readonly IDoctorsApiClient _doctorsApiClient;
    private readonly ILogger<BillingService> _logger;

    public BillingService(
        IInvoiceRepository repo,
        IServiceCatalogRepository catalogRepo,
        IMapper mapper,
        IDoctorsApiClient doctorsApiClient,
        ILogger<BillingService> logger)
    {
        _repo = repo;
        _catalogRepo = catalogRepo;
        _mapper = mapper;
        _doctorsApiClient = doctorsApiClient;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CREATE FROM APPOINTMENT (main billing trigger)
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<InvoiceResponseDto> CreateFromAppointmentAsync(CreateFromAppointmentRequestDto dto)
    {
        // ── Idempotency: same appointment → same invoice ──────────────────────
        var existing = await _repo.GetByAppointmentIdAsync(dto.AppointmentId);
        if (existing != null)
        {
            _logger.LogInformation("Invoice already exists for AppointmentId={AppointmentId} → returning InvoiceId={InvoiceId}",
                dto.AppointmentId, existing.Id);
            return _mapper.Map<InvoiceResponseDto>(existing);
        }

        // ── Task 1: THROW if DoctorsApi unavailable or fee invalid ────────────
        var consultationFee = await _doctorsApiClient.GetConsultationFeeAsync(dto.DoctorId);
        if (consultationFee == null || consultationFee <= 0)
        {
            _logger.LogError(
                "Invoice creation aborted: could not fetch valid consultation fee for DoctorId={DoctorId}. Returned: {Fee}",
                dto.DoctorId, consultationFee);
            throw new InvalidOperationException(
                $"Cannot create invoice: consultation fee unavailable for DoctorId={dto.DoctorId}. " +
                "Ensure DoctorsApi is reachable and the doctor has a valid ConsultationFee configured.");
        }

        var consultationItem = new InvoiceItem
        {
            ServiceId = null,                    // Consultation items are not catalog-based
            ServiceName = "Consultation Fee",
            Type = "Consultation",
            Price = consultationFee.Value,
            Quantity = 1,
            Amount = consultationFee.Value,
            CreatedAt = DateTime.UtcNow
        };

        var invoice = new Invoice
        {
            PatientId = dto.PatientId,
            UHID = dto.UHID,
            AppointmentId = dto.AppointmentId,
            IsClosed = false,
            Status = "Pending",
            PaidAmount = 0,
            Items = new List<InvoiceItem> { consultationItem }
        };

        invoice.TotalAmount = invoice.Items.Sum(x => x.Amount);
        invoice.BalanceAmount = invoice.TotalAmount;

        // ── Task 3: handle unique constraint race condition at DB level ────────
        try
        {
            var result = await _repo.CreateInvoiceAsync(invoice);

            // ── Task 4: generate InvoiceNumber after save (uses DB Id) ─────────
            result.InvoiceNumber = $"INV-{DateTime.UtcNow.Year}-{result.Id:D4}";
            await _repo.UpdateInvoiceAsync(result);

            _logger.LogInformation("Invoice {InvoiceNumber} created for AppointmentId={AppointmentId}, Fee={Fee}",
                result.InvoiceNumber, dto.AppointmentId, consultationFee);

            return _mapper.Map<InvoiceResponseDto>(result);
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
        {
            // Another concurrent request already created an invoice for this appointment.
            _logger.LogWarning("Duplicate invoice race condition caught for AppointmentId={AppointmentId}. Returning existing.",
                dto.AppointmentId);
            var race = await _repo.GetByAppointmentIdAsync(dto.AppointmentId);
            return _mapper.Map<InvoiceResponseDto>(race!);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CREATE INVOICE (manual / legacy path)
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceRequestDto dto)
    {
        var invoice = _mapper.Map<Invoice>(dto);
        invoice.Items = _mapper.Map<List<InvoiceItem>>(dto.Items);

        foreach (var item in invoice.Items)
        {
            item.Amount = item.Price * item.Quantity;
            item.CreatedAt = DateTime.UtcNow;
        }

        invoice.TotalAmount = invoice.Items.Sum(x => x.Amount);
        invoice.PaidAmount = 0;
        invoice.BalanceAmount = invoice.TotalAmount;
        invoice.Status = "Pending";
        invoice.IsClosed = false;

        var result = await _repo.CreateInvoiceAsync(invoice);

        // ── Task 4: generate InvoiceNumber ────────────────────────────────────
        result.InvoiceNumber = $"INV-{DateTime.UtcNow.Year}-{result.Id:D4}";
        await _repo.UpdateInvoiceAsync(result);

        return _mapper.Map<InvoiceResponseDto>(result);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET BY ID
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int invoiceId)
    {
        var invoice = await _repo.GetInvoiceByIdAsync(invoiceId);
        return invoice == null ? null : _mapper.Map<InvoiceResponseDto>(invoice);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET BY PATIENT
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<List<InvoiceResponseDto>> GetInvoicesByPatientIdAsync(int patientId)
    {
        var invoices = await _repo.GetInvoicesByPatientIdAsync(patientId);
        return _mapper.Map<List<InvoiceResponseDto>>(invoices);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET ACTIVE INVOICE (no-appointment)
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<InvoiceResponseDto?> GetActiveInvoiceAsync(int patientId)
    {
        var invoice = await _repo.GetActiveInvoiceByPatientIdAsync(patientId);
        return invoice == null ? null : _mapper.Map<InvoiceResponseDto>(invoice);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET SERVICE CATALOG (Task 2)
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<List<ServiceCatalogResponseDto>> GetServiceCatalogAsync()
    {
        var services = await _catalogRepo.GetAllActiveAsync();
        return _mapper.Map<List<ServiceCatalogResponseDto>>(services);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ADD ITEM (Task 2 — price from ServiceCatalog, never from request)
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<InvoiceResponseDto> AddInvoiceItemAsync(int invoiceId, AddInvoiceItemRequestDto dto)
    {
        var invoice = await _repo.GetInvoiceByIdAsync(invoiceId);
        if (invoice == null)
            throw new InvalidOperationException("Invoice not found.");

        if (invoice.IsClosed)
            throw new InvalidOperationException("Cannot add items to a closed invoice.");

        // ── Fetch price from ServiceCatalog (NEVER from request) ─────────────
        var service = await _catalogRepo.GetByIdAsync(dto.ServiceId);
        if (service == null)
            throw new InvalidOperationException($"Service with Id={dto.ServiceId} not found or inactive in catalog.");

        var item = new InvoiceItem
        {
            ServiceId = service.Id,
            ServiceName = service.Name,
            Type = service.Type,
            Price = service.Price,                  // from catalog only
            Quantity = dto.Quantity,
            Amount = service.Price * dto.Quantity,  // calculated server-side
            CreatedAt = DateTime.UtcNow
        };

        var updated = await _repo.AddInvoiceItemAsync(invoiceId, item);

        updated.TotalAmount = updated.Items.Sum(x => x.Amount);
        updated.BalanceAmount = updated.TotalAmount - updated.PaidAmount;
        updated.Status = DeriveStatus(updated.PaidAmount, updated.TotalAmount);

        await _repo.UpdateInvoiceAsync(updated);
        return _mapper.Map<InvoiceResponseDto>(updated);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ADD PAYMENT
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<InvoiceResponseDto> AddPaymentAsync(int invoiceId, PaymentRequestDto dto)
    {
        var invoice = await _repo.GetInvoiceByIdAsync(invoiceId);

        if (invoice == null)
            throw new InvalidOperationException("Invoice not found.");

        if (invoice.IsClosed)
            throw new InvalidOperationException("Invoice is already closed (fully paid).");

        var actualBalance = invoice.TotalAmount - invoice.PaidAmount;

        if (actualBalance <= 0)
            throw new InvalidOperationException("Invoice is already fully paid.");

        if (dto.Amount > actualBalance)
            throw new InvalidOperationException("Payment exceeds remaining balance.");

        var payment = _mapper.Map<Payment>(dto);
        await _repo.AddPaymentAsync(invoiceId, payment);

        invoice.PaidAmount += dto.Amount;
        invoice.BalanceAmount = invoice.TotalAmount - invoice.PaidAmount;
        invoice.Status = DeriveStatus(invoice.PaidAmount, invoice.TotalAmount);

        if (invoice.Status == "Paid")
            invoice.IsClosed = true;

        await _repo.UpdateInvoiceAsync(invoice);
        return _mapper.Map<InvoiceResponseDto>(invoice);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────────

    private static string DeriveStatus(decimal paid, decimal total)
    {
        if (paid == 0) return "Pending";
        if (paid >= total) return "Paid";
        return "Partial";
    }

    /// <summary>Detects SQL Server / SQLite unique constraint violation inside DbUpdateException.</summary>
    private static bool IsDuplicateKeyException(DbUpdateException ex)
    {
        var msg = ex.InnerException?.Message ?? string.Empty;
        return msg.Contains("unique", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("2601", StringComparison.OrdinalIgnoreCase)   // SQL Server error code
            || msg.Contains("2627", StringComparison.OrdinalIgnoreCase);  // SQL Server error code
    }
}
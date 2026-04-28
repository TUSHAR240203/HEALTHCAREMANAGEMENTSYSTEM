using AutoMapper;
using Hms.BillingApi.DTOs.Billing;
using Hms.BillingApi.Entities;
using Hms.BillingApi.Interfaces;

namespace Hms.BillingApi.Services;

public class BillingService : IBillingService
{
    private readonly IInvoiceRepository _repo;
    private readonly IMapper _mapper;

    public BillingService(IInvoiceRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    // ✅ CREATE INVOICE
    public async Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceRequestDto dto)
    {
        var invoice = _mapper.Map<Invoice>(dto);

        // map items
        invoice.Items = _mapper.Map<List<InvoiceItem>>(dto.Items);

        // add consultation fee as item
        if (dto.ConsultationFee > 0)
        {
            invoice.Items.Add(new InvoiceItem
            {
                ServiceName = "Consultation Fee",
                Price = dto.ConsultationFee,
                Quantity = 1
            });
        }

        // 🔥 CALCULATE TOTAL
        invoice.TotalAmount = invoice.Items.Sum(x => x.Price * x.Quantity);

        invoice.PaidAmount = 0;
        invoice.BalanceAmount = invoice.TotalAmount;
        invoice.Status = "Pending";

        var result = await _repo.CreateInvoiceAsync(invoice);

        return _mapper.Map<InvoiceResponseDto>(result);
    }

    // ✅ GET BY ID
    public async Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int invoiceId)
    {
        var invoice = await _repo.GetInvoiceByIdAsync(invoiceId);
        return _mapper.Map<InvoiceResponseDto>(invoice);
    }

    // ✅ GET BY PATIENT
    public async Task<List<InvoiceResponseDto>> GetInvoicesByPatientIdAsync(int patientId)
    {
        var invoices = await _repo.GetInvoicesByPatientIdAsync(patientId);
        return _mapper.Map<List<InvoiceResponseDto>>(invoices);
    }

    // ✅ ADD ITEM
    public async Task<InvoiceResponseDto> AddInvoiceItemAsync(int invoiceId, AddInvoiceItemRequestDto dto)
    {
        var item = _mapper.Map<InvoiceItem>(dto);

        var updated = await _repo.AddInvoiceItemAsync(invoiceId, item);

        // 🔥 RECALCULATE TOTAL
        updated.TotalAmount = updated.Items.Sum(x => x.Price * x.Quantity);

        // 🔥 RECALCULATE BALANCE
        updated.BalanceAmount = updated.TotalAmount - updated.PaidAmount;

        // 🔥 FIX STATUS (MISSING IN YOUR CODE)
        if (updated.BalanceAmount == 0)
            updated.Status = "Paid";
        else if (updated.PaidAmount > 0)
            updated.Status = "Partial";
        else
            updated.Status = "Pending";

        // 🔥 SAVE CHANGES (VERY IMPORTANT - MISSING BEFORE)
        await _repo.UpdateInvoiceAsync(updated);

        return _mapper.Map<InvoiceResponseDto>(updated);
    }


    // ✅ ADD PAYMENT
    public async Task<InvoiceResponseDto> AddPaymentAsync(int invoiceId, PaymentRequestDto dto)
    {
        var invoice = await _repo.GetInvoiceByIdAsync(invoiceId);

        if (invoice == null)
            throw new Exception("Invoice not found");

        // 🔥 Always calculate fresh balance
        var actualBalance = invoice.TotalAmount - invoice.PaidAmount;

        // 🔥 Prevent paying already fully paid invoice
        if (actualBalance <= 0)
            throw new Exception("Invoice is already fully paid");

        // 🔥 Only block OVERPAYMENT (equal is allowed ✔)
        if (dto.Amount > actualBalance)
            throw new Exception("Payment exceeds remaining balance");

        var payment = _mapper.Map<Payment>(dto);

        // Save payment
        await _repo.AddPaymentAsync(invoiceId, payment);

        // 🔥 Update SAME invoice object
        invoice.PaidAmount += dto.Amount;
        invoice.BalanceAmount = invoice.TotalAmount - invoice.PaidAmount;

        // 🔥 Status logic
        if (invoice.BalanceAmount == 0)
            invoice.Status = "Paid";
        else if (invoice.PaidAmount > 0)
            invoice.Status = "Partial";
        else
            invoice.Status = "Pending";

        // 🔥 Persist updated totals
        await _repo.UpdateInvoiceAsync(invoice);

        return _mapper.Map<InvoiceResponseDto>(invoice);
    }
}
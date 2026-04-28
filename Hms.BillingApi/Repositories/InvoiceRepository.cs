using Hms.BillingApi.Data;
using Hms.BillingApi.Entities;
using Hms.BillingApi.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hms.BillingApi.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly BillingDbContext _context;

    public InvoiceRepository(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
    {
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<Invoice?> GetInvoiceByIdAsync(int invoiceId)
    {
        return await _context.Invoices
            .Include(x => x.Items)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == invoiceId);
    }

    public async Task<List<Invoice>> GetInvoicesByPatientIdAsync(int patientId)
    {
        return await _context.Invoices
            .Include(x => x.Items)
            .Include(x => x.Payments)
            .Where(x => x.PatientId == patientId)
            .ToListAsync();
    }

    public async Task<Invoice> AddInvoiceItemAsync(int invoiceId, InvoiceItem item)
    {
        var invoice = await _context.Invoices
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == invoiceId);

        if (invoice == null)
            throw new Exception("Invoice not found");

        invoice.Items.Add(item);

        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<Invoice> AddPaymentAsync(int invoiceId, Payment payment)
    {
        var invoice = await _context.Invoices
            .Include(x => x.Payments)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == invoiceId);

        if (invoice == null)
            throw new Exception("Invoice not found");

        invoice.Payments.Add(payment);

        await _context.SaveChangesAsync();
        return invoice;
    }

    // ✅ NEW METHOD (for saving totals)
    public async Task UpdateInvoiceAsync(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync();
    }
}
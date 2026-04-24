using Hms.BillingApi.Data;
using Hms.BillingApi.Entities;
using Hms.BillingApi.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace Hms.BillingApi.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly BillingDbContext _context;

    public InvoiceRepository(BillingDbContext context)
    {
        _context = context;
    }

    public async Task AddInvoiceAsync(Invoice invoice)
    {
        await _context.Invoices.AddAsync(invoice);
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
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task AddInvoiceItemAsync(InvoiceItem item)
    {
        await _context.InvoiceItems.AddAsync(item);
    }

    public async Task AddPaymentAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
    }

    public Task UpdateInvoiceAsync(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
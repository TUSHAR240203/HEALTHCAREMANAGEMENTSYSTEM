using Xunit;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Hms.BillingApi.Data;
using Hms.BillingApi.Repositories;
using Hms.BillingApi.Entities;

public class InvoiceRepositoryTests
{
    private BillingDbContext GetDb()
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BillingDbContext(options);
    }

    private Invoice GetValidInvoice()
    {
        return new Invoice
        {
            PatientId = 1,
            UHID = "UHID-001",   // ✅ required

            TotalAmount = 100,
            PaidAmount = 0,
            BalanceAmount = 100,
            Status = "Pending",

            Items = new List<InvoiceItem>(),
            Payments = new List<Payment>()
        };
    }

    [Fact]
    public async Task CreateInvoice_ShouldSaveToDb()
    {
        var context = GetDb();
        var repo = new InvoiceRepository(context);

        var invoice = GetValidInvoice();

        await repo.CreateInvoiceAsync(invoice);

        context.Invoices.Count().Should().Be(1);
    }

    [Fact]
    public async Task GetInvoiceById_ShouldIncludeItemsAndPayments()
    {
        var context = GetDb();
        var repo = new InvoiceRepository(context);

        var invoice = GetValidInvoice();

        invoice.Items.Add(new InvoiceItem
        {
            ServiceName = "Test",
            Price = 50,
            Quantity = 1,
            InvoiceId = invoice.Id
        });

        invoice.Payments.Add(new Payment
        {
            Amount = 50,
            InvoiceId = invoice.Id,
            PaymentMethod = "Cash"   // 🔥 REQUIRED FIX
        });

        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();

        var result = await repo.GetInvoiceByIdAsync(invoice.Id);

        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Payments.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AddInvoiceItem_ShouldAddItem()
    {
        var context = GetDb();
        var repo = new InvoiceRepository(context);

        var invoice = GetValidInvoice();
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();

        var item = new InvoiceItem
        {
            ServiceName = "Test Item",
            Price = 50,
            Quantity = 1,
            InvoiceId = invoice.Id
        };

        await repo.AddInvoiceItemAsync(invoice.Id, item);

        context.Invoices.First().Items.Count.Should().Be(1);
    }

    [Fact]
    public async Task AddPayment_ShouldAddPayment()
    {
        var context = GetDb();
        var repo = new InvoiceRepository(context);

        var invoice = GetValidInvoice();
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();

        var payment = new Payment
        {
            Amount = 50,
            InvoiceId = invoice.Id,
            PaymentMethod = "Cash"   // 🔥 REQUIRED FIX
        };

        await repo.AddPaymentAsync(invoice.Id, payment);

        context.Invoices.First().Payments.Count.Should().Be(1);
    }
}
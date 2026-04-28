using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using Hms.BillingApi.Services;
using Hms.BillingApi.Interfaces;
using Hms.BillingApi.Entities;
using Hms.BillingApi.DTOs.Billing;
using Hms.BillingApi.Mappings;

public class BillingServiceTests
{
    private readonly Mock<IInvoiceRepository> _repoMock;
    private readonly BillingService _service;

    public BillingServiceTests()
    {
        _repoMock = new Mock<IInvoiceRepository>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<BillingProfile>();
        });

        var mapper = config.CreateMapper();

        _service = new BillingService(_repoMock.Object, mapper);
    }

    [Fact]
    public async Task CreateInvoice_AddsConsultationFee_AndCalculatesTotal()
    {
        var dto = new CreateInvoiceRequestDto
        {
            ConsultationFee = 100,
            Items = new List<AddInvoiceItemRequestDto>
            {
                new AddInvoiceItemRequestDto { Price = 50, Quantity = 2 }
            }
        };

        _repoMock.Setup(r => r.CreateInvoiceAsync(It.IsAny<Invoice>()))
                 .ReturnsAsync((Invoice x) => x);

        var result = await _service.CreateInvoiceAsync(dto);

        result.TotalAmount.Should().Be(200);
        result.BalanceAmount.Should().Be(200);
        result.Status.Should().Be("Pending");

        // 🔥 Verify repo call
        _repoMock.Verify(r => r.CreateInvoiceAsync(It.IsAny<Invoice>()), Times.Once);
    }

    [Fact]
    public async Task AddPayment_FullPayment_ShouldMarkPaid()
    {
        var invoice = new Invoice
        {
            TotalAmount = 200,
            PaidAmount = 0,
            Items = new List<InvoiceItem>(),
            Payments = new List<Payment>()
        };

        _repoMock.Setup(r => r.GetInvoiceByIdAsync(1))
                 .ReturnsAsync(invoice);

        var result = await _service.AddPaymentAsync(1, new PaymentRequestDto { Amount = 200 });

        result.Status.Should().Be("Paid");
        result.BalanceAmount.Should().Be(0);

        // 🔥 Verify update + payment save
        _repoMock.Verify(r => r.AddPaymentAsync(1, It.IsAny<Payment>()), Times.Once);
        _repoMock.Verify(r => r.UpdateInvoiceAsync(It.IsAny<Invoice>()), Times.Once);
    }

    [Fact]
    public async Task AddPayment_Partial_ShouldMarkPartial()
    {
        var invoice = new Invoice
        {
            TotalAmount = 200,
            PaidAmount = 0,
            Items = new List<InvoiceItem>(),
            Payments = new List<Payment>()
        };

        _repoMock.Setup(r => r.GetInvoiceByIdAsync(1))
                 .ReturnsAsync(invoice);

        var result = await _service.AddPaymentAsync(1, new PaymentRequestDto { Amount = 100 });

        result.Status.Should().Be("Partial");
        result.BalanceAmount.Should().Be(100);

        _repoMock.Verify(r => r.AddPaymentAsync(1, It.IsAny<Payment>()), Times.Once);
        _repoMock.Verify(r => r.UpdateInvoiceAsync(It.IsAny<Invoice>()), Times.Once);
    }

    [Fact]
    public async Task AddPayment_ShouldThrow_WhenOverpayment()
    {
        var invoice = new Invoice
        {
            TotalAmount = 200,
            PaidAmount = 150
        };

        _repoMock.Setup(r => r.GetInvoiceByIdAsync(1))
                 .ReturnsAsync(invoice);

        Func<Task> act = async () =>
            await _service.AddPaymentAsync(1, new PaymentRequestDto { Amount = 100 });

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Payment exceeds remaining balance");

        // 🔥 Ensure no DB update happened
        _repoMock.Verify(r => r.UpdateInvoiceAsync(It.IsAny<Invoice>()), Times.Never);
    }

    [Fact]
    public async Task AddPayment_ShouldThrow_WhenAlreadyPaid()
    {
        var invoice = new Invoice
        {
            TotalAmount = 200,
            PaidAmount = 200
        };

        _repoMock.Setup(r => r.GetInvoiceByIdAsync(1))
                 .ReturnsAsync(invoice);

        Func<Task> act = async () =>
            await _service.AddPaymentAsync(1, new PaymentRequestDto { Amount = 10 });

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Invoice is already fully paid");

        _repoMock.Verify(r => r.UpdateInvoiceAsync(It.IsAny<Invoice>()), Times.Never);
    }

    [Fact]
    public async Task AddPayment_ShouldThrow_WhenInvoiceNotFound()
    {
        _repoMock.Setup(r => r.GetInvoiceByIdAsync(1))
                 .ReturnsAsync((Invoice)null!);

        Func<Task> act = async () =>
            await _service.AddPaymentAsync(1, new PaymentRequestDto());

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Invoice not found");

        _repoMock.Verify(r => r.UpdateInvoiceAsync(It.IsAny<Invoice>()), Times.Never);
    }
}
using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Hms.BillingApi.Services;
using Hms.BillingApi.Interfaces;
using Hms.BillingApi.Entities;
using Hms.BillingApi.DTOs.Billing;
using Hms.BillingApi.Mappings;

public class BillingServiceTests
{
    private readonly Mock<IInvoiceRepository> _repoMock;
    private readonly Mock<IServiceCatalogRepository> _catalogMock;
    private readonly Mock<IDoctorsApiClient> _doctorMock;
    private readonly Mock<ILogger<BillingService>> _loggerMock;
    private readonly BillingService _service;

    public BillingServiceTests()
    {
        _repoMock = new Mock<IInvoiceRepository>();
        _catalogMock = new Mock<IServiceCatalogRepository>();
        _doctorMock = new Mock<IDoctorsApiClient>();
        _loggerMock = new Mock<ILogger<BillingService>>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<BillingProfile>();
        }, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);

        var mapper = config.CreateMapper();

        _service = new BillingService(
            _repoMock.Object,
            _catalogMock.Object,
            mapper,
            _doctorMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CreateFromAppointment_ShouldAddConsultationFee()
    {
        // Arrange
        _doctorMock.Setup(x => x.GetConsultationFeeAsync(It.IsAny<int>()))
                   .ReturnsAsync(100);

        _repoMock.Setup(r => r.GetByAppointmentIdAsync(It.IsAny<int>()))
                 .ReturnsAsync((Invoice?)null);

        _repoMock.Setup(r => r.CreateInvoiceAsync(It.IsAny<Invoice>()))
                 .ReturnsAsync((Invoice x) => x);

        var dto = new CreateFromAppointmentRequestDto
        {
            PatientId = 1,
            AppointmentId = 10,
            DoctorId = 5
        };

        // Act
        var result = await _service.CreateFromAppointmentAsync(dto);

        // Assert
        result.TotalAmount.Should().Be(100);
        result.Status.Should().Be("Pending");

        _repoMock.Verify(r => r.CreateInvoiceAsync(It.IsAny<Invoice>()), Times.Once);
    }

    [Fact]
    public async Task AddInvoiceItem_ShouldUseCatalogPrice()
    {
        // Arrange
        var invoice = new Invoice
        {
            Id = 1,
            TotalAmount = 100,
            PaidAmount = 0,
            IsClosed = false,
            Items = new List<InvoiceItem>()
        };

        _repoMock.Setup(r => r.GetInvoiceByIdAsync(1))
                 .ReturnsAsync(invoice);

        _catalogMock.Setup(c => c.GetByIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(new ServiceCatalog
                    {
                        Id = 1,
                        Name = "Blood Test",
                        Price = 200,
                        Type = "Test"
                    });

        var dto = new AddInvoiceItemRequestDto
        {
            ServiceId = 1,
            Quantity = 2
        };

        // Act
        var result = await _service.AddInvoiceItemAsync(1, dto);

        // Assert
        result.TotalAmount.Should().Be(500); // 100 + (200 * 2)

        _repoMock.Verify(r => r.UpdateInvoiceAsync(It.IsAny<Invoice>()), Times.Once);
    }

    [Fact]
    public async Task AddPayment_FullPayment_ShouldMarkPaid()
    {
        var invoice = new Invoice
        {
            Id = 1,
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

        _repoMock.Verify(r => r.AddPaymentAsync(1, It.IsAny<Payment>()), Times.Once);
        _repoMock.Verify(r => r.UpdateInvoiceAsync(It.IsAny<Invoice>()), Times.Once);
    }

    [Fact]
    public async Task AddPayment_Partial_ShouldMarkPartial()
    {
        var invoice = new Invoice
        {
            Id = 1,
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
            Id = 1,
            TotalAmount = 200,
            PaidAmount = 150
        };

        _repoMock.Setup(r => r.GetInvoiceByIdAsync(1))
                 .ReturnsAsync(invoice);

        Func<Task> act = async () =>
            await _service.AddPaymentAsync(1, new PaymentRequestDto { Amount = 100 });

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Payment exceeds remaining balance");
    }

    [Fact]
    public async Task AddPayment_ShouldThrow_WhenAlreadyPaid()
    {
        var invoice = new Invoice
        {
            Id = 1,
            TotalAmount = 200,
            PaidAmount = 200
        };

        _repoMock.Setup(r => r.GetInvoiceByIdAsync(1))
                 .ReturnsAsync(invoice);

        Func<Task> act = async () =>
            await _service.AddPaymentAsync(1, new PaymentRequestDto { Amount = 10 });

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Invoice is already fully paid");
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
    }
}
using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Hms.BillingApi.Controllers;
using Hms.BillingApi.Interfaces;
using Hms.BillingApi.DTOs.Billing;
using Hms.BillingApi.Common;


public class BillingControllerTests
{
    private readonly Mock<IBillingService> _serviceMock;
    private readonly BillingController _controller;

    public BillingControllerTests()
    {
        _serviceMock = new Mock<IBillingService>();
        _controller = new BillingController(_serviceMock.Object);
    }

    [Fact]
    public async Task CreateInvoice_ReturnsWrappedResponse()
    {
        var responseDto = new InvoiceResponseDto();

        _serviceMock.Setup(x => x.CreateInvoiceAsync(It.IsAny<CreateInvoiceRequestDto>()))
            .ReturnsAsync(responseDto);

        var result = await _controller.CreateInvoice(new CreateInvoiceRequestDto());

        var okResult = result as OkObjectResult;

        okResult.Should().NotBeNull();
        okResult.Value.Should().BeOfType<ApiResponse<InvoiceResponseDto>>();
    }

    [Fact]
    public async Task AddItem_ReturnsWrappedResponse()
    {
        _serviceMock.Setup(x => x.AddInvoiceItemAsync(1, It.IsAny<AddInvoiceItemRequestDto>()))
            .ReturnsAsync(new InvoiceResponseDto());

        var result = await _controller.AddItem(1, new AddInvoiceItemRequestDto());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AddPayment_ReturnsWrappedResponse()
    {
        _serviceMock.Setup(x => x.AddPaymentAsync(1, It.IsAny<PaymentRequestDto>()))
            .ReturnsAsync(new InvoiceResponseDto());

        var result = await _controller.AddPayment(1, new PaymentRequestDto());

        result.Should().BeOfType<OkObjectResult>();
    }
}
using Hms.BillingApi.DTOs.Billing;
using Hms.BillingApi.Interfaces;
using Hms.BillingApi.Common;
using Microsoft.AspNetCore.Mvc;

namespace Hms.BillingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;

    public BillingController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    // ✅ CREATE INVOICE
    [HttpPost("invoice")]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequestDto dto)
    {
        var result = await _billingService.CreateInvoiceAsync(dto);

        return Ok(ApiResponse<InvoiceResponseDto>.SuccessResponse(
            result,
            "Invoice created successfully"
        ));
    }

    // ✅ ADD ITEM
    [HttpPost("{invoiceId}/item")]
    public async Task<IActionResult> AddItem(int invoiceId, [FromBody] AddInvoiceItemRequestDto dto)
    {
        var result = await _billingService.AddInvoiceItemAsync(invoiceId, dto);

        return Ok(ApiResponse<InvoiceResponseDto>.SuccessResponse(
            result,
            "Item added successfully"
        ));
    }

    // ✅ ADD PAYMENT
    [HttpPost("{invoiceId}/payment")]
    public async Task<IActionResult> AddPayment(int invoiceId, [FromBody] PaymentRequestDto dto)
    {
        var result = await _billingService.AddPaymentAsync(invoiceId, dto);

        return Ok(ApiResponse<InvoiceResponseDto>.SuccessResponse(
            result,
            "Payment processed successfully"
        ));
    }
}
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

    [HttpPost("invoice")]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequestDto dto)
    {
        var result = await _billingService.CreateInvoiceAsync(dto);
        return Ok(ApiResponse<InvoiceResponseDto>.SuccessResponse(result, "Invoice created successfully"));
    }

    [HttpGet("invoice/{invoiceId:int}")]
    public async Task<IActionResult> GetInvoiceById(int invoiceId)
    {
        var result = await _billingService.GetInvoiceByIdAsync(invoiceId);
        return result == null
            ? NotFound(ApiResponse<object>.FailResponse("Invoice not found"))
            : Ok(ApiResponse<InvoiceResponseDto>.SuccessResponse(result));
    }

    [HttpGet("patient/{patientId:int}/invoices")]
    public async Task<IActionResult> GetInvoicesByPatientId(int patientId)
    {
        var result = await _billingService.GetInvoicesByPatientIdAsync(patientId);
        return Ok(ApiResponse<List<InvoiceResponseDto>>.SuccessResponse(result));
    }

    [HttpPost("{invoiceId:int}/item")]
    public async Task<IActionResult> AddItem(int invoiceId, [FromBody] AddInvoiceItemRequestDto dto)
    {
        var result = await _billingService.AddInvoiceItemAsync(invoiceId, dto);
        return Ok(ApiResponse<InvoiceResponseDto>.SuccessResponse(result, "Item added successfully"));
    }

    [HttpPost("{invoiceId:int}/payment")]
    public async Task<IActionResult> AddPayment(int invoiceId, [FromBody] PaymentRequestDto dto)
    {
        var result = await _billingService.AddPaymentAsync(invoiceId, dto);
        return Ok(ApiResponse<InvoiceResponseDto>.SuccessResponse(result, "Payment processed successfully"));
    }
}

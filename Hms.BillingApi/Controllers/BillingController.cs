using Hms.BillingApi.DTOs.Billing;
using Hms.BillingApi.Interfaces;
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
        return Ok(result);
    }

    [HttpPost("{invoiceId}/item")]
    public async Task<IActionResult> AddItem(int invoiceId, AddInvoiceItemRequestDto dto)
    {
        var result = await _billingService.AddInvoiceItemAsync(invoiceId, dto);
        return Ok(result);
    }

    [HttpPost("{invoiceId}/payment")]
    public async Task<IActionResult> AddPayment(int invoiceId, PaymentRequestDto dto)
    {
        var result = await _billingService.AddPaymentAsync(invoiceId, dto);
        return Ok(result);
    }
}
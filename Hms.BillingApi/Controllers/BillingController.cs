using Hms.BillingApi.DTOs.Billing;
using Hms.BillingApi.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hms.BillingApi.Controllers;

[ApiController]
[Route("api/billing")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;

    public BillingController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpPost("invoice")]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequestDto request)
    {
        var result = await _billingService.CreateInvoiceAsync(request);
        return CreatedAtAction(nameof(GetInvoiceById), new { invoiceId = result.Id }, result);
    }

    [HttpGet("invoice/{invoiceId:int}")]
    public async Task<IActionResult> GetInvoiceById(int invoiceId)
    {
        var result = await _billingService.GetInvoiceByIdAsync(invoiceId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("patient/{patientId:int}/invoices")]
    public async Task<IActionResult> GetInvoicesByPatientId(int patientId)
    {
        var result = await _billingService.GetInvoicesByPatientIdAsync(patientId);
        return Ok(result);
    }

    [HttpPost("invoice/{invoiceId:int}/items")]
    public async Task<IActionResult> AddInvoiceItem(int invoiceId, [FromBody] AddInvoiceItemRequestDto request)
    {
        var result = await _billingService.AddInvoiceItemAsync(invoiceId, request);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("invoice/{invoiceId:int}/pay")]
    public async Task<IActionResult> AddPayment(int invoiceId, [FromBody] PaymentRequestDto request)
    {
        var result = await _billingService.AddPaymentAsync(invoiceId, request);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
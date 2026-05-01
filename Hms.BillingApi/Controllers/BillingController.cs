using Hms.BillingApi.DTOs.Billing;
using Hms.BillingApi.Interfaces;
using Hms.BillingApi.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hms.BillingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]   // all endpoints require a valid JWT by default
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;

    public BillingController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    // ── Existing endpoints ────────────────────────────────────────────────────
    [AllowAnonymous]
    [HttpPost("invoice")]
    //[Authorize(Roles = "Receptionist")]
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
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> AddItem(int invoiceId, [FromBody] AddInvoiceItemRequestDto dto)
    {
        var result = await _billingService.AddInvoiceItemAsync(invoiceId, dto);
        return Ok(ApiResponse<InvoiceResponseDto>.SuccessResponse(result, "Item added successfully"));
    }

    [HttpPost("{invoiceId:int}/payment")]
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> AddPayment(int invoiceId, [FromBody] PaymentRequestDto dto)
    {
        var result = await _billingService.AddPaymentAsync(invoiceId, dto);
        return Ok(ApiResponse<InvoiceResponseDto>.SuccessResponse(result, "Payment processed successfully"));
    }

    // ── New endpoints ─────────────────────────────────────────────────────────

    /// <summary>
    /// Called by AppointmentsApi when an appointment is marked Completed.
    /// Restricted to internal System role — NOT accessible by end users.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("create-from-appointment")]
    //[Authorize(Roles = "System")]
    public async Task<IActionResult> CreateFromAppointment([FromBody] CreateFromAppointmentRequestDto dto)
    {
        var result = await _billingService.CreateFromAppointmentAsync(dto);
        return Ok(ApiResponse<InvoiceResponseDto>.SuccessResponse(result, "Invoice created from appointment"));
    }

    /// <summary>
    /// Returns the latest open invoice for a patient (no-appointment item-add use case).
    /// Returns 404 if no open invoice exists.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("active/{patientId:int}")]
    //[Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> GetActiveInvoice(int patientId)
    {
        var result = await _billingService.GetActiveInvoiceAsync(patientId);
        return result == null
            ? NotFound(ApiResponse<object>.FailResponse("No active invoice found for this patient"))
            : Ok(ApiResponse<InvoiceResponseDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Returns all active services from the catalog.
    /// Receptionist calls this first to get ServiceId + Price before adding an item to an invoice.
    /// </summary>
    [HttpGet("services")]
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> GetServiceCatalog()
    {
        var result = await _billingService.GetServiceCatalogAsync();
        return Ok(ApiResponse<List<ServiceCatalogResponseDto>>.SuccessResponse(result));
    }
}

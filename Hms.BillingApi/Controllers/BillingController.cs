using Hms.BillingApi.DTOs.Billing;
using Hms.BillingApi.Interfaces;
using Hms.BillingApi.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hms.BillingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;

    public BillingController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    // ── Existing endpoints ────────────────────────────────────────────────────

    [HttpPost("invoice")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequestDto dto)
    {
        var result = await _billingService.CreateInvoiceAsync(dto);
        return Ok(ApiResponse<InvoiceResponseDto>.SuccessResponse(result, "Invoice created successfully"));
    }

    [HttpGet("invoice/{invoiceId:int}")]
    [Authorize(Roles = "Admin,Receptionist,Patient")]
    public async Task<IActionResult> GetInvoiceById(int invoiceId)
    {
        var result = await _billingService.GetInvoiceByIdAsync(invoiceId);
        return result == null
            ? NotFound(ApiResponse<object>.FailResponse("Invoice not found"))
            : Ok(ApiResponse<InvoiceResponseDto>.SuccessResponse(result));
    }

    [HttpGet("appointment/{appointmentId:int}/invoice")]
    [Authorize(Roles = "Admin,Receptionist,Doctor,Patient")]
    public async Task<IActionResult> GetInvoiceByAppointmentId(int appointmentId)
    {
        var result = await _billingService.GetInvoiceByAppointmentIdAsync(appointmentId);
        return result == null
            ? NotFound(ApiResponse<object>.FailResponse("Invoice not found for this appointment yet"))
            : Ok(ApiResponse<InvoiceResponseDto>.SuccessResponse(result));
    }

    [HttpGet("patient/{patientId:int}/invoices")]
    [Authorize(Roles = "Admin,Receptionist,Patient")]
    public async Task<IActionResult> GetInvoicesByPatientId(int patientId)
    {
        var result = await _billingService.GetInvoicesByPatientIdAsync(patientId);
        return Ok(ApiResponse<List<InvoiceResponseDto>>.SuccessResponse(result));
    }

    [HttpPost("{invoiceId:int}/item")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> AddItem(int invoiceId, [FromBody] AddInvoiceItemRequestDto dto)
    {
        var result = await _billingService.AddInvoiceItemAsync(invoiceId, dto);
        return Ok(ApiResponse<InvoiceResponseDto>.SuccessResponse(result, "Item added successfully"));
    }

    [HttpPost("{invoiceId:int}/payment")]
    [Authorize(Roles = "Admin,Receptionist")]
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
    [HttpPost("create-from-appointment")]
    [Authorize(Roles = "System,Admin,Receptionist,Doctor")]
    public async Task<IActionResult> CreateFromAppointment([FromBody] CreateFromAppointmentRequestDto dto)
    {
        var result = await _billingService.CreateFromAppointmentAsync(dto);
        return Ok(ApiResponse<InvoiceResponseDto>.SuccessResponse(result, "Invoice created from appointment"));
    }
    /// <summary>
    /// Returns the latest open invoice for a patient.
    /// Admin/Receptionist only.
    /// </summary>
    [HttpGet("active/{patientId:int}")]
    [Authorize(Roles = "Admin,Receptionist")]
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
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> GetServiceCatalog()
    {
        var result = await _billingService.GetServiceCatalogAsync();
        return Ok(ApiResponse<List<ServiceCatalogResponseDto>>.SuccessResponse(result));
    }
}
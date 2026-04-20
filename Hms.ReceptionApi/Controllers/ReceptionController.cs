using Hms.ReceptionApi.DTOs.Reception;
using Hms.ReceptionApi.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hms.ReceptionApi.Controllers;

[ApiController]
[Route("api/reception")]
public class ReceptionController : ControllerBase
{
    private readonly IReceptionService _receptionService;

    public ReceptionController(IReceptionService receptionService)
    {
        _receptionService = receptionService;
    }

    [HttpPost("patients/search")]
    public async Task<IActionResult> SearchPatients([FromBody] ReceptionPatientSearchRequestDto request)
    {
        var result = await _receptionService.SearchPatientsAsync(request);
        return Ok(result);
    }

    [HttpPost("patients/register")]
    public async Task<IActionResult> RegisterPatient([FromBody] RegisterPatientByReceptionRequestDto request)
    {
        var result = await _receptionService.RegisterPatientAsync(request);
        return Ok(result);
    }

    [HttpGet("patients/{patientId:int}/summary")]
    public async Task<IActionResult> GetPatientSummary(int patientId)
    {
        var result = await _receptionService.GetPatientSummaryAsync(patientId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("patients/{patientId:int}/verify")]
    public async Task<IActionResult> VerifyPatient(int patientId, [FromBody] VerifyPatientRequestDto request)
    {
        var result = await _receptionService.VerifyPatientAsync(patientId, request);
        return Ok(result);
    }

    [HttpPost("patients/{patientId:int}/portal-activation/resend")]
    public async Task<IActionResult> ResendPortalActivation(int patientId, [FromBody] ResendPortalActivationRequestDto request)
    {
        await _receptionService.ResendPortalActivationAsync(patientId, request);
        return Ok(new { message = "Portal activation message sent successfully." });
    }

    [HttpPost("appointments/book")]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentRequestDto request)
    {
        var result = await _receptionService.BookAppointmentAsync(request);
        return Ok(result);
    }

    [HttpPut("appointments/{appointmentId:int}/reschedule")]
    public async Task<IActionResult> RescheduleAppointment(int appointmentId, [FromBody] RescheduleAppointmentRequestDto request)
    {
        var result = await _receptionService.RescheduleAppointmentAsync(appointmentId, request);
        return Ok(result);
    }

    [HttpPut("appointments/{appointmentId:int}/cancel")]
    public async Task<IActionResult> CancelAppointment(int appointmentId, [FromBody] CancelAppointmentRequestDto request)
    {
        var result = await _receptionService.CancelAppointmentAsync(appointmentId, request);
        return Ok(result);
    }

    [HttpPost("checkin")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDto request)
    {
        var result = await _receptionService.CheckInAsync(request);
        return Ok(result);
    }

    [HttpGet("checkin/{checkInId:int}")]
    public async Task<IActionResult> GetCheckInById(int checkInId)
    {
        var result = await _receptionService.GetCheckInByIdAsync(checkInId);
        if (result == null) return NotFound();
        return Ok(result);
    }
    [HttpPost("billing/invoice")]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequestDto request)
    {
        var result = await _receptionService.CreateInvoiceAsync(request);
        return Ok(result);
    }

    [HttpGet("billing/invoice/{invoiceId:int}")]
    public async Task<IActionResult> GetInvoiceById(int invoiceId)
    {
        var result = await _receptionService.GetInvoiceByIdAsync(invoiceId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("billing/patient/{patientId:int}/invoices")]
    public async Task<IActionResult> GetInvoicesByPatientId(int patientId)
    {
        var result = await _receptionService.GetInvoicesByPatientIdAsync(patientId);
        return Ok(result);
    }

    [HttpPost("billing/invoice/{invoiceId:int}/items")]
    public async Task<IActionResult> AddInvoiceItem(int invoiceId, [FromBody] AddInvoiceItemRequestDto request)
    {
        var result = await _receptionService.AddInvoiceItemAsync(invoiceId, request);
        return Ok(result);
    }

    [HttpPost("billing/invoice/{invoiceId:int}/pay")]
    public async Task<IActionResult> AddPayment(int invoiceId, [FromBody] PaymentRequestDto request)
    {
        var result = await _receptionService.AddPaymentAsync(invoiceId, request);
        return Ok(result);
    }
}
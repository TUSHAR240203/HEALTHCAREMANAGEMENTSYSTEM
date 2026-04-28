using Hms.ReceptionApi.DTOs.Common;
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

        return Ok(
            ApiResponse<object>.Ok(result, "Patients fetched successfully."));
    }

    [HttpPost("patients/register")]
    public async Task<IActionResult> RegisterPatient([FromBody] RegisterPatientByReceptionRequestDto request)
    {
        var result = await _receptionService.RegisterPatientAsync(request);

        return Ok(
            ApiResponse<object>.Ok(result, "Patient registered successfully."));
    }

    [HttpGet("patients/{patientId:int}/summary")]
    public async Task<IActionResult> GetPatientSummary(int patientId)
    {
        var result = await _receptionService.GetPatientSummaryAsync(patientId);

        if (result == null)
            return NotFound(
                ApiResponse<object>.Fail("Patient not found."));

        return Ok(
            ApiResponse<object>.Ok(result, "Patient summary fetched successfully."));
    }

    [HttpPost("patients/{patientId:int}/verify")]
    public async Task<IActionResult> VerifyPatient(
        int patientId,
        [FromBody] VerifyPatientRequestDto request)
    {
        var result = await _receptionService.VerifyPatientAsync(patientId, request);

        return Ok(
            ApiResponse<object>.Ok(result, "Verification completed."));
    }

    [HttpPost("patients/{patientId:int}/portal-activation/resend")]
    public async Task<IActionResult> ResendPortalActivation(
        int patientId,
        [FromBody] ResendPortalActivationRequestDto request)
    {
        await _receptionService.ResendPortalActivationAsync(patientId, request);

        return Ok(
            ApiResponse<object>.Ok(null, "Portal activation sent successfully."));
    }

    [HttpPost("appointments/book")]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentRequestDto request)
    {
        var result = await _receptionService.BookAppointmentAsync(request);

        return Ok(
            ApiResponse<object>.Ok(result, "Appointment booked successfully."));
    }

    [HttpPut("appointments/{appointmentId:int}/reschedule")]
    public async Task<IActionResult> RescheduleAppointment(
        int appointmentId,
        [FromBody] RescheduleAppointmentRequestDto request)
    {
        var result = await _receptionService.RescheduleAppointmentAsync(appointmentId, request);

        return Ok(
            ApiResponse<object>.Ok(result, "Appointment rescheduled successfully."));
    }

    [HttpPut("appointments/{appointmentId:int}/cancel")]
    public async Task<IActionResult> CancelAppointment(
        int appointmentId,
        [FromBody] CancelAppointmentRequestDto request)
    {
        var result = await _receptionService.CancelAppointmentAsync(appointmentId, request);

        return Ok(
            ApiResponse<object>.Ok(result, "Appointment cancelled successfully."));
    }

    [HttpPost("checkin")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDto request)
    {
        try
        {
            var result = await _receptionService.CheckInAsync(request);

            return Ok(
                ApiResponse<object>.Ok(result, "Patient checked in successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpGet("checkin/{checkInId:int}")]
    public async Task<IActionResult> GetCheckInById(int checkInId)
    {
        var result = await _receptionService.GetCheckInByIdAsync(checkInId);

        if (result == null)
            return NotFound(
                ApiResponse<object>.Fail("Check-in record not found."));

        return Ok(
            ApiResponse<object>.Ok(result, "Check-in fetched successfully."));
    }

    [HttpPost("billing/invoice")]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequestDto request)
    {
        var result = await _receptionService.CreateInvoiceAsync(request);

        return Ok(
            ApiResponse<object>.Ok(result, "Invoice created successfully."));
    }

    [HttpGet("billing/invoice/{invoiceId:int}")]
    public async Task<IActionResult> GetInvoiceById(int invoiceId)
    {
        var result = await _receptionService.GetInvoiceByIdAsync(invoiceId);

        if (result == null)
            return NotFound(
                ApiResponse<object>.Fail("Invoice not found."));

        return Ok(
            ApiResponse<object>.Ok(result, "Invoice fetched successfully."));
    }

    [HttpGet("billing/patient/{patientId:int}/invoices")]
    public async Task<IActionResult> GetInvoicesByPatientId(int patientId)
    {
        var result = await _receptionService.GetInvoicesByPatientIdAsync(patientId);

        return Ok(
            ApiResponse<object>.Ok(result, "Invoices fetched successfully."));
    }

    [HttpPost("billing/invoice/{invoiceId:int}/items")]
    public async Task<IActionResult> AddInvoiceItem(
        int invoiceId,
        [FromBody] AddInvoiceItemRequestDto request)
    {
        var result = await _receptionService.AddInvoiceItemAsync(invoiceId, request);

        return Ok(
            ApiResponse<object>.Ok(result, "Invoice item added successfully."));
    }

    [HttpPost("billing/invoice/{invoiceId:int}/pay")]
    public async Task<IActionResult> AddPayment(
        int invoiceId,
        [FromBody] PaymentRequestDto request)
    {
        var result = await _receptionService.AddPaymentAsync(invoiceId, request);

        return Ok(
            ApiResponse<object>.Ok(result, "Payment added successfully."));
    }
    [HttpGet("appointments/today")]
    public async Task<IActionResult> GetTodayAppointmentsForCheckIn([FromQuery] DateOnly date)
    {
        try
        {
            var result = await _receptionService.GetTodayAppointmentsForCheckInAsync(date);

            return Ok(
                ApiResponse<object>.Ok(result, "Today appointments fetched successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                ApiResponse<object>.Fail(
                    "Could not load today's appointments.",
                    new
                    {
                        Error = ex.Message,
                        InnerError = ex.InnerException?.Message,
                        StackTrace = ex.StackTrace
                    }));
        }
    }

}
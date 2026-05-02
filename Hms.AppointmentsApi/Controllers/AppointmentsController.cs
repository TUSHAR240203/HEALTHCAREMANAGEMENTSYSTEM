using Hms.AppointmentsApi.Common;
using Hms.AppointmentsApi.DTOs.Appointments;
using Hms.AppointmentsApi.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hms.AppointmentsApi.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequestDto request)
    {
        if (request == null)
            return BadRequest(ApiResponse<object>.Fail("Appointment request is required."));

        var result = await _appointmentService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            ApiResponse<AppointmentResponseDto>.Ok(result, "Appointment created successfully.")
        );
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Receptionist,Doctor,Patient")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequest(ApiResponse<object>.Fail("Invalid appointment id."));

        var result = await _appointmentService.GetByIdAsync(id);

        return result == null
            ? NotFound(ApiResponse<object>.Fail("Appointment not found."))
            : Ok(ApiResponse<AppointmentResponseDto>.Ok(result));
    }

    [HttpGet("patient/{patientId:int}")]
    [Authorize(Roles = "Admin,Receptionist,Patient")]
    public async Task<IActionResult> GetByPatientId(int patientId)
    {
        if (patientId <= 0)
            return BadRequest(ApiResponse<object>.Fail("Invalid patient id."));

        var result = await _appointmentService.GetByPatientIdAsync(patientId);

        return Ok(ApiResponse<List<AppointmentResponseDto>>.Ok(result));
    }

    [HttpGet("doctor/{doctorId:int}")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public async Task<IActionResult> GetByDoctorId(int doctorId)
    {
        if (doctorId <= 0)
            return BadRequest(ApiResponse<object>.Fail("Invalid doctor id."));

        var result = await _appointmentService.GetByDoctorIdAsync(doctorId);

        return Ok(ApiResponse<List<AppointmentResponseDto>>.Ok(result));
    }

    [HttpPost("search")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public async Task<IActionResult> Search([FromBody] AppointmentSearchRequestDto request)
    {
        if (request == null)
            return BadRequest(ApiResponse<object>.Fail("Search request is required."));

        var result = await _appointmentService.SearchAsync(request);

        return Ok(ApiResponse<AppointmentSearchResponseDto>.Ok(result));
    }

    [HttpPut("{id:int}/reschedule")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Reschedule(
        int id,
        [FromBody] RescheduleAppointmentRequestDto request)
    {
        if (id <= 0)
            return BadRequest(ApiResponse<object>.Fail("Invalid appointment id."));

        if (request == null)
            return BadRequest(ApiResponse<object>.Fail("Reschedule request is required."));

        var result = await _appointmentService.RescheduleAsync(id, request);

        return result == null
            ? NotFound(ApiResponse<object>.Fail("Appointment not found."))
            : Ok(ApiResponse<AppointmentResponseDto>.Ok(result, "Appointment rescheduled successfully."));
    }

    [HttpPut("{id:int}/cancel")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Cancel(
        int id,
        [FromBody] CancelAppointmentRequestDto request)
    {
        if (id <= 0)
            return BadRequest(ApiResponse<object>.Fail("Invalid appointment id."));

        if (request == null)
            return BadRequest(ApiResponse<object>.Fail("Cancel request is required."));

        var result = await _appointmentService.CancelAsync(id, request);

        return result == null
            ? NotFound(ApiResponse<object>.Fail("Appointment not found."))
            : Ok(ApiResponse<AppointmentResponseDto>.Ok(result, "Appointment cancelled successfully."));
    }

    [HttpPut("{id:int}/complete")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public async Task<IActionResult> Complete(
        int id,
        [FromBody] CompleteAppointmentRequestDto request)
    {
        if (id <= 0)
            return BadRequest(ApiResponse<object>.Fail("Invalid appointment id."));

        if (request == null)
            return BadRequest(ApiResponse<object>.Fail("Complete appointment request is required."));

        var result = await _appointmentService.CompleteAsync(id, request);

        return result == null
            ? NotFound(ApiResponse<object>.Fail("Appointment not found."))
            : Ok(ApiResponse<AppointmentResponseDto>.Ok(result, "Appointment completed successfully."));
    }
}
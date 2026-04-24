using Hms.AppointmentsApi.Common;
using Hms.AppointmentsApi.DTOs.Appointments;
using Hms.AppointmentsApi.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hms.AppointmentsApi.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequestDto request)
    {
        var result = await _appointmentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<AppointmentResponseDto>.Ok(result, "Appointment created successfully."));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _appointmentService.GetByIdAsync(id);
        return result == null
            ? NotFound(ApiResponse<object>.Fail("Appointment not found."))
            : Ok(ApiResponse<AppointmentResponseDto>.Ok(result));
    }

    [HttpGet("patient/{patientId:int}")]
    public async Task<IActionResult> GetByPatientId(int patientId)
    {
        var result = await _appointmentService.GetByPatientIdAsync(patientId);
        return Ok(ApiResponse<List<AppointmentResponseDto>>.Ok(result));
    }

    [HttpGet("doctor/{doctorId:int}")]
    public async Task<IActionResult> GetByDoctorId(int doctorId)
    {
        var result = await _appointmentService.GetByDoctorIdAsync(doctorId);
        return Ok(ApiResponse<List<AppointmentResponseDto>>.Ok(result));
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] AppointmentSearchRequestDto request)
    {
        var result = await _appointmentService.SearchAsync(request);
        return Ok(ApiResponse<AppointmentSearchResponseDto>.Ok(result));
    }

    [HttpPut("{id:int}/reschedule")]
    public async Task<IActionResult> Reschedule(int id, [FromBody] RescheduleAppointmentRequestDto request)
    {
        var result = await _appointmentService.RescheduleAsync(id, request);
        return result == null
            ? NotFound(ApiResponse<object>.Fail("Appointment not found."))
            : Ok(ApiResponse<AppointmentResponseDto>.Ok(result, "Appointment rescheduled successfully."));
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelAppointmentRequestDto request)
    {
        var result = await _appointmentService.CancelAsync(id, request);
        return result == null
            ? NotFound(ApiResponse<object>.Fail("Appointment not found."))
            : Ok(ApiResponse<AppointmentResponseDto>.Ok(result, "Appointment cancelled successfully."));
    }

    [HttpPut("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, [FromBody] CompleteAppointmentRequestDto request)
    {
        var result = await _appointmentService.CompleteAsync(id, request);
        return result == null
            ? NotFound(ApiResponse<object>.Fail("Appointment not found."))
            : Ok(ApiResponse<AppointmentResponseDto>.Ok(result, "Appointment completed successfully."));
    }
}

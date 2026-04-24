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
    [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequestDto request)
    {
        var result = await _appointmentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _appointmentService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("patient/{patientId:int}")]
    [ProducesResponseType(typeof(List<AppointmentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPatientId(int patientId)
    {
        var result = await _appointmentService.GetByPatientIdAsync(patientId);
        return Ok(result);
    }

    [HttpGet("doctor/{doctorId:int}")]
    [ProducesResponseType(typeof(List<AppointmentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDoctorId(int doctorId)
    {
        var result = await _appointmentService.GetByDoctorIdAsync(doctorId);
        return Ok(result);
    }

    [HttpPost("search")]
    [ProducesResponseType(typeof(AppointmentSearchResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromBody] AppointmentSearchRequestDto request)
    {
        var result = await _appointmentService.SearchAsync(request);
        return Ok(result);
    }

    [HttpPut("{id:int}/reschedule")]
    [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reschedule(int id, [FromBody] RescheduleAppointmentRequestDto request)
    {
        var result = await _appointmentService.RescheduleAsync(id, request);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("{id:int}/cancel")]
    [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelAppointmentRequestDto request)
    {
        var result = await _appointmentService.CancelAsync(id, request);
        if (result == null) return NotFound();
        return Ok(result);
    }

<<<<<<< HEAD
    [HttpPut("{id:int}/start")]
    [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Start(int id)
    {
        var result = await _appointmentService.StartAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

=======
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    [HttpPut("{id:int}/complete")]
    [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(int id, [FromBody] CompleteAppointmentRequestDto request)
    {
        var result = await _appointmentService.CompleteAsync(id, request);
        if (result == null) return NotFound();
        return Ok(result);
    }
<<<<<<< HEAD

    [HttpPut("{id:int}/notes")]
    [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddNotes(int id, [FromBody] UpdateAppointmentNotesRequestDto request)
    {
        var result = await _appointmentService.AddNotesAsync(id, request);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
=======
}
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85

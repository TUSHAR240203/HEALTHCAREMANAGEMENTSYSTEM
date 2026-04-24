using Hms.DoctorsApi.Common;
using Hms.DoctorsApi.DTOs.Appointments;
using Hms.DoctorsApi.DTOs.Doctors;
using Hms.DoctorsApi.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hms.DoctorsApi.Controllers;

[ApiController]
[Route("api/doctors")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDoctorRequestDto request)
    {
        var result = await _doctorService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, Wrap(result, "Doctor created successfully."));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _doctorService.GetByIdAsync(id);
        return result == null ? NotFound(Fail("Doctor not found.")) : Ok(Wrap(result, "Doctor fetched successfully."));
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] DoctorSearchRequestDto request)
    {
        var result = await _doctorService.SearchAsync(request);
        return Ok(Wrap(result, "Doctors fetched successfully."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDoctorRequestDto request)
    {
        var result = await _doctorService.UpdateAsync(id, request);
        return result == null ? NotFound(Fail("Doctor not found.")) : Ok(Wrap(result, "Doctor updated successfully."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _doctorService.SoftDeleteAsync(id);
        return deleted ? Ok(Wrap<object?>(null, "Doctor deleted successfully.")) : NotFound(Fail("Doctor not found."));
    }

    [HttpGet("{doctorId:int}/schedules")]
    public async Task<IActionResult> GetSchedules(int doctorId)
    {
        var result = await _doctorService.GetSchedulesAsync(doctorId);
        return Ok(Wrap(result, "Doctor schedules fetched successfully."));
    }

    [HttpPost("{doctorId:int}/schedules")]
    public async Task<IActionResult> AddSchedule(int doctorId, [FromBody] CreateDoctorScheduleRequestDto request)
    {
        var result = await _doctorService.AddScheduleAsync(doctorId, request);
        return Ok(Wrap(result, "Doctor schedule added successfully."));
    }

    [HttpDelete("{doctorId:int}/schedules/{scheduleId:int}")]
    public async Task<IActionResult> DeleteSchedule(int doctorId, int scheduleId)
    {
        var deleted = await _doctorService.DeleteScheduleAsync(doctorId, scheduleId);
        return deleted ? Ok(Wrap<object?>(null, "Doctor schedule deleted successfully.")) : NotFound(Fail("Doctor schedule not found."));
    }

    [HttpGet("{doctorId:int}/leaves")]
    public async Task<IActionResult> GetLeaves(int doctorId)
    {
        var result = await _doctorService.GetLeavesAsync(doctorId);
        return Ok(Wrap(result, "Doctor leaves fetched successfully."));
    }

    [HttpPost("{doctorId:int}/leaves")]
    public async Task<IActionResult> AddLeave(int doctorId, [FromBody] CreateDoctorLeaveRequestDto request)
    {
        var result = await _doctorService.AddLeaveAsync(doctorId, request);
        return Ok(Wrap(result, "Doctor leave added successfully."));
    }

    [HttpDelete("{doctorId:int}/leaves/{leaveId:int}")]
    public async Task<IActionResult> DeleteLeave(int doctorId, int leaveId)
    {
        var deleted = await _doctorService.DeleteLeaveAsync(doctorId, leaveId);
        return deleted ? Ok(Wrap<object?>(null, "Doctor leave deleted successfully.")) : NotFound(Fail("Doctor leave not found."));
    }

    [HttpGet("{doctorId:int}/available-slots")]
    public async Task<IActionResult> GetAvailableSlots(int doctorId, [FromQuery] DateOnly date, [FromQuery] bool? isTeleConsultation)
    {
        var result = await _doctorService.GetAvailableSlotsAsync(doctorId, date, isTeleConsultation);
        return Ok(Wrap(result, "Available slots fetched successfully."));
    }

    [HttpGet("{doctorId:int}/appointments/today")]
    public async Task<IActionResult> GetTodayAppointments(int doctorId)
    {
        var result = await _doctorService.GetTodayAppointmentsAsync(doctorId);
        return Ok(Wrap(result, "Today appointments fetched successfully."));
    }

    [HttpGet("{doctorId:int}/appointments/upcoming")]
    public async Task<IActionResult> GetUpcomingAppointments(int doctorId)
    {
        var result = await _doctorService.GetUpcomingAppointmentsAsync(doctorId);
        return Ok(Wrap(result, "Upcoming appointments fetched successfully."));
    }

    [HttpGet("{doctorId:int}/queue/current")]
    public async Task<IActionResult> GetCurrentQueue(int doctorId, [FromQuery] DateOnly date)
    {
        var result = await _doctorService.GetCurrentQueueAsync(doctorId, date);
        return result == null ? NotFound(Fail("No current queue item found.")) : Ok(Wrap(result, "Current queue item fetched successfully."));
    }

    [HttpPut("{doctorId:int}/appointments/{appointmentId:int}/start")]
    public async Task<IActionResult> StartAppointment(int doctorId, int appointmentId)
    {
        var result = await _doctorService.StartAppointmentAsync(doctorId, appointmentId);
        return result == null ? NotFound(Fail("Appointment not found.")) : Ok(Wrap(result, "Consultation started successfully."));
    }

    [HttpPut("{doctorId:int}/appointments/{appointmentId:int}/complete")]
    public async Task<IActionResult> CompleteAppointment(int doctorId, int appointmentId, [FromBody] CompleteAppointmentRequestDto request)
    {
        var result = await _doctorService.CompleteAppointmentAsync(doctorId, appointmentId, request);
        return result == null ? NotFound(Fail("Appointment not found.")) : Ok(Wrap(result, "Consultation completed successfully."));
    }

    [HttpPut("{doctorId:int}/appointments/{appointmentId:int}/notes")]
    public async Task<IActionResult> AddAppointmentNotes(int doctorId, int appointmentId, [FromBody] UpdateAppointmentNotesRequestDto request)
    {
        var result = await _doctorService.AddAppointmentNotesAsync(doctorId, appointmentId, request);
        return result == null ? NotFound(Fail("Appointment not found.")) : Ok(Wrap(result, "Appointment notes updated successfully."));
    }

    private ApiResponse<T> Wrap<T>(T data, string message) => ApiResponse<T>.Ok(data, message, HttpContext.TraceIdentifier);
    private ApiResponse<object> Fail(string message) => ApiResponse<object>.Fail(message, traceId: HttpContext.TraceIdentifier);
}

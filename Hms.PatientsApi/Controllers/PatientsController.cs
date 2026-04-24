using Hms.PatientsApi.DTOs.Patients;
using Hms.PatientsApi.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hms.PatientsApi.Controllers;

[ApiController]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(PatientResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePatientRequestDto request)
    {
        var result = await _patientService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PatientResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _patientService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("by-uhid/{uhid}")]
    [ProducesResponseType(typeof(PatientResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUhid(string uhid)
    {
        var result = await _patientService.GetByUhidAsync(uhid);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("search")]
    [ProducesResponseType(typeof(PatientSearchResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromBody] PatientSearchRequestDto request)
    {
        var result = await _patientService.SearchAsync(request);
        return Ok(result);
    }

    [HttpPost("{id:int}/mobile-number/send-otp")]
    public async Task<IActionResult> SendMobileNumberChangeOtp(int id, [FromBody] RequestMobileNumberChangeOtpDto request)
    {
        var result = await _patientService.SendMobileNumberChangeOtpAsync(id, request);
        return Ok(result);
    }

    [HttpPost("{id:int}/mobile-number/verify-otp")]
    public async Task<IActionResult> VerifyMobileNumberChangeOtp(int id, [FromBody] VerifyMobileNumberChangeOtpDto request)
    {
        var result = await _patientService.VerifyMobileNumberChangeOtpAsync(id, request);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PatientResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientRequestDto request)
    {
        var result = await _patientService.UpdateAsync(id, request);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _patientService.SoftDeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
    [HttpPut("{id:int}/complete-profile")]
    public async Task<IActionResult> CompleteProfile(
    int id,
    [FromBody] CompletePatientProfileRequestDto request)
    {
        var result = await _patientService.CompleteProfileAsync(id, request);
        if (result == null) return NotFound();
        return Ok(result);
    }
}

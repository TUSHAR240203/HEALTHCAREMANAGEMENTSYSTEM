<<<<<<< HEAD
using Hms.ReceptionApi.DTOs.Reception;
=======
﻿using Hms.ReceptionApi.DTOs.Reception;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
using Hms.ReceptionApi.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hms.ReceptionApi.Controllers;

[ApiController]
[Route("api/reception/queue")]
public class QueueController : ControllerBase
{
    private readonly IQueueService _queueService;

    public QueueController(IQueueService queueService)
    {
        _queueService = queueService;
    }

    [HttpGet("{departmentId:int}")]
    public async Task<IActionResult> GetQueue(int departmentId, [FromQuery] DateOnly date)
    {
        var result = await _queueService.GetDepartmentQueueAsync(departmentId, date);
        return Ok(result);
    }

<<<<<<< HEAD
    [HttpGet("doctor/{doctorId:int}")]
    public async Task<IActionResult> GetDoctorQueue(int doctorId, [FromQuery] DateOnly date)
    {
        var result = await _queueService.GetDoctorQueueAsync(doctorId, date);
        return Ok(result);
    }

=======
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    [HttpGet("{departmentId:int}/current")]
    public async Task<IActionResult> GetCurrent(int departmentId, [FromQuery] DateOnly date)
    {
        var result = await _queueService.GetCurrentAsync(departmentId, date);
        if (result == null) return NotFound();
        return Ok(result);
    }

<<<<<<< HEAD
    [HttpGet("doctor/{doctorId:int}/current")]
    public async Task<IActionResult> GetDoctorCurrent(int doctorId, [FromQuery] DateOnly date)
    {
        var result = await _queueService.GetDoctorCurrentAsync(doctorId, date);
        if (result == null) return NotFound();
        return Ok(result);
    }

=======
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    [HttpPost("{departmentId:int}/next")]
    public async Task<IActionResult> CallNext(int departmentId, [FromQuery] DateOnly date)
    {
        var result = await _queueService.CallNextAsync(departmentId, date);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("token/{queueTokenId:int}/start")]
    public async Task<IActionResult> Start(int queueTokenId)
    {
        var result = await _queueService.StartAsync(queueTokenId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("token/{queueTokenId:int}/complete")]
    public async Task<IActionResult> Complete(int queueTokenId, [FromBody] CompleteQueueTokenRequestDto request)
    {
        var result = await _queueService.CompleteAsync(queueTokenId, request);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("token/{queueTokenId:int}/skip")]
    public async Task<IActionResult> Skip(int queueTokenId)
    {
        var result = await _queueService.SkipAsync(queueTokenId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("token/{queueTokenId:int}/recall")]
    public async Task<IActionResult> Recall(int queueTokenId)
    {
        var result = await _queueService.RecallAsync(queueTokenId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("token/{queueTokenId:int}/cancel")]
    public async Task<IActionResult> Cancel(int queueTokenId, [FromBody] CancelQueueTokenRequestDto request)
    {
        var result = await _queueService.CancelAsync(queueTokenId, request);
        if (result == null) return NotFound();
        return Ok(result);
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85

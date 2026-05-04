using Hms.ReceptionApi.DTOs.Reception;
using Hms.ReceptionApi.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hms.ReceptionApi.Controllers;

[ApiController]
[Route("api/reception/queue")]
[Authorize]
public class QueueController : ControllerBase
{
    private readonly IQueueService _queueService;

    public QueueController(IQueueService queueService)
    {
        _queueService = queueService;
    }

    [HttpGet("{departmentId:int}")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public async Task<IActionResult> GetQueue(int departmentId, [FromQuery] DateOnly date)
    {
        var result = await _queueService.GetDepartmentQueueAsync(departmentId, date);
        return Ok(result);
    }

    [HttpGet("{departmentId:int}/current")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public async Task<IActionResult> GetCurrent(int departmentId, [FromQuery] DateOnly date)
    {
        var result = await _queueService.GetCurrentAsync(departmentId, date);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("{departmentId:int}/next")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public async Task<IActionResult> CallNext(int departmentId, [FromQuery] DateOnly date)
    {
        var result = await _queueService.CallNextAsync(departmentId, date);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("token/{queueTokenId:int}/start")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public async Task<IActionResult> Start(int queueTokenId)
    {
        var result = await _queueService.StartAsync(queueTokenId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("token/{queueTokenId:int}/complete")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public async Task<IActionResult> Complete(
        int queueTokenId,
        [FromBody] CompleteQueueTokenRequestDto request)
    {
        var result = await _queueService.CompleteAsync(queueTokenId, request);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("token/{queueTokenId:int}/skip")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public async Task<IActionResult> Skip(int queueTokenId)
    {
        var result = await _queueService.SkipAsync(queueTokenId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("token/{queueTokenId:int}/recall")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public async Task<IActionResult> Recall(int queueTokenId)
    {
        var result = await _queueService.RecallAsync(queueTokenId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("token/{queueTokenId:int}/cancel")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Cancel(
        int queueTokenId,
        [FromBody] CancelQueueTokenRequestDto request)
    {
        var result = await _queueService.CancelAsync(queueTokenId, request);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
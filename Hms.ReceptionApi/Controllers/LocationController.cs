using Hms.ReceptionApi.Interfaces.Clients;
using Microsoft.AspNetCore.Mvc;

namespace Hms.ReceptionApi.Controllers;

[ApiController]
[Route("api/reception/locations")]
public class LocationController : ControllerBase
{
    private readonly ILocationApiClient _locationApiClient;

    public LocationController(ILocationApiClient locationApiClient)
    {
        _locationApiClient = locationApiClient;
    }

    [HttpGet("states")]
    public async Task<IActionResult> GetStates([FromQuery] string country = "India")
    {
        var result = await _locationApiClient.GetStatesAsync(country);
        return Ok(result);
    }

    [HttpGet("cities")]
    public async Task<IActionResult> GetCities([FromQuery] string country = "India", [FromQuery] string state = "")
    {
        if (string.IsNullOrWhiteSpace(state))
            return BadRequest(new { message = "State is required." });

        var result = await _locationApiClient.GetCitiesAsync(country, state);
        return Ok(result);
    }
}
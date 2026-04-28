using Hms.ReceptionApi.Controllers;
using Hms.ReceptionApi.DTOs.Location;
using Hms.ReceptionApi.Interfaces.Clients;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Hms.ReceptionApi.Tests.Controllers;

public class LocationControllerTests
{
    private readonly Mock<ILocationApiClient> _locationApiClientMock;
    private readonly LocationController _controller;

    public LocationControllerTests()
    {
        _locationApiClientMock = new Mock<ILocationApiClient>();
        _controller = new LocationController(_locationApiClientMock.Object);
    }

    [Fact]
    public async Task GetStates_ReturnsOkResult()
    {
        var states = new List<StateDto>
        {
            new StateDto { Id = 1, Name = "Punjab" },
            new StateDto { Id = 2, Name = "Haryana" }
        };

        _locationApiClientMock
            .Setup(x => x.GetStatesAsync("India"))
            .ReturnsAsync(states);

        var result = await _controller.GetStates("India");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(states, okResult.Value);
    }

    [Fact]
    public async Task GetCities_ReturnsBadRequest_WhenStateMissing()
    {
        var result = await _controller.GetCities("India", "");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetCities_ReturnsOkResult_WhenStateValid()
    {
        var cities = new List<CityDto>
        {
            new CityDto { Id = 1, Name = "Mohali" },
            new CityDto { Id = 2, Name = "Chandigarh" }
        };

        _locationApiClientMock
            .Setup(x => x.GetCitiesAsync("India", "Punjab"))
            .ReturnsAsync(cities);

        var result = await _controller.GetCities("India", "Punjab");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(cities, okResult.Value);
    }
}
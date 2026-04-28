using Hms.PatientsApi.Controllers;
using Hms.PatientsApi.DTOs.Patients;
using Hms.PatientsApi.Interfaces.Services;
using Hms.PatientsApi.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Hms.PatientsApi.Tests.Controllers;

public class PatientsControllerTests
{
    private readonly Mock<IPatientService> _serviceMock;
    private readonly PatientsController _controller;

    public PatientsControllerTests()
    {
        _serviceMock = new Mock<IPatientService>();
        _controller = new PatientsController(_serviceMock.Object);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction()
    {
        var request = MockData.CreateRequest();
        var response = MockData.PatientResponse();

        _serviceMock.Setup(x => x.CreateAsync(request)).ReturnsAsync(response);

        var result = await _controller.Create(request);

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenPatientExists()
    {
        _serviceMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(MockData.PatientResponse());

        var result = await _controller.GetById(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenPatientNotFound()
    {
        _serviceMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((PatientResponseDto?)null);

        var result = await _controller.GetById(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetByUhid_ShouldReturnOk_WhenPatientExists()
    {
        _serviceMock.Setup(x => x.GetByUhidAsync("UHID001")).ReturnsAsync(MockData.PatientResponse());

        var result = await _controller.GetByUhid("UHID001");

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Search_ShouldReturnOk()
    {
        var request = new PatientSearchRequestDto();

        var response = new PatientSearchResponseDto
        {
            TotalCount = 1,
            Patients = new List<PatientResponseDto> { MockData.PatientResponse() }
        };

        _serviceMock.Setup(x => x.SearchAsync(request)).ReturnsAsync(response);

        var result = await _controller.Search(request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenPatientExists()
    {
        var request = MockData.UpdateRequest();

        _serviceMock.Setup(x => x.UpdateAsync(1, request)).ReturnsAsync(MockData.PatientResponse());

        var result = await _controller.Update(1, request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenPatientNotFound()
    {
        var request = MockData.UpdateRequest();

        _serviceMock.Setup(x => x.UpdateAsync(1, request)).ReturnsAsync((PatientResponseDto?)null);

        var result = await _controller.Update(1, request);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenDeleted()
    {
        _serviceMock.Setup(x => x.SoftDeleteAsync(1)).ReturnsAsync(true);

        var result = await _controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenNotDeleted()
    {
        _serviceMock.Setup(x => x.SoftDeleteAsync(1)).ReturnsAsync(false);

        var result = await _controller.Delete(1);

        Assert.IsType<NotFoundResult>(result);
    }
}
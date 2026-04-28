using FluentAssertions;
using Hms.DoctorsApi.Common;
using Hms.DoctorsApi.Controllers;
using Hms.DoctorsApi.DTOs.Doctors;
using Hms.DoctorsApi.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Hms.DoctorsApi.Tests.Controllers;

public class DoctorsControllerTests
{
    [Fact]
    public async Task GetById_WhenDoctorExists_ReturnsOkWithWrappedResponse()
    {
        var service = new Mock<IDoctorService>();
        service.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new DoctorResponseDto
        {
            Id = 1,
            DoctorCode = "DOC-TEST",
            FullName = "Dr Test",
            Specialization = "Cardiology",
            DepartmentId = 1,
            DepartmentName = "Cardiology"
        });
        var controller = CreateController(service.Object);

        var actionResult = await controller.GetById(1);

        var ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeAssignableTo<ApiResponse<DoctorResponseDto>>().Subject;
        body.Success.Should().BeTrue();
        body.Data!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_WhenDoctorDoesNotExist_ReturnsNotFound()
    {
        var service = new Mock<IDoctorService>();
        service.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((DoctorResponseDto?)null);
        var controller = CreateController(service.Object);

        var actionResult = await controller.GetById(99);

        var notFound = actionResult.Should().BeOfType<NotFoundObjectResult>().Subject;
        var body = notFound.Value.Should().BeAssignableTo<ApiResponse<object>>().Subject;
        body.Success.Should().BeFalse();
        body.Message.Should().Be("Doctor not found.");
    }

    private static DoctorsController CreateController(IDoctorService service) => new(service)
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { TraceIdentifier = "test-trace" }
        }
    };
}

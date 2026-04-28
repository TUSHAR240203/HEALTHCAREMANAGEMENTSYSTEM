using Hms.ReceptionApi.Controllers;
using Hms.ReceptionApi.DTOs.Reception;
using Hms.ReceptionApi.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Hms.ReceptionApi.Tests.Controllers;

public class QueueControllerTests
{
    private readonly Mock<IQueueService> _queueServiceMock;
    private readonly QueueController _controller;

    public QueueControllerTests()
    {
        _queueServiceMock = new Mock<IQueueService>();
        _controller = new QueueController(_queueServiceMock.Object);
    }

    [Fact]
    public async Task GetQueue_ShouldReturnOk()
    {
        var date = DateOnly.FromDateTime(DateTime.Today);

        var response = new DepartmentQueueResponseDto
        {
            DepartmentId = 1,
            DepartmentName = "Cardiology",
            Date = date,
            Queue = new List<QueueItemDto>()
        };

        _queueServiceMock
            .Setup(x => x.GetDepartmentQueueAsync(1, date))
            .ReturnsAsync(response);

        var result = await _controller.GetQueue(1, date);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task GetCurrent_ShouldReturnOk_WhenCurrentExists()
    {
        var date = DateOnly.FromDateTime(DateTime.Today);

        var response = new QueueCurrentResponseDto
        {
            QueueTokenId = 1,
            TokenNumber = 101,
            PatientId = 1,
            UHID = "UHID001",
            PatientName = "Tushar Sharma",
            Status = "Called"
        };

        _queueServiceMock
            .Setup(x => x.GetCurrentAsync(1, date))
            .ReturnsAsync(response);

        var result = await _controller.GetCurrent(1, date);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task GetCurrent_ShouldReturnNotFound_WhenCurrentNotExists()
    {
        var date = DateOnly.FromDateTime(DateTime.Today);

        _queueServiceMock
            .Setup(x => x.GetCurrentAsync(1, date))
            .ReturnsAsync((QueueCurrentResponseDto?)null);

        var result = await _controller.GetCurrent(1, date);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CallNext_ShouldReturnOk_WhenTokenExists()
    {
        var date = DateOnly.FromDateTime(DateTime.Today);

        var response = GetQueueActionResponse();

        _queueServiceMock
            .Setup(x => x.CallNextAsync(1, date))
            .ReturnsAsync(response);

        var result = await _controller.CallNext(1, date);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task CallNext_ShouldReturnNotFound_WhenTokenNotExists()
    {
        var date = DateOnly.FromDateTime(DateTime.Today);

        _queueServiceMock
            .Setup(x => x.CallNextAsync(1, date))
            .ReturnsAsync((QueueActionResponseDto?)null);

        var result = await _controller.CallNext(1, date);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Start_ShouldReturnOk_WhenTokenExists()
    {
        var response = GetQueueActionResponse();

        _queueServiceMock
            .Setup(x => x.StartAsync(1))
            .ReturnsAsync(response);

        var result = await _controller.Start(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task Start_ShouldReturnNotFound_WhenTokenNotExists()
    {
        _queueServiceMock
            .Setup(x => x.StartAsync(1))
            .ReturnsAsync((QueueActionResponseDto?)null);

        var result = await _controller.Start(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Complete_ShouldReturnOk_WhenTokenExists()
    {
        var request = new CompleteQueueTokenRequestDto
        {
            Notes = "Done"
        };

        var response = GetQueueActionResponse();

        _queueServiceMock
            .Setup(x => x.CompleteAsync(1, request))
            .ReturnsAsync(response);

        var result = await _controller.Complete(1, request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task Complete_ShouldReturnNotFound_WhenTokenNotExists()
    {
        var request = new CompleteQueueTokenRequestDto
        {
            Notes = "Done"
        };

        _queueServiceMock
            .Setup(x => x.CompleteAsync(1, request))
            .ReturnsAsync((QueueActionResponseDto?)null);

        var result = await _controller.Complete(1, request);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Skip_ShouldReturnOk_WhenTokenExists()
    {
        var response = GetQueueActionResponse();

        _queueServiceMock
            .Setup(x => x.SkipAsync(1))
            .ReturnsAsync(response);

        var result = await _controller.Skip(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task Skip_ShouldReturnNotFound_WhenTokenNotExists()
    {
        _queueServiceMock
            .Setup(x => x.SkipAsync(1))
            .ReturnsAsync((QueueActionResponseDto?)null);

        var result = await _controller.Skip(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Recall_ShouldReturnOk_WhenTokenExists()
    {
        var response = GetQueueActionResponse();

        _queueServiceMock
            .Setup(x => x.RecallAsync(1))
            .ReturnsAsync(response);

        var result = await _controller.Recall(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task Recall_ShouldReturnNotFound_WhenTokenNotExists()
    {
        _queueServiceMock
            .Setup(x => x.RecallAsync(1))
            .ReturnsAsync((QueueActionResponseDto?)null);

        var result = await _controller.Recall(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Cancel_ShouldReturnOk_WhenTokenExists()
    {
        var request = new CancelQueueTokenRequestDto
        {
            Notes = "Patient cancelled"
        };

        var response = GetQueueActionResponse();

        _queueServiceMock
            .Setup(x => x.CancelAsync(1, request))
            .ReturnsAsync(response);

        var result = await _controller.Cancel(1, request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task Cancel_ShouldReturnNotFound_WhenTokenNotExists()
    {
        var request = new CancelQueueTokenRequestDto
        {
            Notes = "Patient cancelled"
        };

        _queueServiceMock
            .Setup(x => x.CancelAsync(1, request))
            .ReturnsAsync((QueueActionResponseDto?)null);

        var result = await _controller.Cancel(1, request);

        Assert.IsType<NotFoundResult>(result);
    }

    private static QueueActionResponseDto GetQueueActionResponse()
    {
        return new QueueActionResponseDto
        {
            QueueTokenId = 1,
            TokenNumber = 101,
            PatientId = 1,
            UHID = "UHID001",
            PatientName = "Tushar Sharma",
            Status = "Called",
            Message = "Success"
        };
    }
}
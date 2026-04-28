using Hms.ReceptionApi.DTOs.Reception;
using Hms.ReceptionApi.Entities;
using Hms.ReceptionApi.Interfaces.Repository;
using Hms.ReceptionApi.Services;
using Moq;
using Xunit;

namespace Hms.ReceptionApi.Tests.Services;

public class QueueServiceTests
{
    private readonly Mock<IQueueRepository> _queueRepositoryMock;
    private readonly QueueService _service;

    public QueueServiceTests()
    {
        _queueRepositoryMock = new Mock<IQueueRepository>();
        _service = new QueueService(_queueRepositoryMock.Object);
    }

    [Fact]
    public async Task GetDepartmentQueueAsync_ShouldReturnDepartmentQueue()
    {
        var date = DateOnly.FromDateTime(DateTime.Today);

        var tokens = new List<QueueToken>
        {
            GetToken(1, 1, "Waiting"),
            GetToken(2, 1, "Called")
        };

        _queueRepositoryMock
            .Setup(x => x.GetDepartmentQueueAsync(1, date))
            .ReturnsAsync(tokens);

        var result = await _service.GetDepartmentQueueAsync(1, date);

        Assert.NotNull(result);
        Assert.Equal(1, result.DepartmentId);
        Assert.Equal(2, result.Queue.Count);
    }

    [Fact]
    public async Task GetCurrentAsync_ShouldReturnNull_WhenTokenNotFound()
    {
        var date = DateOnly.FromDateTime(DateTime.Today);

        _queueRepositoryMock
            .Setup(x => x.GetCurrentAsync(1, date))
            .ReturnsAsync((QueueToken?)null);

        var result = await _service.GetCurrentAsync(1, date);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentAsync_ShouldReturnCurrentToken_WhenTokenExists()
    {
        var date = DateOnly.FromDateTime(DateTime.Today);
        var token = GetToken(1, 1, "Called");

        _queueRepositoryMock
            .Setup(x => x.GetCurrentAsync(1, date))
            .ReturnsAsync(token);

        var result = await _service.GetCurrentAsync(1, date);

        Assert.NotNull(result);
        Assert.Equal("Called", result.Status);
        Assert.Equal(1, result.QueueTokenId);
    }

    [Fact]
    public async Task CallNextAsync_ShouldReturnNull_WhenNoWaitingToken()
    {
        var date = DateOnly.FromDateTime(DateTime.Today);

        _queueRepositoryMock
            .Setup(x => x.GetCurrentAsync(1, date))
            .ReturnsAsync((QueueToken?)null);

        _queueRepositoryMock
            .Setup(x => x.GetNextWaitingAsync(1, date))
            .ReturnsAsync((QueueToken?)null);

        var result = await _service.CallNextAsync(1, date);

        Assert.Null(result);
    }

    [Fact]
    public async Task CallNextAsync_ShouldThrow_WhenCurrentTokenAlreadyExists()
    {
        var date = DateOnly.FromDateTime(DateTime.Today);
        var current = GetToken(1, 1, "Called");

        _queueRepositoryMock
            .Setup(x => x.GetCurrentAsync(1, date))
            .ReturnsAsync(current);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CallNextAsync(1, date));
    }

    [Fact]
    public async Task CallNextAsync_ShouldCallNextWaitingToken()
    {
        var date = DateOnly.FromDateTime(DateTime.Today);
        var next = GetToken(1, 1, "Waiting");

        _queueRepositoryMock
            .Setup(x => x.GetCurrentAsync(1, date))
            .ReturnsAsync((QueueToken?)null);

        _queueRepositoryMock
            .Setup(x => x.GetNextWaitingAsync(1, date))
            .ReturnsAsync(next);

        var result = await _service.CallNextAsync(1, date);

        Assert.NotNull(result);
        Assert.Equal("Called", result.Status);
        _queueRepositoryMock.Verify(x => x.UpdateAsync(next), Times.Once);
        _queueRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task StartAsync_ShouldReturnNull_WhenTokenNotFound()
    {
        _queueRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((QueueToken?)null);

        var result = await _service.StartAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task StartAsync_ShouldThrow_WhenTokenIsNotCalled()
    {
        var token = GetToken(1, 1, "Waiting");

        _queueRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(token);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.StartAsync(1));
    }

    [Fact]
    public async Task StartAsync_ShouldStartCalledToken()
    {
        var token = GetToken(1, 1, "Called");

        _queueRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(token);

        var result = await _service.StartAsync(1);

        Assert.NotNull(result);
        Assert.Equal("InProgress", result.Status);
        _queueRepositoryMock.Verify(x => x.UpdateAsync(token), Times.Once);
        _queueRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_ShouldCompleteActiveToken()
    {
        var token = GetToken(1, 1, "InProgress");

        var request = new CompleteQueueTokenRequestDto
        {
            Notes = "Done"
        };

        _queueRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(token);

        var result = await _service.CompleteAsync(1, request);

        Assert.NotNull(result);
        Assert.Equal("Completed", result.Status);
        Assert.Equal("Done", token.Notes);
    }

    [Fact]
    public async Task SkipAsync_ShouldSkipCalledToken()
    {
        var token = GetToken(1, 1, "Called");

        _queueRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(token);

        var result = await _service.SkipAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Skipped", result.Status);
    }

    [Fact]
    public async Task RecallAsync_ShouldRecallSkippedToken()
    {
        var token = GetToken(1, 1, "Skipped");

        _queueRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(token);

        var result = await _service.RecallAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Called", result.Status);
    }

    [Fact]
    public async Task CancelAsync_ShouldCancelToken()
    {
        var token = GetToken(1, 1, "Waiting");

        var request = new CancelQueueTokenRequestDto
        {
            Notes = "Patient not available"
        };

        _queueRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(token);

        var result = await _service.CancelAsync(1, request);

        Assert.NotNull(result);
        Assert.Equal("Cancelled", result.Status);
        Assert.Equal("Patient not available", token.Notes);
    }

    private static QueueToken GetToken(int id, int tokenNumber, string status)
    {
        return new QueueToken
        {
            Id = id,
            DepartmentId = 1,
            QueueDate = DateOnly.FromDateTime(DateTime.Today),
            TokenNumber = tokenNumber,
            PatientId = 1,
            UHID = "UHID001",
            PatientName = "Tushar Sharma",
            AppointmentId = 1,
            DoctorId = 1,
            Status = status
        };
    }
}
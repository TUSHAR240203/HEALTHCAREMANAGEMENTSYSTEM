using Hms.ReceptionApi.Entities;
using Hms.ReceptionApi.Repositories;
using Hms.ReceptionApi.Tests.TestHelpers;
using Xunit;

namespace Hms.ReceptionApi.Tests.Repositories;

public class QueueRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldAddQueueToken()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new QueueRepository(context);

        var token = GetQueueToken(1, 1, "Waiting");

        await repo.AddAsync(token);
        await repo.SaveChangesAsync();

        Assert.Single(context.QueueTokens);
    }

    [Fact]
    public async Task GetNextTokenNumberAsync_ShouldReturnOne_WhenNoTokenExists()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new QueueRepository(context);

        var date = DateOnly.FromDateTime(DateTime.Today);

        var result = await repo.GetNextTokenNumberAsync(1, date);

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task GetNextTokenNumberAsync_ShouldReturnNextNumber_WhenTokensExist()
    {
        using var context = TestDbContextFactory.Create();
        var date = DateOnly.FromDateTime(DateTime.Today);

        context.QueueTokens.Add(GetQueueToken(1, 1, "Waiting", date));
        context.QueueTokens.Add(GetQueueToken(1, 2, "Waiting", date));
        context.QueueTokens.Add(GetQueueToken(1, 5, "Waiting", date));
        await context.SaveChangesAsync();

        var repo = new QueueRepository(context);

        var result = await repo.GetNextTokenNumberAsync(1, date);

        Assert.Equal(6, result);
    }

    [Fact]
    public async Task GetDepartmentQueueAsync_ShouldReturnOnlyMatchingDepartmentQueue()
    {
        using var context = TestDbContextFactory.Create();
        var date = DateOnly.FromDateTime(DateTime.Today);

        context.QueueTokens.Add(GetQueueToken(1, 1, "Waiting", date));
        context.QueueTokens.Add(GetQueueToken(1, 2, "Called", date));
        context.QueueTokens.Add(GetQueueToken(2, 1, "Waiting", date));
        await context.SaveChangesAsync();

        var repo = new QueueRepository(context);

        var result = await repo.GetDepartmentQueueAsync(1, date);

        Assert.Equal(2, result.Count);
        Assert.All(result, x => Assert.Equal(1, x.DepartmentId));
    }

    [Fact]
    public async Task GetDepartmentQueueAsync_ShouldIgnoreDeletedTokens()
    {
        using var context = TestDbContextFactory.Create();
        var date = DateOnly.FromDateTime(DateTime.Today);

        var active = GetQueueToken(1, 1, "Waiting", date);
        var deleted = GetQueueToken(1, 2, "Waiting", date);
        deleted.IsDeleted = true;

        context.QueueTokens.Add(active);
        context.QueueTokens.Add(deleted);
        await context.SaveChangesAsync();

        var repo = new QueueRepository(context);

        var result = await repo.GetDepartmentQueueAsync(1, date);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnToken_WhenExists()
    {
        using var context = TestDbContextFactory.Create();

        var token = GetQueueToken(1, 1, "Waiting");
        context.QueueTokens.Add(token);
        await context.SaveChangesAsync();

        var repo = new QueueRepository(context);

        var result = await repo.GetByIdAsync(token.Id);

        Assert.NotNull(result);
        Assert.Equal(token.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDeleted()
    {
        using var context = TestDbContextFactory.Create();

        var token = GetQueueToken(1, 1, "Waiting");
        token.IsDeleted = true;

        context.QueueTokens.Add(token);
        await context.SaveChangesAsync();

        var repo = new QueueRepository(context);

        var result = await repo.GetByIdAsync(token.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentAsync_ShouldReturnCalledOrInProgressToken()
    {
        using var context = TestDbContextFactory.Create();
        var date = DateOnly.FromDateTime(DateTime.Today);

        context.QueueTokens.Add(GetQueueToken(1, 1, "Waiting", date));
        context.QueueTokens.Add(GetQueueToken(1, 2, "Called", date));
        context.QueueTokens.Add(GetQueueToken(1, 3, "InProgress", date));
        await context.SaveChangesAsync();

        var repo = new QueueRepository(context);

        var result = await repo.GetCurrentAsync(1, date);

        Assert.NotNull(result);
        Assert.Equal("Called", result.Status);
    }

    [Fact]
    public async Task GetNextWaitingAsync_ShouldReturnFirstWaitingToken()
    {
        using var context = TestDbContextFactory.Create();
        var date = DateOnly.FromDateTime(DateTime.Today);

        context.QueueTokens.Add(GetQueueToken(1, 3, "Waiting", date));
        context.QueueTokens.Add(GetQueueToken(1, 1, "Waiting", date));
        context.QueueTokens.Add(GetQueueToken(1, 2, "Called", date));
        await context.SaveChangesAsync();

        var repo = new QueueRepository(context);

        var result = await repo.GetNextWaitingAsync(1, date);

        Assert.NotNull(result);
        Assert.Equal(1, result.TokenNumber);
        Assert.Equal("Waiting", result.Status);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateQueueToken()
    {
        using var context = TestDbContextFactory.Create();

        var token = GetQueueToken(1, 1, "Waiting");
        context.QueueTokens.Add(token);
        await context.SaveChangesAsync();

        var repo = new QueueRepository(context);

        token.Status = "Called";
        await repo.UpdateAsync(token);
        await repo.SaveChangesAsync();

        var result = await repo.GetByIdAsync(token.Id);

        Assert.NotNull(result);
        Assert.Equal("Called", result.Status);
    }

    private static QueueToken GetQueueToken(
        int departmentId,
        int tokenNumber,
        string status,
        DateOnly? date = null)
    {
        return new QueueToken
        {
            DepartmentId = departmentId,
            QueueDate = date ?? DateOnly.FromDateTime(DateTime.Today),
            TokenNumber = tokenNumber,
            PatientId = tokenNumber,
            UHID = $"UHID00{tokenNumber}",
            PatientName = "Tushar Sharma",
            AppointmentId = tokenNumber,
            DoctorId = 1,
            Status = status,
            IsDeleted = false
        };
    }
}
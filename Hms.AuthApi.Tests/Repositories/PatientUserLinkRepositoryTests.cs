using Hms.AuthApi.Entities;
using Hms.AuthApi.Repositories;
using Hms.AuthApi.Tests.TestHelpers;
using Xunit;

namespace Hms.AuthApi.Tests.Repositories;

public class PatientUserLinkRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldAddPatientUserLink()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new PatientUserLinkRepository(context);

        var link = new PatientUserLink
        {
            PatientId = 1,
            UserId = 1,
            UHID = "UHID001"
        };

        await repo.AddAsync(link);
        await repo.SaveChangesAsync();

        Assert.Single(context.PatientUserLinks);
    }

    [Fact]
    public async Task GetByPatientIdAsync_ShouldReturnLink_WhenExists()
    {
        using var context = TestDbContextFactory.Create();

        context.PatientUserLinks.Add(new PatientUserLink
        {
            PatientId = 1,
            UserId = 1,
            UHID = "UHID001",
            IsDeleted = false
        });

        await context.SaveChangesAsync();

        var repo = new PatientUserLinkRepository(context);

        var result = await repo.GetByPatientIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.PatientId);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnLink_WhenExists()
    {
        using var context = TestDbContextFactory.Create();

        context.PatientUserLinks.Add(new PatientUserLink
        {
            PatientId = 2,
            UserId = 5,
            UHID = "UHID002",
            IsDeleted = false
        });

        await context.SaveChangesAsync();

        var repo = new PatientUserLinkRepository(context);

        var result = await repo.GetByUserIdAsync(5);

        Assert.NotNull(result);
        Assert.Equal(5, result.UserId);
    }

    [Fact]
    public async Task GetByPatientIdAsync_ShouldReturnNull_WhenDeleted()
    {
        using var context = TestDbContextFactory.Create();

        context.PatientUserLinks.Add(new PatientUserLink
        {
            PatientId = 1,
            UserId = 1,
            UHID = "UHID001",
            IsDeleted = true
        });

        await context.SaveChangesAsync();

        var repo = new PatientUserLinkRepository(context);

        var result = await repo.GetByPatientIdAsync(1);

        Assert.Null(result);
    }
}
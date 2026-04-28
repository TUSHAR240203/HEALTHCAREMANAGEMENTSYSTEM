using Hms.AuthApi.Entities;
using Hms.AuthApi.Repositories;
using Hms.AuthApi.Tests.TestHelpers;
using Xunit;

namespace Hms.AuthApi.Tests.Repositories;

public class UserRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldAddUser()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new UserRepository(context);

        var user = new User
        {
            MobileNumber = "9999999999",
            Role = "Patient"
        };

        await repo.AddAsync(user);
        await repo.SaveChangesAsync();

        Assert.Single(context.Users);
    }

    [Fact]
    public async Task GetByMobileAsync_ShouldReturnUser_WhenUserExists()
    {
        using var context = TestDbContextFactory.Create();
        context.Users.Add(new User
        {
            MobileNumber = "9999999999",
            Role = "Patient",
            IsDeleted = false
        });
        await context.SaveChangesAsync();

        var repo = new UserRepository(context);

        var result = await repo.GetByMobileAsync("9999999999");

        Assert.NotNull(result);
        Assert.Equal("9999999999", result.MobileNumber);
    }

    [Fact]
    public async Task GetByMobileAsync_ShouldReturnNull_WhenUserIsDeleted()
    {
        using var context = TestDbContextFactory.Create();
        context.Users.Add(new User
        {
            MobileNumber = "9999999999",
            Role = "Patient",
            IsDeleted = true
        });
        await context.SaveChangesAsync();

        var repo = new UserRepository(context);

        var result = await repo.GetByMobileAsync("9999999999");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        using var context = TestDbContextFactory.Create();
        var user = new User
        {
            MobileNumber = "8888888888",
            Role = "Patient",
            IsDeleted = false
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repo = new UserRepository(context);

        var result = await repo.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }
}
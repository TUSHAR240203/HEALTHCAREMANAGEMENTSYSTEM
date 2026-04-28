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

        var role = new Role
        {
            Name = "Patient",
            NormalizedName = "PATIENT"
        };

        var user = new User
        {
            MobileNumber = "9999999999",
            LoginId = "9999999999",
            IsActive = true,
            IsDeleted = false
        };

        user.UserRoles.Add(new UserRole
        {
            User = user,
            Role = role
        });

        await repo.AddAsync(user);
        await repo.SaveChangesAsync();

        Assert.Single(context.Users);
        Assert.Single(context.Roles);
        Assert.Single(context.UserRoles);
    }

    [Fact]
    public async Task GetByMobileWithRolesAsync_ShouldReturnUser_WhenUserExists()
    {
        using var context = TestDbContextFactory.Create();

        var role = new Role
        {
            Name = "Patient",
            NormalizedName = "PATIENT"
        };

        var user = new User
        {
            MobileNumber = "9999999999",
            LoginId = "9999999999",
            IsActive = true,
            IsDeleted = false
        };

        user.UserRoles.Add(new UserRole
        {
            User = user,
            Role = role
        });

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repo = new UserRepository(context);

        var result = await repo.GetByMobileWithRolesAsync("9999999999");

        Assert.NotNull(result);
        Assert.Equal("9999999999", result.MobileNumber);
        Assert.Single(result.UserRoles);
        Assert.Equal("Patient", result.UserRoles.First().Role.Name);
    }

    [Fact]
    public async Task GetByMobileWithRolesAsync_ShouldReturnNull_WhenUserIsDeleted()
    {
        using var context = TestDbContextFactory.Create();

        var role = new Role
        {
            Name = "Patient",
            NormalizedName = "PATIENT"
        };

        var user = new User
        {
            MobileNumber = "9999999999",
            LoginId = "9999999999",
            IsActive = true,
            IsDeleted = true
        };

        user.UserRoles.Add(new UserRole
        {
            User = user,
            Role = role
        });

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repo = new UserRepository(context);

        var result = await repo.GetByMobileWithRolesAsync("9999999999");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdWithRolesAsync_ShouldReturnUser_WhenUserExists()
    {
        using var context = TestDbContextFactory.Create();

        var role = new Role
        {
            Name = "Patient",
            NormalizedName = "PATIENT"
        };

        var user = new User
        {
            MobileNumber = "8888888888",
            LoginId = "8888888888",
            IsActive = true,
            IsDeleted = false
        };

        user.UserRoles.Add(new UserRole
        {
            User = user,
            Role = role
        });

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repo = new UserRepository(context);

        var result = await repo.GetByIdWithRolesAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Single(result.UserRoles);
        Assert.Equal("Patient", result.UserRoles.First().Role.Name);
    }
}
using Hms.AuthApi.Entities;
using Hms.AuthApi.Repositories;
using Hms.AuthApi.Tests.TestHelpers;
using Xunit;

namespace Hms.AuthApi.Tests.Repositories;

public class OtpRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldAddOtp()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new OtpRepository(context);

        var otp = new OtpVerification
        {
            PatientId = 1,
            MobileNumber = "9999999999",
            OtpCode = "123456",
            Purpose = "Activation",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5)
        };

        await repo.AddAsync(otp);
        await repo.SaveChangesAsync();

        Assert.Single(context.OtpVerifications);
    }

    [Fact]
    public async Task GetValidOtpAsync_ShouldReturnOtp_WhenOtpIsValid()
    {
        using var context = TestDbContextFactory.Create();

        context.OtpVerifications.Add(new OtpVerification
        {
            PatientId = 1,
            MobileNumber = "9999999999",
            OtpCode = "123456",
            Purpose = "Activation",
            IsUsed = false,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5)
        });

        await context.SaveChangesAsync();

        var repo = new OtpRepository(context);

        var result = await repo.GetValidOtpAsync(
            1,
            "9999999999",
            "123456",
            "Activation"
        );

        Assert.NotNull(result);
        Assert.Equal("123456", result.OtpCode);
    }

    [Fact]
    public async Task GetValidOtpAsync_ShouldReturnNull_WhenOtpIsUsed()
    {
        using var context = TestDbContextFactory.Create();

        context.OtpVerifications.Add(new OtpVerification
        {
            PatientId = 1,
            MobileNumber = "9999999999",
            OtpCode = "123456",
            Purpose = "Activation",
            IsUsed = true,
            IsDeleted = false,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5)
        });

        await context.SaveChangesAsync();

        var repo = new OtpRepository(context);

        var result = await repo.GetValidOtpAsync(
            1,
            "9999999999",
            "123456",
            "Activation"
        );

        Assert.Null(result);
    }

    [Fact]
    public async Task GetValidOtpAsync_ShouldReturnNull_WhenOtpExpired()
    {
        using var context = TestDbContextFactory.Create();

        context.OtpVerifications.Add(new OtpVerification
        {
            PatientId = 1,
            MobileNumber = "9999999999",
            OtpCode = "123456",
            Purpose = "Activation",
            IsUsed = false,
            IsDeleted = false,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-5)
        });

        await context.SaveChangesAsync();

        var repo = new OtpRepository(context);

        var result = await repo.GetValidOtpAsync(
            1,
            "9999999999",
            "123456",
            "Activation"
        );

        Assert.Null(result);
    }
}
using Hms.AuthApi.Services;
using Xunit;

namespace Hms.AuthApi.Tests.Services;

public class OtpServiceTests
{
    [Fact]
    public void GenerateOtp_ShouldReturnSixDigitOtp()
    {
        var service = new OtpService();

        var otp = service.GenerateOtp();

        Assert.False(string.IsNullOrWhiteSpace(otp));
        Assert.Equal(6, otp.Length);
        Assert.True(int.TryParse(otp, out _));
    }

    [Fact]
    public void CreateOtp_ShouldReturnOtpVerification()
    {
        var service = new OtpService();

        var result = service.CreateOtp(1, "9999999999", "Login");

        Assert.Equal(1, result.PatientId);
        Assert.Equal("9999999999", result.MobileNumber);
        Assert.Equal("Login", result.Purpose);
        Assert.False(result.IsUsed);
        Assert.True(result.ExpiresAtUtc > DateTime.UtcNow);
    }
}
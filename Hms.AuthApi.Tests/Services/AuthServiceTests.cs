using Hms.AuthApi.Clients;
using Hms.AuthApi.Common;
using Hms.AuthApi.DTOs.Auth;
using Hms.AuthApi.Entities;
using Hms.AuthApi.Interfaces.Clients;
using Hms.AuthApi.Interfaces.Repository;
using Hms.AuthApi.Interfaces.Services;
using Hms.AuthApi.Services;
using Moq;
using Xunit;

namespace Hms.AuthApi.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IPatientsApiClient> _patientsApiClientMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IOtpRepository> _otpRepositoryMock = new();
    private readonly Mock<IPatientUserLinkRepository> _linkRepositoryMock = new();
    private readonly Mock<IOtpService> _otpServiceMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();

    private AuthService CreateService()
    {
        return new AuthService(
            _patientsApiClientMock.Object,
            _userRepositoryMock.Object,
            _otpRepositoryMock.Object,
            _linkRepositoryMock.Object,
            _otpServiceMock.Object,
            _jwtServiceMock.Object
        );
    }

    [Fact]
    public async Task SendPortalActivationAsync_ShouldThrow_WhenPatientIdInvalid()
    {
        var service = CreateService();

        var request = new SendPatientPortalActivationRequestDto
        {
            PatientId = 0
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SendPortalActivationAsync(request));
    }

    [Fact]
    public async Task SendPortalActivationAsync_ShouldCreateOtp_WhenPatientIsValid()
    {
        var service = CreateService();

        var request = new SendPatientPortalActivationRequestDto
        {
            PatientId = 1
        };

        var patient = GetPatient();

        var otp = new OtpVerification
        {
            PatientId = 1,
            MobileNumber = "9999999999",
            OtpCode = "123456",
            Purpose = "PortalActivation",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5)
        };

        _patientsApiClientMock
            .Setup(x => x.GetPatientByIdAsync(1))
            .ReturnsAsync(patient);

        _otpServiceMock
            .Setup(x => x.CreateOtp(1, "9999999999", "PortalActivation"))
            .Returns(otp);

        await service.SendPortalActivationAsync(request);

        _otpRepositoryMock.Verify(x => x.AddAsync(otp), Times.Once);
        _otpRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task VerifyOtpAndActivateAsync_ShouldReturnAuthResponse_WhenOtpIsValid()
    {
        var service = CreateService();

        var request = new VerifyOtpRequestDto
        {
            PatientId = 1,
            MobileNumber = "9999999999",
            OtpCode = "123456",
            Purpose = "PortalActivation"
        };

        var patient = GetPatient();

        var otp = new OtpVerification
        {
            PatientId = 1,
            MobileNumber = "9999999999",
            OtpCode = "123456",
            Purpose = "PortalActivation",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false
        };

        var user = new User
        {
            Id = 5,
            MobileNumber = "9999999999",
            Role = AppRoles.Patient,
            IsActive = true
        };

        var link = new PatientUserLink
        {
            Id = 2,
            PatientId = 1,
            UserId = 5,
            UHID = "UHID001",
            PortalActivated = false
        };

        _patientsApiClientMock
            .Setup(x => x.GetPatientByIdAsync(1))
            .ReturnsAsync(patient);

        _otpRepositoryMock
            .Setup(x => x.GetValidOtpAsync(1, "9999999999", "123456", "PortalActivation"))
            .ReturnsAsync(otp);

        _userRepositoryMock
            .Setup(x => x.GetByMobileAsync("9999999999"))
            .ReturnsAsync(user);

        _linkRepositoryMock
            .Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync(link);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(user, link))
            .Returns(("fake-token", DateTime.UtcNow.AddMinutes(60)));

        var result = await service.VerifyOtpAndActivateAsync(request);

        Assert.NotNull(result);
        Assert.Equal(5, result.UserId);
        Assert.Equal(1, result.PatientId);
        Assert.Equal("UHID001", result.UHID);
        Assert.Equal("fake-token", result.AccessToken);
        Assert.True(otp.IsUsed);

        _otpRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _linkRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task PatientLoginAsync_ShouldReturnAuthResponse_WhenOtpIsValid()
    {
        var service = CreateService();

        var request = new PatientLoginRequestDto
        {
            PatientId = 1,
            MobileNumber = "9999999999",
            OtpCode = "123456"
        };

        var patient = GetPatient();

        var link = new PatientUserLink
        {
            PatientId = 1,
            UserId = 5,
            UHID = "UHID001",
            PortalActivated = true
        };

        var otp = new OtpVerification
        {
            PatientId = 1,
            MobileNumber = "9999999999",
            OtpCode = "123456",
            Purpose = "Login",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5)
        };

        var user = new User
        {
            Id = 5,
            MobileNumber = "9999999999",
            Role = AppRoles.Patient
        };

        _patientsApiClientMock
            .Setup(x => x.GetPatientByIdAsync(1))
            .ReturnsAsync(patient);

        _linkRepositoryMock
            .Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync(link);

        _otpRepositoryMock
            .Setup(x => x.GetValidOtpAsync(1, "9999999999", "123456", "Login"))
            .ReturnsAsync(otp);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(5))
            .ReturnsAsync(user);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(user, link))
            .Returns(("login-token", DateTime.UtcNow.AddMinutes(60)));

        var result = await service.PatientLoginAsync(request);

        Assert.Equal(5, result.UserId);
        Assert.Equal("login-token", result.AccessToken);
        Assert.True(otp.IsUsed);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ShouldReturnCurrentUser_WhenUserExists()
    {
        var service = CreateService();

        var user = new User
        {
            Id = 5,
            MobileNumber = "9999999999",
            Role = AppRoles.Patient
        };

        var link = new PatientUserLink
        {
            PatientId = 1,
            UserId = 5,
            UHID = "UHID001"
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(5))
            .ReturnsAsync(user);

        _linkRepositoryMock
            .Setup(x => x.GetByUserIdAsync(5))
            .ReturnsAsync(link);

        var result = await service.GetCurrentUserAsync(5);

        Assert.NotNull(result);
        Assert.Equal(5, result.UserId);
        Assert.Equal(1, result.PatientId);
        Assert.Equal("UHID001", result.UHID);
        Assert.Equal("9999999999", result.MobileNumber);
    }

    private static PatientApiResponse GetPatient()
    {
        return new PatientApiResponse
        {
            Id = 1,
            UHID = "UHID001",
            FullName = "Tushar Sharma",
            MobileNumber = "9999999999",
            Email = "tushar@gmail.com",
            PortalAccessEnabled = true,
            PortalActivated = false
        };
    }
}
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
    private readonly Mock<IRoleRepository> _roleRepositoryMock = new();
    private readonly Mock<IOtpRepository> _otpRepositoryMock = new();
    private readonly Mock<IPatientUserLinkRepository> _linkRepositoryMock = new();
    private readonly Mock<IOtpService> _otpServiceMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();

    private AuthService CreateService()
    {
        return new AuthService(
            _patientsApiClientMock.Object,
            _userRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _otpRepositoryMock.Object,
            _linkRepositoryMock.Object,
            _otpServiceMock.Object,
            _jwtServiceMock.Object
        );
    }

    [Fact]
    public async Task SendLoginOtpAsync_ShouldThrow_WhenPatientIdInvalid()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SendLoginOtpAsync(0, "9999999999"));
    }

    [Fact]
    public async Task SendLoginOtpAsync_ShouldThrow_WhenPatientNotFound()
    {
        var service = CreateService();

        _patientsApiClientMock
            .Setup(x => x.GetPatientByIdAsync(1))
            .ReturnsAsync((PatientApiResponse?)null);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SendLoginOtpAsync(1, "9999999999"));
    }

    [Fact]
    public async Task SendLoginOtpAsync_ShouldThrow_WhenMobileDoesNotMatch()
    {
        var service = CreateService();

        _patientsApiClientMock
            .Setup(x => x.GetPatientByIdAsync(1))
            .ReturnsAsync(GetPatient());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SendLoginOtpAsync(1, "8888888888"));
    }

    [Fact]
    public async Task SendLoginOtpAsync_ShouldCreateUserLinkAndOtp_WhenFirstLogin()
    {
        var service = CreateService();

        var patient = GetPatient();

        var patientRole = new Role
        {
            Id = 1,
            Name = AppRoles.Patient,
            NormalizedName = AppRoles.Patient.ToUpperInvariant()
        };

        var otp = new OtpVerification
        {
            PatientId = 1,
            MobileNumber = "9999999999",
            OtpCode = "123456",
            Purpose = "Login",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5)
        };

        _patientsApiClientMock
            .Setup(x => x.GetPatientByIdAsync(1))
            .ReturnsAsync(patient);

        _linkRepositoryMock
            .Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync((PatientUserLink?)null);

        _roleRepositoryMock
            .Setup(x => x.GetByNameAsync(AppRoles.Patient))
            .ReturnsAsync(patientRole);

        _otpServiceMock
            .Setup(x => x.CreateOtp(1, "9999999999", "Login"))
            .Returns(otp);

        await service.SendLoginOtpAsync(1, "9999999999");

        _userRepositoryMock.Verify(x => x.AddAsync(It.Is<User>(u =>
            u.MobileNumber == "9999999999" &&
            u.LoginId == "9999999999" &&
            u.Email == "tushar@gmail.com" &&
            u.IsActive &&
            u.IsOtpLoginEnabled &&
            !u.IsPasswordLoginEnabled &&
            !u.IsFirstLoginCompleted &&
            u.UserRoles.Any(ur => ur.RoleId == patientRole.Id)
        )), Times.Once);

        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);

        _linkRepositoryMock.Verify(x => x.AddAsync(It.Is<PatientUserLink>(l =>
            l.PatientId == 1 &&
            l.UHID == "UHID001" &&
            l.PortalActivated
        )), Times.Once);

        _linkRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);

        _otpRepositoryMock.Verify(x => x.AddAsync(otp), Times.Once);
        _otpRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SendLoginOtpAsync_ShouldOnlyCreateOtp_WhenLinkAlreadyExists()
    {
        var service = CreateService();

        var otp = new OtpVerification
        {
            PatientId = 1,
            MobileNumber = "9999999999",
            OtpCode = "123456",
            Purpose = "Login",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5)
        };

        var link = new PatientUserLink
        {
            Id = 1,
            PatientId = 1,
            UserId = 5,
            UHID = "UHID001",
            PortalActivated = true
        };

        _patientsApiClientMock
            .Setup(x => x.GetPatientByIdAsync(1))
            .ReturnsAsync(GetPatient());

        _linkRepositoryMock
            .Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync(link);

        _otpServiceMock
            .Setup(x => x.CreateOtp(1, "9999999999", "Login"))
            .Returns(otp);

        await service.SendLoginOtpAsync(1, "9999999999");

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        _linkRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PatientUserLink>()), Times.Never);

        _otpRepositoryMock.Verify(x => x.AddAsync(otp), Times.Once);
        _otpRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task PatientLoginAsync_ShouldReturnAuthResponse_WhenOtpIsValid()
    {
        var service = CreateService();

        var request = new LoginRequestDto
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
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false
        };

        var user = GetPatientUser();

        _patientsApiClientMock
            .Setup(x => x.GetPatientByIdAsync(1))
            .ReturnsAsync(patient);

        _linkRepositoryMock
            .Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync(link);

        _userRepositoryMock
            .Setup(x => x.GetByIdWithRolesAsync(5))
            .ReturnsAsync(user);

        _otpRepositoryMock
            .Setup(x => x.GetValidOtpAsync(1, "9999999999", "123456", "Login"))
            .ReturnsAsync(otp);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(
                user,
                link,
                It.Is<string[]>(roles => roles.Contains(AppRoles.Patient))
            ))
            .Returns(("login-token", DateTime.UtcNow.AddMinutes(60)));

        var result = await service.PatientLoginAsync(request);

        Assert.Equal(5, result.UserId);
        Assert.Equal(1, result.PatientId);
        Assert.Equal("UHID001", result.UHID);
        Assert.Equal("login-token", result.AccessToken);
        Assert.True(otp.IsUsed);

        _otpRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task PatientLoginAsync_ShouldThrow_WhenPatientNotFound()
    {
        var service = CreateService();

        var request = new LoginRequestDto
        {
            PatientId = 1,
            MobileNumber = "9999999999",
            OtpCode = "123456"
        };

        _patientsApiClientMock
            .Setup(x => x.GetPatientByIdAsync(1))
            .ReturnsAsync((PatientApiResponse?)null);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.PatientLoginAsync(request));
    }

    [Fact]
    public async Task PatientLoginAsync_ShouldThrow_WhenMobileDoesNotMatch()
    {
        var service = CreateService();

        var request = new LoginRequestDto
        {
            PatientId = 1,
            MobileNumber = "8888888888",
            OtpCode = "123456"
        };

        _patientsApiClientMock
            .Setup(x => x.GetPatientByIdAsync(1))
            .ReturnsAsync(GetPatient());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.PatientLoginAsync(request));
    }

    [Fact]
    public async Task PatientLoginAsync_ShouldThrow_WhenPortalUserWasNotCreated()
    {
        var service = CreateService();

        var request = new LoginRequestDto
        {
            PatientId = 1,
            MobileNumber = "9999999999",
            OtpCode = "123456"
        };

        _patientsApiClientMock
            .Setup(x => x.GetPatientByIdAsync(1))
            .ReturnsAsync(GetPatient());

        _linkRepositoryMock
            .Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync((PatientUserLink?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.PatientLoginAsync(request));
    }

    [Fact]
    public async Task GetCurrentUserAsync_ShouldReturnCurrentUser_WhenUserExists()
    {
        var service = CreateService();

        var user = GetPatientUser();

        var link = new PatientUserLink
        {
            PatientId = 1,
            UserId = 5,
            UHID = "UHID001"
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdWithRolesAsync(5))
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
        Assert.Contains(AppRoles.Patient, result.Roles);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ShouldReturnNull_WhenUserIdInvalid()
    {
        var service = CreateService();

        var result = await service.GetCurrentUserAsync(0);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateProfilePhotoAsync_ShouldUpdatePhoto_WhenUserExists()
    {
        var service = CreateService();

        var user = GetPatientUser();

        _userRepositoryMock
            .Setup(x => x.GetByIdWithRolesAsync(5))
            .ReturnsAsync(user);

        _linkRepositoryMock
            .Setup(x => x.GetByUserIdAsync(5))
            .ReturnsAsync(new PatientUserLink
            {
                PatientId = 1,
                UserId = 5,
                UHID = "UHID001"
            });

        var result = await service.UpdateProfilePhotoAsync(5, "/uploads/profile.jpg");

        Assert.NotNull(result);
        Assert.Equal("/uploads/profile.jpg", user.PhotoUrl);

        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
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
            IsProfileCompleted = true
        };
    }

    private static User GetPatientUser()
    {
        var role = new Role
        {
            Id = 1,
            Name = AppRoles.Patient,
            NormalizedName = AppRoles.Patient.ToUpperInvariant()
        };

        var user = new User
        {
            Id = 5,
            MobileNumber = "9999999999",
            LoginId = "9999999999",
            Email = "tushar@gmail.com",
            IsActive = true,
            IsOtpLoginEnabled = true,
            IsPasswordLoginEnabled = false,
            IsFirstLoginCompleted = false
        };

        user.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            User = user,
            RoleId = role.Id,
            Role = role
        });

        return user;
    }
}
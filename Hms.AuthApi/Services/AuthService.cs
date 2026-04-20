using Hms.AuthApi.DTOs.Auth;
using Hms.AuthApi.Entities;
using Hms.AuthApi.Interfaces.Clients;
using Hms.AuthApi.Interfaces.Repository;
using Hms.AuthApi.Interfaces.Services;
using Hms.AuthApi.Common;
namespace Hms.AuthApi.Services;

public class AuthService : IAuthService
{
    private readonly IPatientsApiClient _patientsApiClient;
    private readonly IUserRepository _userRepository;
    private readonly IOtpRepository _otpRepository;
    private readonly IPatientUserLinkRepository _patientUserLinkRepository;
    private readonly IOtpService _otpService;
    private readonly IJwtService _jwtService;

    public AuthService(
        IPatientsApiClient patientsApiClient,
        IUserRepository userRepository,
        IOtpRepository otpRepository,
        IPatientUserLinkRepository patientUserLinkRepository,
        IOtpService otpService,
        IJwtService jwtService)
    {
        _patientsApiClient = patientsApiClient;
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _patientUserLinkRepository = patientUserLinkRepository;
        _otpService = otpService;
        _jwtService = jwtService;
    }

    public async Task SendPortalActivationAsync(SendPatientPortalActivationRequestDto request)
    {
        if (request.PatientId <= 0)
            throw new ArgumentException("Invalid patient id.");

        var patient = await _patientsApiClient.GetPatientByIdAsync(request.PatientId);
        if (patient == null)
            throw new ArgumentException("Patient not found.");

        if (!patient.PortalAccessEnabled)
            throw new InvalidOperationException("Portal access is not enabled for this patient.");

        var otp = _otpService.CreateOtp(patient.Id, patient.MobileNumber, "PortalActivation");

        await _otpRepository.AddAsync(otp);
        await _otpRepository.SaveChangesAsync();

        Console.WriteLine($"[OTP SENT] PatientId={patient.Id}, Mobile={patient.MobileNumber}, OTP={otp.OtpCode}");
    }

    public async Task<AuthResponseDto> VerifyOtpAndActivateAsync(VerifyOtpRequestDto request)
    {
        ValidateVerifyOtpRequest(request);

        var patient = await _patientsApiClient.GetPatientByIdAsync(request.PatientId);
        if (patient == null)
            throw new ArgumentException("Patient not found.");

        var otp = await _otpRepository.GetValidOtpAsync(
            request.PatientId,
            request.MobileNumber.Trim(),
            request.OtpCode.Trim(),
            request.Purpose.Trim());

        if (otp == null)
            throw new InvalidOperationException("Invalid or expired OTP.");

        otp.IsUsed = true;
        otp.UpdatedAtUtc = DateTime.UtcNow;
        await _otpRepository.SaveChangesAsync();

        var user = await _userRepository.GetByMobileAsync(request.MobileNumber.Trim());
        if (user == null)
        {
            user = new User
            {
                MobileNumber = request.MobileNumber.Trim(),
                Email = patient.Email,
                Role = AppRoles.Patient,
                IsActive = true
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
        }

        var link = await _patientUserLinkRepository.GetByPatientIdAsync(request.PatientId);
        if (link == null)
        {
            link = new PatientUserLink
            {
                PatientId = patient.Id,
                UHID = patient.UHID,
                UserId = user.Id,
                PortalActivated = true,
                ActivatedAtUtc = DateTime.UtcNow
            };

            await _patientUserLinkRepository.AddAsync(link);
            await _patientUserLinkRepository.SaveChangesAsync();
        }
        else
        {
            link.PortalActivated = true;
            link.ActivatedAtUtc = DateTime.UtcNow;
            await _patientUserLinkRepository.SaveChangesAsync();
        }

        var tokenResult = _jwtService.GenerateToken(user, link);

        return new AuthResponseDto
        {
            UserId = user.Id,
            PatientId = patient.Id,
            UHID = patient.UHID,
            MobileNumber = user.MobileNumber,
            Role = user.Role,
            AccessToken = tokenResult.Token,
            ExpiresAtUtc = tokenResult.ExpiresAtUtc
        };
    }

    public async Task<AuthResponseDto> PatientLoginAsync(PatientLoginRequestDto request)
    {
        if (request.PatientId <= 0)
            throw new ArgumentException("Invalid patient id.");

        if (string.IsNullOrWhiteSpace(request.MobileNumber))
            throw new ArgumentException("Mobile number is required.");

        if (string.IsNullOrWhiteSpace(request.OtpCode))
            throw new ArgumentException("OTP is required.");

        var patient = await _patientsApiClient.GetPatientByIdAsync(request.PatientId);
        if (patient == null)
            throw new ArgumentException("Patient not found.");

        var link = await _patientUserLinkRepository.GetByPatientIdAsync(request.PatientId);
        if (link == null || !link.PortalActivated)
            throw new InvalidOperationException("Patient portal is not activated.");

        var otp = await _otpRepository.GetValidOtpAsync(
            request.PatientId,
            request.MobileNumber.Trim(),
            request.OtpCode.Trim(),
            "Login");

        if (otp == null)
            throw new InvalidOperationException("Invalid or expired OTP.");

        otp.IsUsed = true;
        otp.UpdatedAtUtc = DateTime.UtcNow;
        await _otpRepository.SaveChangesAsync();

        var user = await _userRepository.GetByIdAsync(link.UserId);
        if (user == null)
            throw new InvalidOperationException("Linked user not found.");

        var tokenResult = _jwtService.GenerateToken(user, link);

        return new AuthResponseDto
        {
            UserId = user.Id,
            PatientId = patient.Id,
            UHID = patient.UHID,
            MobileNumber = user.MobileNumber,
            Role = user.Role,
            AccessToken = tokenResult.Token,
            ExpiresAtUtc = tokenResult.ExpiresAtUtc
        };
    }

    public async Task<CurrentUserResponseDto?> GetCurrentUserAsync(int userId)
    {
        if (userId <= 0)
            return null;

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return null;

        var link = await _patientUserLinkRepository.GetByUserIdAsync(userId);

        return new CurrentUserResponseDto
        {
            UserId = user.Id,
            PatientId = link?.PatientId,
            UHID = link?.UHID,
            MobileNumber = user.MobileNumber,
            Role = user.Role
        };
    }
    public async Task SendLoginOtpAsync(int patientId)
    {
        if (patientId <= 0)
            throw new ArgumentException("Invalid patient id.");

        var patient = await _patientsApiClient.GetPatientByIdAsync(patientId);
        if (patient == null)
            throw new ArgumentException("Patient not found.");

        var link = await _patientUserLinkRepository.GetByPatientIdAsync(patientId);
        if (link == null || !link.PortalActivated)
            throw new InvalidOperationException("Patient portal is not activated.");

        var otp = _otpService.CreateOtp(patient.Id, patient.MobileNumber, "Login");

        await _otpRepository.AddAsync(otp);
        await _otpRepository.SaveChangesAsync();

        Console.WriteLine($"[LOGIN OTP SENT] PatientId={patient.Id}, Mobile={patient.MobileNumber}, OTP={otp.OtpCode}");
    }
    private static void ValidateVerifyOtpRequest(VerifyOtpRequestDto request)
    {
        if (request.PatientId <= 0)
            throw new ArgumentException("Invalid patient id.");

        if (string.IsNullOrWhiteSpace(request.MobileNumber))
            throw new ArgumentException("Mobile number is required.");

        if (string.IsNullOrWhiteSpace(request.OtpCode))
            throw new ArgumentException("OTP is required.");

        if (string.IsNullOrWhiteSpace(request.Purpose))
            throw new ArgumentException("Purpose is required.");
    }
}
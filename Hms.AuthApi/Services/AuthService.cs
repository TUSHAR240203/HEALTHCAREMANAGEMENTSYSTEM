using Hms.AuthApi.Common;
using Hms.AuthApi.DTOs.Auth;
using Hms.AuthApi.Entities;
using Hms.AuthApi.Interfaces.Clients;
using Hms.AuthApi.Interfaces.Repository;
using Hms.AuthApi.Interfaces.Services;

namespace Hms.AuthApi.Services;

public class AuthService : IAuthService
{
    private readonly IPatientsApiClient _patientsApiClient;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IOtpRepository _otpRepository;
    private readonly IPatientUserLinkRepository _patientUserLinkRepository;
    private readonly IOtpService _otpService;
    private readonly IJwtService _jwtService;

    public AuthService(
        IPatientsApiClient patientsApiClient,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IOtpRepository otpRepository,
        IPatientUserLinkRepository patientUserLinkRepository,
        IOtpService otpService,
        IJwtService jwtService)
    {
        _patientsApiClient = patientsApiClient;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _otpRepository = otpRepository;
        _patientUserLinkRepository = patientUserLinkRepository;
        _otpService = otpService;
        _jwtService = jwtService;
    }

    public async Task SendPortalActivationAsync(
        SendPatientPortalActivationRequestDto request)
    {
        var patient = await _patientsApiClient.GetPatientByIdAsync(request.PatientId);

        if (patient == null)
            throw new ArgumentException("Patient not found.");

        var normalizedMobile = NormalizeMobile(request.MobileNumber);

        if (NormalizeMobile(patient.MobileNumber) != normalizedMobile)
            throw new ArgumentException("Mobile number does not match patient record.");

        if (!patient.PortalAccessEnabled)
            throw new InvalidOperationException("Portal access is not enabled for this patient.");

        var otp = _otpService.CreateOtp(
            patient.Id,
            normalizedMobile,
            "PortalActivation");

        await _otpRepository.AddAsync(otp);
        await _otpRepository.SaveChangesAsync();

        Console.WriteLine($"[PORTAL ACTIVATION OTP GENERATED] PatientId={patient.Id}");
        Console.WriteLine($"Mobile={normalizedMobile}");
        Console.WriteLine($"OTP={otp.OtpCode}");
        Console.WriteLine("Purpose=PortalActivation");
        Console.WriteLine("[OTP SAVED] Portal activation OTP saved successfully.");
    }

    public async Task<AuthResponseDto> VerifyOtpAndActivateAsync(
        VerifyOtpRequestDto request)
    {
        var patient = await _patientsApiClient.GetPatientByIdAsync(request.PatientId);

        if (patient == null)
            throw new ArgumentException("Patient not found.");

        var normalizedMobile = NormalizeMobile(request.MobileNumber);

        if (NormalizeMobile(patient.MobileNumber) != normalizedMobile)
            throw new ArgumentException("Mobile number does not match patient record.");

        var otp = await _otpRepository.GetValidOtpAsync(
            request.PatientId,
            normalizedMobile,
            request.OtpCode.Trim(),
            request.Purpose.Trim());

        if (otp == null)
            throw new InvalidOperationException("Invalid or expired OTP.");

        otp.IsUsed = true;
        otp.UpdatedAtUtc = DateTime.UtcNow;
        await _otpRepository.SaveChangesAsync();

        var link = await _patientUserLinkRepository.GetByPatientIdAsync(request.PatientId);
        User? user = null;

        if (link != null)
        {
            user = await _userRepository.GetByIdWithRolesAsync(link.UserId);
        }

        if (user == null)
        {
            user = new User
            {
                MobileNumber = normalizedMobile,
                Email = patient.Email,
                IsActive = true
            };

            var patientRole = await _roleRepository.GetByNameAsync(AppRoles.Patient);

            if (patientRole == null)
                throw new InvalidOperationException("Patient role not found.");

            user.UserRoles.Add(new UserRole
            {
                RoleId = patientRole.Id
            });

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            user = await _userRepository.GetByIdWithRolesAsync(user.Id)
                ?? throw new InvalidOperationException("User could not be loaded after creation.");
        }

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
            link.UserId = user.Id;
            link.UHID = patient.UHID;
            link.PortalActivated = true;
            link.ActivatedAtUtc = DateTime.UtcNow;

            await _patientUserLinkRepository.SaveChangesAsync();
        }

        var roles = user.UserRoles
            .Select(x => x.Role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var tokenResult = _jwtService.GenerateToken(user, link, roles);

        return new AuthResponseDto
        {
            UserId = user.Id,
            PatientId = patient.Id,
            UHID = patient.UHID,
            FullName = patient.FullName,
            MobileNumber = user.MobileNumber,
            Roles = roles,
            AccessToken = tokenResult.Token,
            ExpiresAtUtc = tokenResult.ExpiresAtUtc,
            IsProfileCompleted = patient.IsProfileCompleted
        };
    }

    public async Task<AuthResponseDto> PatientLoginAsync(
        LoginRequestDto request)
    {
        var patient = await _patientsApiClient.GetPatientByIdAsync(request.PatientId);

        if (patient == null)
            throw new ArgumentException("Patient not found.");

        var normalizedMobile = NormalizeMobile(request.MobileNumber);

        if (NormalizeMobile(patient.MobileNumber) != normalizedMobile)
            throw new ArgumentException("Mobile number does not match patient record.");

        var link = await _patientUserLinkRepository.GetByPatientIdAsync(request.PatientId);

        if (link == null || !link.PortalActivated)
            throw new InvalidOperationException("Patient portal is not activated.");

        var otp = await _otpRepository.GetValidOtpAsync(
            request.PatientId,
            normalizedMobile,
            request.OtpCode.Trim(),
            "Login");

        if (otp == null)
            throw new InvalidOperationException("Invalid or expired OTP.");

        otp.IsUsed = true;
        otp.UpdatedAtUtc = DateTime.UtcNow;

        await _otpRepository.SaveChangesAsync();

        var user = await _userRepository.GetByIdWithRolesAsync(link.UserId);

        if (user == null)
            throw new InvalidOperationException("Linked user not found.");

        var roles = user.UserRoles
            .Select(x => x.Role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var tokenResult = _jwtService.GenerateToken(user, link, roles);

        return new AuthResponseDto
        {
            UserId = user.Id,
            PatientId = patient.Id,
            UHID = patient.UHID,
            FullName = patient.FullName,
            MobileNumber = user.MobileNumber,
            Roles = roles,
            AccessToken = tokenResult.Token,
            ExpiresAtUtc = tokenResult.ExpiresAtUtc,
            IsProfileCompleted = patient.IsProfileCompleted

        };
    }

    public async Task<CurrentUserResponseDto?> GetCurrentUserAsync(int userId)
    {
        if (userId <= 0)
            return null;

        var user = await _userRepository.GetByIdWithRolesAsync(userId);

        if (user == null)
            return null;

        var link = await _patientUserLinkRepository.GetByUserIdAsync(userId);

        return new CurrentUserResponseDto
        {
            UserId = user.Id,
            PatientId = link?.PatientId,
            UHID = link?.UHID,
            MobileNumber = user.MobileNumber,
            Roles = user.UserRoles
                .Select(x => x.Role.Name)
                .Distinct()
                .ToArray()
        };
    }

    public async Task SendLoginOtpAsync(
        int patientId,
        string mobileNumber)
    {
        if (patientId <= 0)
            throw new ArgumentException("Invalid patient id.");

        var patient = await _patientsApiClient.GetPatientByIdAsync(patientId);

        if (patient == null)
            throw new ArgumentException("Patient not found.");

        var normalizedMobile = NormalizeMobile(mobileNumber);

        if (NormalizeMobile(patient.MobileNumber) != normalizedMobile)
            throw new ArgumentException("Mobile number does not match patient record.");

        var link = await _patientUserLinkRepository.GetByPatientIdAsync(patientId);

        if (link == null || !link.PortalActivated)
            throw new InvalidOperationException("Patient portal is not activated.");

        var otp = _otpService.CreateOtp(
            patient.Id,
            normalizedMobile,
            "Login");

        await _otpRepository.AddAsync(otp);
        await _otpRepository.SaveChangesAsync();

        Console.WriteLine($"[LOGIN OTP GENERATED] PatientId={patient.Id}");
        Console.WriteLine($"Mobile={normalizedMobile}");
        Console.WriteLine($"OTP={otp.OtpCode}");
        Console.WriteLine("Purpose=Login");
        Console.WriteLine("[OTP SAVED] Login OTP saved successfully.");
    }

    private static string NormalizeMobile(string mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile))
            throw new ArgumentException("Mobile number is required.");

        var value = mobile.Trim()
            .Replace(" ", "")
            .Replace("-", "");

        if (value.StartsWith("+91"))
            value = value[3..];
        else if (value.StartsWith("91") && value.Length == 12)
            value = value[2..];

        return value;
    }
}
<<<<<<< HEAD
=======
using Hms.AuthApi.Common;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
using Hms.AuthApi.DTOs.Auth;
using Hms.AuthApi.Entities;
using Hms.AuthApi.Interfaces.Clients;
using Hms.AuthApi.Interfaces.Repository;
using Hms.AuthApi.Interfaces.Services;
<<<<<<< HEAD
using Hms.AuthApi.Common;
=======

>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
namespace Hms.AuthApi.Services;

public class AuthService : IAuthService
{
    private readonly IPatientsApiClient _patientsApiClient;
    private readonly IUserRepository _userRepository;
<<<<<<< HEAD
=======
    private readonly IRoleRepository _roleRepository;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    private readonly IOtpRepository _otpRepository;
    private readonly IPatientUserLinkRepository _patientUserLinkRepository;
    private readonly IOtpService _otpService;
    private readonly IJwtService _jwtService;

    public AuthService(
        IPatientsApiClient patientsApiClient,
        IUserRepository userRepository,
<<<<<<< HEAD
=======
        IRoleRepository roleRepository,
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
        IOtpRepository otpRepository,
        IPatientUserLinkRepository patientUserLinkRepository,
        IOtpService otpService,
        IJwtService jwtService)
    {
        _patientsApiClient = patientsApiClient;
        _userRepository = userRepository;
<<<<<<< HEAD
=======
        _roleRepository = roleRepository;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
        _otpRepository = otpRepository;
        _patientUserLinkRepository = patientUserLinkRepository;
        _otpService = otpService;
        _jwtService = jwtService;
    }

<<<<<<< HEAD
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
=======
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
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85

        await _otpRepository.AddAsync(otp);
        await _otpRepository.SaveChangesAsync();

<<<<<<< HEAD
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
=======
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
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
            request.OtpCode.Trim(),
            request.Purpose.Trim());

        if (otp == null)
            throw new InvalidOperationException("Invalid or expired OTP.");

        otp.IsUsed = true;
        otp.UpdatedAtUtc = DateTime.UtcNow;
        await _otpRepository.SaveChangesAsync();

<<<<<<< HEAD
        var user = await _userRepository.GetByMobileAsync(request.MobileNumber.Trim());
=======
        var link = await _patientUserLinkRepository.GetByPatientIdAsync(request.PatientId);
        User? user = null;

        if (link != null)
        {
            user = await _userRepository.GetByIdWithRolesAsync(link.UserId);
        }

>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
        if (user == null)
        {
            user = new User
            {
<<<<<<< HEAD
                MobileNumber = request.MobileNumber.Trim(),
                Email = patient.Email,
                Role = AppRoles.Patient,
                IsActive = true
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
        }

        var link = await _patientUserLinkRepository.GetByPatientIdAsync(request.PatientId);
=======
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

>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
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
<<<<<<< HEAD
            link.PortalActivated = true;
            link.ActivatedAtUtc = DateTime.UtcNow;
            await _patientUserLinkRepository.SaveChangesAsync();
        }

        var tokenResult = _jwtService.GenerateToken(user, link);
=======
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
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85

        return new AuthResponseDto
        {
            UserId = user.Id,
            PatientId = patient.Id,
            UHID = patient.UHID,
<<<<<<< HEAD
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
=======
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

>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
        if (link == null || !link.PortalActivated)
            throw new InvalidOperationException("Patient portal is not activated.");

        var otp = await _otpRepository.GetValidOtpAsync(
            request.PatientId,
<<<<<<< HEAD
            request.MobileNumber.Trim(),
=======
            normalizedMobile,
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
            request.OtpCode.Trim(),
            "Login");

        if (otp == null)
            throw new InvalidOperationException("Invalid or expired OTP.");

        otp.IsUsed = true;
        otp.UpdatedAtUtc = DateTime.UtcNow;
<<<<<<< HEAD
        await _otpRepository.SaveChangesAsync();

        var user = await _userRepository.GetByIdAsync(link.UserId);
        if (user == null)
            throw new InvalidOperationException("Linked user not found.");

        var tokenResult = _jwtService.GenerateToken(user, link);
=======

        await _otpRepository.SaveChangesAsync();

        var user = await _userRepository.GetByIdWithRolesAsync(link.UserId);

        if (user == null)
            throw new InvalidOperationException("Linked user not found.");

        var roles = user.UserRoles
            .Select(x => x.Role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var tokenResult = _jwtService.GenerateToken(user, link, roles);
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85

        return new AuthResponseDto
        {
            UserId = user.Id,
            PatientId = patient.Id,
            UHID = patient.UHID,
<<<<<<< HEAD
            MobileNumber = user.MobileNumber,
            Role = user.Role,
            AccessToken = tokenResult.Token,
            ExpiresAtUtc = tokenResult.ExpiresAtUtc
=======
            FullName = patient.FullName,
            MobileNumber = user.MobileNumber,
            Roles = roles,
            AccessToken = tokenResult.Token,
            ExpiresAtUtc = tokenResult.ExpiresAtUtc,
            IsProfileCompleted = patient.IsProfileCompleted

>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
        };
    }

    public async Task<CurrentUserResponseDto?> GetCurrentUserAsync(int userId)
    {
        if (userId <= 0)
            return null;

<<<<<<< HEAD
        var user = await _userRepository.GetByIdAsync(userId);
=======
        var user = await _userRepository.GetByIdWithRolesAsync(userId);

>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
        if (user == null)
            return null;

        var link = await _patientUserLinkRepository.GetByUserIdAsync(userId);

        return new CurrentUserResponseDto
        {
            UserId = user.Id,
            PatientId = link?.PatientId,
            UHID = link?.UHID,
            MobileNumber = user.MobileNumber,
<<<<<<< HEAD
            Role = user.Role
        };
    }
    public async Task SendLoginOtpAsync(int patientId)
=======
            Roles = user.UserRoles
                .Select(x => x.Role.Name)
                .Distinct()
                .ToArray()
        };
    }

    public async Task SendLoginOtpAsync(
        int patientId,
        string mobileNumber)
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    {
        if (patientId <= 0)
            throw new ArgumentException("Invalid patient id.");

        var patient = await _patientsApiClient.GetPatientByIdAsync(patientId);
<<<<<<< HEAD
        if (patient == null)
            throw new ArgumentException("Patient not found.");

        var link = await _patientUserLinkRepository.GetByPatientIdAsync(patientId);
        if (link == null || !link.PortalActivated)
            throw new InvalidOperationException("Patient portal is not activated.");

        var otp = _otpService.CreateOtp(patient.Id, patient.MobileNumber, "Login");
=======

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
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85

        await _otpRepository.AddAsync(otp);
        await _otpRepository.SaveChangesAsync();

<<<<<<< HEAD
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
=======
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
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    }
}
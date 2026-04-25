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

    public AuthService(IPatientsApiClient patientsApiClient, IUserRepository userRepository, IRoleRepository roleRepository,
        IOtpRepository otpRepository, IPatientUserLinkRepository patientUserLinkRepository, IOtpService otpService, IJwtService jwtService)
    {
        _patientsApiClient = patientsApiClient;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _otpRepository = otpRepository;
        _patientUserLinkRepository = patientUserLinkRepository;
        _otpService = otpService;
        _jwtService = jwtService;
    }

    public async Task SendLoginOtpAsync(int patientId, string mobileNumber)
    {
        if (patientId <= 0) throw new ArgumentException("Invalid patient id.");
        var patient = await _patientsApiClient.GetPatientByIdAsync(patientId) ?? throw new ArgumentException("Patient not found.");
        var normalizedMobile = NormalizeMobile(mobileNumber);
        if (NormalizeMobile(patient.MobileNumber) != normalizedMobile) throw new ArgumentException("Mobile number does not match patient record.");

        var link = await _patientUserLinkRepository.GetByPatientIdAsync(patientId);
        if (link == null)
        {
            var patientRole = await _roleRepository.GetByNameAsync(AppRoles.Patient) ?? throw new InvalidOperationException("Patient role not found.");
            var user = new User
            {
                MobileNumber = normalizedMobile,
                LoginId = normalizedMobile,
                Email = patient.Email,
                IsActive = true,
                IsOtpLoginEnabled = true,
                IsPasswordLoginEnabled = false,
                IsFirstLoginCompleted = false
            };
            user.UserRoles.Add(new UserRole { RoleId = patientRole.Id });
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
            link = new PatientUserLink { PatientId = patient.Id, UHID = patient.UHID, UserId = user.Id, PortalActivated = true, ActivatedAtUtc = DateTime.UtcNow };
            await _patientUserLinkRepository.AddAsync(link);
            await _patientUserLinkRepository.SaveChangesAsync();
        }

        var otp = _otpService.CreateOtp(patient.Id, normalizedMobile, "Login");
        await _otpRepository.AddAsync(otp);
        await _otpRepository.SaveChangesAsync();
        Console.WriteLine($"[PATIENT LOGIN OTP] PatientId={patient.Id}, Mobile={normalizedMobile}, OTP={otp.OtpCode}");
    }

    public async Task<AuthResponseDto> PatientLoginAsync(LoginRequestDto request)
    {
        var patient = await _patientsApiClient.GetPatientByIdAsync(request.PatientId) ?? throw new ArgumentException("Patient not found.");
        var normalizedMobile = NormalizeMobile(request.MobileNumber);
        if (NormalizeMobile(patient.MobileNumber) != normalizedMobile) throw new ArgumentException("Mobile number does not match patient record.");
        var link = await _patientUserLinkRepository.GetByPatientIdAsync(request.PatientId) ?? throw new InvalidOperationException("Patient portal user was not created. Send OTP first.");
        var user = await _userRepository.GetByIdWithRolesAsync(link.UserId) ?? throw new InvalidOperationException("Linked user not found.");
        if (!user.IsActive) throw new InvalidOperationException("This portal user is inactive.");
        if (!user.IsOtpLoginEnabled && user.IsFirstLoginCompleted) throw new InvalidOperationException("OTP login is disabled for this user.");

        var otp = await _otpRepository.GetValidOtpAsync(request.PatientId, normalizedMobile, request.OtpCode.Trim(), "Login") ?? throw new InvalidOperationException("Invalid or expired OTP.");
        otp.IsUsed = true;
        otp.UpdatedAtUtc = DateTime.UtcNow;
        if (!link.PortalActivated) { link.PortalActivated = true; link.ActivatedAtUtc = DateTime.UtcNow; }
        await _otpRepository.SaveChangesAsync();
        await _patientUserLinkRepository.SaveChangesAsync();
        return BuildAuthResponse(user, link, patient.FullName, patient.IsProfileCompleted);
    }

    public async Task SendStaffLoginOtpAsync(StaffOtpRequestDto request)
    {
        var identifier = GetStaffIdentifier(request.LoginId, request.MobileNumber);
        var (user, mobile) = await FindStaffUserForOtpAsync(identifier);
        if (!user.IsActive) throw new InvalidOperationException("This portal user is inactive.");
        if (!user.IsOtpLoginEnabled && user.IsFirstLoginCompleted) throw new InvalidOperationException("OTP login is disabled for this user.");
        var roles = user.UserRoles.Select(x => x.Role.Name).ToArray();
        if (roles.Contains(AppRoles.Patient, StringComparer.OrdinalIgnoreCase)) throw new InvalidOperationException("Use patient OTP login for patient accounts.");
        var otp = _otpService.CreateOtp(0, mobile, "StaffLogin");
        await _otpRepository.AddAsync(otp);
        await _otpRepository.SaveChangesAsync();
        Console.WriteLine($"[STAFF LOGIN OTP] UserId={user.Id}, Identifier={identifier}, Mobile={mobile}, OTP={otp.OtpCode}");
    }

    public async Task<AuthResponseDto> StaffOtpLoginAsync(StaffOtpLoginRequestDto request)
    {
        var identifier = GetStaffIdentifier(request.LoginId, request.MobileNumber);
        var (user, mobile) = await FindStaffUserForOtpAsync(identifier);
        if (!user.IsActive) throw new InvalidOperationException("This portal user is inactive.");
        if (!user.IsOtpLoginEnabled && user.IsFirstLoginCompleted) throw new InvalidOperationException("OTP login is disabled for this user.");
        var otp = await _otpRepository.GetValidOtpAsync(0, mobile, request.OtpCode.Trim(), "StaffLogin") ?? throw new InvalidOperationException("Invalid or expired OTP.");
        otp.IsUsed = true;
        otp.UpdatedAtUtc = DateTime.UtcNow;
        await _otpRepository.SaveChangesAsync();
        return BuildAuthResponse(user, null, null, true);
    }

    public async Task<AuthResponseDto> StaffLoginAsync(StaffLoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.LoginId) || string.IsNullOrWhiteSpace(request.Password)) throw new ArgumentException("Login id and password are required.");
        var user = await _userRepository.GetByLoginIdWithRolesAsync(request.LoginId.Trim()) ?? throw new InvalidOperationException("Invalid login id or password.");
        if (!user.IsActive) throw new InvalidOperationException("This portal user is inactive.");
        if (!user.IsFirstLoginCompleted) throw new InvalidOperationException("First login must be completed with OTP. Then choose your authentication preference.");
        if (!user.IsPasswordLoginEnabled) throw new InvalidOperationException("Password/JWT login is not enabled for this user.");
        if (string.IsNullOrWhiteSpace(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) throw new InvalidOperationException("Invalid login id or password.");
        var link = await _patientUserLinkRepository.GetByUserIdAsync(user.Id);
        return BuildAuthResponse(user, link, null, true);
    }

    public async Task<AuthResponseDto> UpdateAuthPreferenceAsync(int userId, AuthPreferenceRequestDto request)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(userId) ?? throw new InvalidOperationException("User not found.");
        if (!request.EnablePasswordLogin && !request.EnableOtpLogin) throw new ArgumentException("Enable at least one login method.");
        user.IsOtpLoginEnabled = request.EnableOtpLogin;
        user.IsPasswordLoginEnabled = request.EnablePasswordLogin;
        if (request.EnablePasswordLogin)
        {
            if (string.IsNullOrWhiteSpace(request.LoginId)) throw new ArgumentException("Login id is required when password login is enabled.");
            if (!string.Equals(user.LoginId, request.LoginId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _userRepository.GetByLoginIdWithRolesAsync(request.LoginId.Trim());
                if (existing != null && existing.Id != user.Id) throw new InvalidOperationException("Login id already exists.");
            }
            user.LoginId = request.LoginId.Trim();
            if (!string.IsNullOrWhiteSpace(request.Password)) user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            if (string.IsNullOrWhiteSpace(user.PasswordHash)) throw new ArgumentException("Password is required the first time password login is enabled.");
        }
        user.IsFirstLoginCompleted = true;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await _userRepository.SaveChangesAsync();
        var link = await _patientUserLinkRepository.GetByUserIdAsync(user.Id);
        return BuildAuthResponse(user, link, null, true);
    }

    public async Task<CurrentUserResponseDto?> GetCurrentUserAsync(int userId)
    {
        if (userId <= 0) return null;
        var user = await _userRepository.GetByIdWithRolesAsync(userId);
        if (user == null) return null;
        var link = await _patientUserLinkRepository.GetByUserIdAsync(userId);
        return new CurrentUserResponseDto { UserId = user.Id, PatientId = link?.PatientId, UHID = link?.UHID, MobileNumber = user.MobileNumber, Roles = user.UserRoles.Select(x => x.Role.Name).Distinct().ToArray() };
    }

    public async Task<UserAdminResponseDto> CreateStaffUserAsync(CreateStaffUserRequestDto request)
    {
        var roleName = request.Role.Trim();
        if (!new[] { AppRoles.Admin, AppRoles.Doctor, AppRoles.Receptionist }.Contains(roleName, StringComparer.OrdinalIgnoreCase)) throw new ArgumentException("Only Admin, Doctor, or Receptionist users can be created here.");
        var mobile = NormalizeMobile(string.IsNullOrWhiteSpace(request.MobileNumber) ? request.LoginId : request.MobileNumber!);
        var existingMobile = await _userRepository.GetByMobileWithRolesAsync(mobile);
        if (existingMobile != null && !existingMobile.UserRoles.All(r => r.Role.Name.Equals(AppRoles.Patient, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Mobile number already exists for another staff user.");
        if (!string.IsNullOrWhiteSpace(request.LoginId))
        {
            var existing = await _userRepository.GetByLoginIdWithRolesAsync(request.LoginId.Trim());
            if (existing != null) throw new InvalidOperationException("Login id already exists.");
        }
        if (!request.EnablePasswordLogin && !request.EnableOtpLogin) request.EnableOtpLogin = true;
        var role = await _roleRepository.GetByNameAsync(roleName) ?? throw new InvalidOperationException("Role not found.");
        var user = new User { LoginId = string.IsNullOrWhiteSpace(request.LoginId) ? mobile : request.LoginId.Trim(), MobileNumber = mobile, Email = request.Email, IsActive = request.IsActive, IsOtpLoginEnabled = request.EnableOtpLogin, IsPasswordLoginEnabled = request.EnablePasswordLogin, IsFirstLoginCompleted = false };
        if (!string.IsNullOrWhiteSpace(request.Password)) user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.UserRoles.Add(new UserRole { RoleId = role.Id });
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();
        user = await _userRepository.GetByIdWithRolesAsync(user.Id) ?? user;
        return ToAdminDto(user);
    }

    public async Task<IReadOnlyList<UserAdminResponseDto>> GetUsersAsync() => (await _userRepository.GetAllWithRolesAsync())
        .Where(u => !u.UserRoles.Any(ur => ur.Role.Name.Equals(AppRoles.Patient, StringComparison.OrdinalIgnoreCase)))
        .Select(ToAdminDto)
        .ToList();

    public async Task<UserAdminResponseDto?> SetUserActiveStatusAsync(int userId, bool isActive)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(userId);
        if (user == null) return null;
        user.IsActive = isActive; user.UpdatedAtUtc = DateTime.UtcNow; await _userRepository.SaveChangesAsync(); return ToAdminDto(user);
    }

    public async Task<bool> SoftDeleteUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(userId);
        if (user == null) return false;
        user.IsDeleted = true; user.IsActive = false; user.UpdatedAtUtc = DateTime.UtcNow; await _userRepository.SaveChangesAsync(); return true;
    }

    private AuthResponseDto BuildAuthResponse(User user, PatientUserLink? link, string? fullName, bool isProfileCompleted)
    {
        var roles = user.UserRoles.Select(x => x.Role.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var tokenResult = _jwtService.GenerateToken(user, link, roles);
        return new AuthResponseDto { UserId = user.Id, PatientId = link?.PatientId ?? 0, UHID = link?.UHID ?? string.Empty, FullName = fullName ?? string.Empty, MobileNumber = user.MobileNumber, Roles = roles, AccessToken = tokenResult.Token, ExpiresAtUtc = tokenResult.ExpiresAtUtc, IsProfileCompleted = isProfileCompleted, IsPasswordLoginEnabled = user.IsPasswordLoginEnabled, IsOtpLoginEnabled = user.IsOtpLoginEnabled, IsFirstLoginCompleted = user.IsFirstLoginCompleted };
    }

    private static UserAdminResponseDto ToAdminDto(User user) => new() { UserId = user.Id, LoginId = user.LoginId, MobileNumber = user.MobileNumber, Email = user.Email, IsActive = user.IsActive, Roles = user.UserRoles.Select(x => x.Role.Name).Distinct().ToArray(), IsPasswordLoginEnabled = user.IsPasswordLoginEnabled, IsOtpLoginEnabled = user.IsOtpLoginEnabled, IsFirstLoginCompleted = user.IsFirstLoginCompleted };

    private async Task<(User User, string Mobile)> FindStaffUserForOtpAsync(string identifier)
    {
        var trimmed = identifier.Trim();
        User? user;
        string mobile;

        if (LooksLikeMobile(trimmed))
        {
            mobile = NormalizeMobile(trimmed);
            user = await _userRepository.GetByMobileWithRolesAsync(mobile);
        }
        else
        {
            user = await _userRepository.GetByLoginIdWithRolesAsync(trimmed);
            mobile = user?.MobileNumber ?? string.Empty;
        }

        if (user == null) throw new InvalidOperationException("No portal user found for this login id or mobile number.");
        if (string.IsNullOrWhiteSpace(mobile)) throw new InvalidOperationException("This user does not have a registered mobile number for OTP login.");
        return (user, NormalizeMobile(mobile));
    }

    private static string GetStaffIdentifier(string? loginId, string? mobileNumber)
    {
        var identifier = !string.IsNullOrWhiteSpace(loginId) ? loginId : mobileNumber;
        if (string.IsNullOrWhiteSpace(identifier)) throw new ArgumentException("Login id or mobile number is required.");
        return identifier.Trim();
    }

    private static bool LooksLikeMobile(string value)
    {
        var normalized = value.Trim().Replace(" ", "").Replace("-", "");
        if (normalized.StartsWith("+91")) normalized = normalized[3..];
        else if (normalized.StartsWith("91") && normalized.Length == 12) normalized = normalized[2..];
        return normalized.All(char.IsDigit) && normalized.Length >= 10;
    }

    private static string NormalizeMobile(string mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile)) throw new ArgumentException("Mobile number is required.");
        var value = mobile.Trim().Replace(" ", "").Replace("-", "");
        if (value.StartsWith("+91")) value = value[3..]; else if (value.StartsWith("91") && value.Length == 12) value = value[2..];
        return value;
    }
}

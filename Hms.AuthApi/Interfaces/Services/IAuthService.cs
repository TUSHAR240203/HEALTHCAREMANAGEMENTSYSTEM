using Hms.AuthApi.DTOs.Auth;

namespace Hms.AuthApi.Interfaces.Services;

public interface IAuthService
{
    Task SendLoginOtpAsync(int patientId, string number);
    Task<AuthResponseDto> PatientLoginAsync(LoginRequestDto request);
    Task SendStaffLoginOtpAsync(StaffOtpRequestDto request);
    Task<AuthResponseDto> StaffOtpLoginAsync(StaffOtpLoginRequestDto request);
    Task<AuthResponseDto> StaffLoginAsync(StaffLoginRequestDto request);
    Task<AuthResponseDto> UpdateAuthPreferenceAsync(int userId, AuthPreferenceRequestDto request);
    Task<CurrentUserResponseDto?> GetCurrentUserAsync(int userId);
    Task<UserAdminResponseDto> CreateStaffUserAsync(CreateStaffUserRequestDto request);
    Task<IReadOnlyList<UserAdminResponseDto>> GetUsersAsync();
    Task<UserAdminResponseDto?> SetUserActiveStatusAsync(int userId, bool isActive);
    Task<bool> SoftDeleteUserAsync(int userId);
}

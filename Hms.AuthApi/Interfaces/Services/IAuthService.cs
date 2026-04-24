using Hms.AuthApi.DTOs.Auth;

namespace Hms.AuthApi.Interfaces.Services;

public interface IAuthService
{
    Task SendPortalActivationAsync(SendPatientPortalActivationRequestDto request);
    Task<AuthResponseDto> VerifyOtpAndActivateAsync(VerifyOtpRequestDto request);

    Task SendLoginOtpAsync(int patientId); 
    Task<AuthResponseDto> PatientLoginAsync(PatientLoginRequestDto request);
    Task<CurrentUserResponseDto?> GetCurrentUserAsync(int userId);
}
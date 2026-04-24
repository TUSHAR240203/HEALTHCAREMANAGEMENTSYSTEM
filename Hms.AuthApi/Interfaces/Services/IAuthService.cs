using Hms.AuthApi.DTOs.Auth;

namespace Hms.AuthApi.Interfaces.Services;

public interface IAuthService
{
    Task SendPortalActivationAsync(SendPatientPortalActivationRequestDto request);
    Task<AuthResponseDto> VerifyOtpAndActivateAsync(VerifyOtpRequestDto request);

    Task SendLoginOtpAsync(int patientId,string number); 
    Task<AuthResponseDto> PatientLoginAsync(LoginRequestDto request);
    Task<CurrentUserResponseDto?> GetCurrentUserAsync(int userId);
}
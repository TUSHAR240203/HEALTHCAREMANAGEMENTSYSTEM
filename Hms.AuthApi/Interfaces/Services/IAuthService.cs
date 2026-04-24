using Hms.AuthApi.DTOs.Auth;

namespace Hms.AuthApi.Interfaces.Services;

public interface IAuthService
{
    Task SendPortalActivationAsync(SendPatientPortalActivationRequestDto request);
    Task<AuthResponseDto> VerifyOtpAndActivateAsync(VerifyOtpRequestDto request);

<<<<<<< HEAD
    Task SendLoginOtpAsync(int patientId); 
    Task<AuthResponseDto> PatientLoginAsync(PatientLoginRequestDto request);
=======
    Task SendLoginOtpAsync(int patientId,string number); 
    Task<AuthResponseDto> PatientLoginAsync(LoginRequestDto request);
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    Task<CurrentUserResponseDto?> GetCurrentUserAsync(int userId);
}
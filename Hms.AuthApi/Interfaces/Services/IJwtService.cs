using Hms.AuthApi.Entities;

namespace Hms.AuthApi.Interfaces.Services;

public interface IJwtService
{
<<<<<<< HEAD
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user, PatientUserLink? link);
=======
    (string Token, DateTime ExpiresAtUtc) GenerateToken(
        User user,
        PatientUserLink? link,
        IReadOnlyCollection<string> roles);
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
}
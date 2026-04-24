using Hms.AuthApi.Entities;

namespace Hms.AuthApi.Interfaces.Services;

public interface IJwtService
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(
        User user,
        PatientUserLink? link,
        IReadOnlyCollection<string> roles);
}
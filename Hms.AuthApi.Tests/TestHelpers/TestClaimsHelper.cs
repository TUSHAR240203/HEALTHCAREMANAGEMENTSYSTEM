using System.Security.Claims;

namespace Hms.AuthApi.Tests.TestHelpers;

public static class TestClaimsHelper
{
    public static ClaimsPrincipal GetUser(int userId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }
}
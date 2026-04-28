using Hms.AuthApi.Data;
using Microsoft.EntityFrameworkCore;

namespace Hms.AuthApi.Tests.TestHelpers;

public static class TestDbContextFactory
{
    public static AuthDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuthDbContext(options);
    }
}
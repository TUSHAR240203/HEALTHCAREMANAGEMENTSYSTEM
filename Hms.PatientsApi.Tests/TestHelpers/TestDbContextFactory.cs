using Hms.PatientsApi.Data;
using Microsoft.EntityFrameworkCore;

namespace Hms.PatientsApi.Tests.TestHelpers;

public static class TestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
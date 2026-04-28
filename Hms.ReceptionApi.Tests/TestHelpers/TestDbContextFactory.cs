using Hms.ReceptionApi.Data;
using Microsoft.EntityFrameworkCore;

namespace Hms.ReceptionApi.Tests.TestHelpers;

public static class TestDbContextFactory
{
    public static ReceptionDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ReceptionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ReceptionDbContext(options);
    }
}
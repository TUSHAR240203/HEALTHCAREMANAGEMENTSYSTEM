using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hms.AppointmentsApi.Data;

public class AppointmentsDbContextFactory : IDesignTimeDbContextFactory<AppointmentsDbContext>
{
    public AppointmentsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppointmentsDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=.\\sqlexpress;Database=Appointments-Healthcare;Trusted_Connection=True;TrustServerCertificate=True;");

        return new AppointmentsDbContext(optionsBuilder.Options);
    }
}
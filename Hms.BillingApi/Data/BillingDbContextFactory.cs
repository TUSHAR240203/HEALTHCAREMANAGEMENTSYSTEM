using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hms.BillingApi.Data;

public class BillingDbContextFactory : IDesignTimeDbContextFactory<BillingDbContext>
{
    public BillingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BillingDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=.\\sqlexpress;Database=Billing-Healthcare;Trusted_Connection=True;TrustServerCertificate=True;");

        return new BillingDbContext(optionsBuilder.Options);
    }
}
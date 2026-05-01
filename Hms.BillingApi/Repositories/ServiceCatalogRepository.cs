using Hms.BillingApi.Data;
using Hms.BillingApi.Entities;
using Hms.BillingApi.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hms.BillingApi.Repositories;

public class ServiceCatalogRepository : IServiceCatalogRepository
{
    private readonly BillingDbContext _context;

    public ServiceCatalogRepository(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceCatalog?> GetByIdAsync(int id)
        => await _context.ServiceCatalog.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

    public async Task<List<ServiceCatalog>> GetAllActiveAsync()
        => await _context.ServiceCatalog.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync();
}

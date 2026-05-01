using Hms.BillingApi.Entities;

namespace Hms.BillingApi.Interfaces;

public interface IServiceCatalogRepository
{
    Task<ServiceCatalog?> GetByIdAsync(int id);
    Task<List<ServiceCatalog>> GetAllActiveAsync();
}

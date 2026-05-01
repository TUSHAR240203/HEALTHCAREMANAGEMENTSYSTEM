namespace Hms.BillingApi.DTOs.Billing;

/// <summary>Returned to UI so receptionist can pick a service to add to an invoice.</summary>
public class ServiceCatalogResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public string Type { get; set; } = default!;
}

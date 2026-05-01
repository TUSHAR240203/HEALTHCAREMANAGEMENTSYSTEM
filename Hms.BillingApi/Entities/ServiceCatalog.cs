namespace Hms.BillingApi.Entities;

/// <summary>
/// Master list of services (tests, medicines) that receptionist can add to an invoice.
/// Prices are defined here and NOT accepted from any client request.
/// </summary>
public class ServiceCatalog
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }

    /// <summary>Test | Medicine</summary>
    public string Type { get; set; } = "Test";

    public bool IsActive { get; set; } = true;
}

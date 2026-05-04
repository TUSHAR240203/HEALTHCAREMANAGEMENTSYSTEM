namespace Frontend.Models.Billing
{
    public class ServiceCatalogResponseDto
    {
        public int Id { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public bool IsActive { get; set; }
    }
}
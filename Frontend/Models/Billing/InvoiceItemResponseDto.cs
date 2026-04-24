namespace Frontend.Models.Billing
{
    public class InvoiceItemResponseDto
    {
        public int Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}

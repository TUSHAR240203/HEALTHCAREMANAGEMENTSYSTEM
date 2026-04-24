namespace Frontend.Models.Billing
{
    public class AddInvoiceItemRequestDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}

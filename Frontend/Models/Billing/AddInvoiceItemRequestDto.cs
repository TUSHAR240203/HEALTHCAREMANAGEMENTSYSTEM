namespace Frontend.Models.Billing
{
    public class AddInvoiceItemRequestDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;

        public decimal Amount
        {
            get => Price * Quantity;
            set
            {
                Price = value;
                Quantity = 1;
            }
        }
    }
}

namespace Frontend.Models.Billing
{
    public class PaymentRequestDto
    {
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
    }
}

namespace Frontend.Models.Billing
{
    public class PaymentRequestDto
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;

        public string PaymentMode
        {
            get => PaymentMethod;
            set => PaymentMethod = value;
        }
    }
}

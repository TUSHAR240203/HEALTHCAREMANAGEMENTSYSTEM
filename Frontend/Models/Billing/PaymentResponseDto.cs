namespace Frontend.Models.Billing
{
    public class PaymentResponseDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentMode => PaymentMethod;
        public DateTime PaidAtUtc { get; set; }
    }
}

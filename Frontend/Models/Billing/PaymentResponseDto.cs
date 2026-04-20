namespace Frontend.Models.Billing
{
    public class PaymentResponseDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public DateTime PaidAtUtc { get; set; }
    }
}

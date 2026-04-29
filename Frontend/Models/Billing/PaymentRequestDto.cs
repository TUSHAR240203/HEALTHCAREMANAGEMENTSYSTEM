namespace Frontend.Models.Billing
{
    public class PaymentRequestDto
    {
        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; } = "Cash";

        public string PaymentMode
        {
            get => PaymentMethod;
            set => PaymentMethod = string.IsNullOrWhiteSpace(value) ? "Cash" : value;
        }

        public string? ReferenceNumber { get; set; }

        public string? Notes { get; set; }

        public DateTime PaymentDateUtc { get; set; } = DateTime.UtcNow;
    }
}
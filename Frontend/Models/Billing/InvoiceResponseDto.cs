using System.Collections.Generic;

namespace Frontend.Models.Billing
{
    public class InvoiceResponseDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int PatientId { get; set; }
        public string UHID { get; set; } = string.Empty;
        public int? AppointmentId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsClosed { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public List<InvoiceItemResponseDto> Items { get; set; } = new();
        public List<PaymentResponseDto> Payments { get; set; } = new();
    }
}

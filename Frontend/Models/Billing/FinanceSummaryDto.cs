namespace Frontend.Models.Billing
{
    public class FinanceSummaryDto
    {
        public decimal GrossRevenue { get; set; }
        public decimal PaidRevenue { get; set; }
        public decimal OutstandingBalance { get; set; }
        public int InvoiceCount { get; set; }
        public int PaidInvoiceCount { get; set; }
        public int PendingInvoiceCount { get; set; }
        public decimal CollectionRatePercent { get; set; }
    }
}

namespace Frontend.Models.Billing
{
    public class FinanceDashboardViewModel
    {
        public FinanceSummaryDto Summary { get; set; } = new();
        public PagedResultDto<InvoiceResponseDto> Invoices { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }
}

using Hms.BillingApi.DTOs.Finance;
using Hms.BillingApi.Entities;

namespace Hms.BillingApi.Finance;

public sealed class FinanceCalculator : IFinanceCalculator
{
    public FinanceSummaryDto BuildSummary(IEnumerable<Invoice> invoices)
    {
        var invoiceList = invoices.ToList();
        var gross = invoiceList.Sum(x => x.TotalAmount);
        var paid = invoiceList.Sum(x => x.PaidAmount);
        var outstanding = invoiceList.Sum(x => x.BalanceAmount);

        return new FinanceSummaryDto
        {
            GrossRevenue = gross,
            PaidRevenue = paid,
            OutstandingBalance = outstanding,
            InvoiceCount = invoiceList.Count,
            PaidInvoiceCount = invoiceList.Count(x => string.Equals(x.Status, "Paid", StringComparison.OrdinalIgnoreCase)),
            PendingInvoiceCount = invoiceList.Count(x => !string.Equals(x.Status, "Paid", StringComparison.OrdinalIgnoreCase)),
            CollectionRatePercent = gross <= 0 ? 0 : Math.Round((paid / gross) * 100, 2)
        };
    }
}

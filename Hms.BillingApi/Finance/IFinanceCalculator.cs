using Hms.BillingApi.DTOs.Finance;
using Hms.BillingApi.Entities;

namespace Hms.BillingApi.Finance;

public interface IFinanceCalculator
{
    FinanceSummaryDto BuildSummary(IEnumerable<Invoice> invoices);
}

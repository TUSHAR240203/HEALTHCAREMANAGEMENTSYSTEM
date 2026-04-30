using Hms.BillingApi.Common;
using Hms.BillingApi.Data;
using Hms.BillingApi.DTOs.Finance;
using Hms.BillingApi.Entities;
using Hms.BillingApi.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Hms.BillingApi.Controllers;

[ApiController]
[Authorize(Policy = "BillingAccess")]
[Route("api/v1/finance")]
public sealed class FinanceController : ControllerBase
{
    private const string SummaryCacheKey = "finance-summary-v1";
    private readonly BillingDbContext _dbContext;
    private readonly IFinanceCalculator _financeCalculator;
    private readonly IMemoryCache _cache;

    public FinanceController(BillingDbContext dbContext, IFinanceCalculator financeCalculator, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _financeCalculator = financeCalculator;
        _cache = cache;
    }

    [HttpGet("summary")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public async Task<ActionResult<FinanceSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _cache.GetOrCreateAsync(SummaryCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            var invoices = await _dbContext.Invoices.AsNoTracking().ToListAsync(cancellationToken);
            return _financeCalculator.BuildSummary(invoices);
        });

        Response.Headers["X-Api-Version"] = "1.0";
        return Ok(summary);
    }

    [HttpGet("invoices")]
    public async Task<ActionResult<PagedResult<Invoice>>> GetInvoices(
        [FromQuery] PaginationParameters pagination,
        CancellationToken cancellationToken)
    {
        var invoices = await _dbContext.Invoices
            .AsNoTracking()
            .OrderByDescending(invoice => invoice.Id)
            .ToPagedResultAsync(pagination, cancellationToken);

        Response.Headers["X-Api-Version"] = "1.0";
        Response.Headers["X-Total-Count"] = invoices.TotalCount.ToString();
        Response.Headers["X-Page-Number"] = invoices.PageNumber.ToString();
        Response.Headers["X-Page-Size"] = invoices.PageSize.ToString();
        Response.Headers["X-Total-Pages"] = invoices.TotalPages.ToString();

        return Ok(invoices);
    }
}

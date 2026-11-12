using Microsoft.EntityFrameworkCore;
using SalesFlow.Application.Abstractions;
using SalesFlow.Application.Common.Errors;
using SalesFlow.Domain.Entities;
using SalesFlow.Domain.Enums;

namespace SalesFlow.Infrastructure.Persistence.Reports;

public sealed class EfSalesDashboardReader : ISalesDashboardReader
{
    private readonly SalesFlowDbContext _db;
    public EfSalesDashboardReader(SalesFlowDbContext db) => _db = db;
    public async Task<SalesDashboardDto> ReadAsync(Guid tenantId, CancellationToken ct)
    {
        if (_db.CurrentTenantId != tenantId || tenantId == Guid.Empty) throw new ForbiddenException("Tenant mismatch.");
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var customers = await _db.Set<Customer>().AsNoTracking().CountAsync(x => x.TenantId == tenantId && x.IsActive, ct);
        var leads = await _db.Set<Lead>().AsNoTracking().CountAsync(x => x.TenantId == tenantId, ct);
        var tasks = await _db.Set<SalesTask>().AsNoTracking().CountAsync(x => x.TenantId == tenantId && x.Status == SalesTaskStatus.Open, ct);
        var overdueTasks = await _db.Set<SalesTask>().AsNoTracking().CountAsync(x => x.TenantId == tenantId && x.Status == SalesTaskStatus.Open && x.DueAt != null && x.DueAt < now, ct);
        var overdueInvoices = await _db.Set<SalesInvoice>().AsNoTracking().CountAsync(x => x.TenantId == tenantId && x.Status == InvoiceStatus.Issued && x.DueDate < today, ct);
        var deals = await _db.Set<Deal>().AsNoTracking().Where(x => x.TenantId == tenantId)
            .GroupBy(x => new { x.Stage, x.Currency })
            .Select(g => new StageCurrencyMetric(g.Key.Stage, g.Key.Currency, g.Count(), g.Sum(x => x.Amount)))
            .ToListAsync(ct);
        var won = deals.Where(x => x.Stage == DealStage.Won).Sum(x => x.Count);
        var lost = deals.Where(x => x.Stage == DealStage.Lost).Sum(x => x.Count);
        var total = deals.Sum(x => x.Count);
        var winRate = won + lost == 0 ? 0m : decimal.Round(100m * won / (won + lost), 2);
        var summaries = deals.GroupBy(x => x.Currency).Select(g => new CurrencySummary(
            g.Key,
            g.Where(x => x.Stage is not DealStage.Won and not DealStage.Lost).Sum(x => x.Amount),
            decimal.Round(g.Sum(x => x.Amount * Probability(x.Stage)), 2),
            g.Where(x => x.Stage == DealStage.Won).Sum(x => x.Amount))).ToArray();
        return new SalesDashboardDto(customers, leads, tasks, overdueTasks, total, won, lost, overdueInvoices, winRate, deals, summaries);
    }
    private static decimal Probability(DealStage stage) => stage switch
    {
        DealStage.Prospecting => 0.10m,
        DealStage.Qualification => 0.25m,
        DealStage.Proposal => 0.50m,
        DealStage.Negotiation => 0.75m,
        _ => 0m
    };
}

using SalesFlow.Domain.Enums;

namespace SalesFlow.Application.Abstractions;

public sealed record StageCurrencyMetric(DealStage Stage, string Currency, int Count, decimal Amount);
public sealed record CurrencySummary(string Currency, decimal OpenPipeline, decimal WeightedForecast, decimal WonRevenue);
public sealed record SalesDashboardDto(int TotalCustomers, int TotalLeads, int OpenTasks, int OverdueTasks,
    int TotalDeals, int WonDeals, int LostDeals, int OverdueInvoices, decimal ClosedWinRatePercent,
    IReadOnlyList<StageCurrencyMetric> ByStage, IReadOnlyList<CurrencySummary> ByCurrency);
public interface ISalesDashboardReader
{
    Task<SalesDashboardDto> ReadAsync(Guid tenantId, CancellationToken cancellationToken);
}

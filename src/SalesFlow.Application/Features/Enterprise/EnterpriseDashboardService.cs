using SalesFlow.Application.Abstractions;
using SalesFlow.Application.Security;

namespace SalesFlow.Application.Features.Enterprise;

public sealed class EnterpriseDashboardService
{
    private readonly AccessGuard _guard;
    private readonly ISalesDashboardReader _reader;
    public EnterpriseDashboardService(AccessGuard guard, ISalesDashboardReader reader) => (_guard, _reader) = (guard, reader);
    public async Task<SalesDashboardDto> Get(CancellationToken ct)
    {
        var access = await _guard.RequireAsync(ct);
        return await _reader.ReadAsync(access.TenantId, ct);
    }
}

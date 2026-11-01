using System.Linq.Expressions;
using SalesFlow.Application.Abstractions;
using SalesFlow.Application.Common.Errors;
using SalesFlow.Application.Common.Pagination;
using SalesFlow.Domain.Common;

namespace SalesFlow.Application.Common.Data;
public static class RepositoryExtensions
{
    public static async Task<TEntity> GetRequiredAsync<TEntity>(this ITenantRepository<TEntity> repository, Guid tenantId, Guid id, CancellationToken cancellationToken = default) where TEntity : TenantEntity
    {
        var entity = await repository.GetAsync(tenantId, id, cancellationToken);
        if (entity is null || entity.TenantId != tenantId) throw new NotFoundException($"{typeof(TEntity).Name} not found.");
        return entity;
    }
    public static async Task<PagedResult<TEntity>> PageOwnedAsync<TEntity>(this ITenantRepository<TEntity> repository, Guid tenantId, PageRequest page, Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default) where TEntity : TenantEntity
    {
        var result = await repository.PageAsync(tenantId, page, filter, cancellationToken);
        if (result.Items.Any(x => x.TenantId != tenantId)) throw new ForbiddenException("Repository returned data from a different tenant.");
        return result;
    }
}

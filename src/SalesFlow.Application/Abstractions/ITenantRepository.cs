using System.Linq.Expressions;
using SalesFlow.Application.Common.Pagination;
using SalesFlow.Domain.Common;

namespace SalesFlow.Application.Abstractions;
public interface ITenantRepository<TEntity> where TEntity : TenantEntity
{
    Task<TEntity?> GetAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<TEntity>> PageAsync(Guid tenantId, PageRequest page, Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Guid tenantId, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
}

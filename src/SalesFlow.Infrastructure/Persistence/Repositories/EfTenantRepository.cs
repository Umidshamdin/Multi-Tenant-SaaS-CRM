using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SalesFlow.Application.Abstractions;
using SalesFlow.Application.Common.Errors;
using SalesFlow.Application.Common.Pagination;
using SalesFlow.Domain.Common;
using SalesFlow.Infrastructure.Persistence;

namespace SalesFlow.Infrastructure.Persistence.Repositories;

public class EfTenantRepository<TEntity> : ITenantRepository<TEntity> where TEntity : TenantEntity
{
    protected readonly SalesFlowDbContext Context;
    protected DbSet<TEntity> Set => Context.Set<TEntity>();

    public EfTenantRepository(SalesFlowDbContext context) => Context = context;

    protected void RequireSelectedTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty || Context.CurrentTenantId != tenantId) throw new ForbiddenException("The requested tenant does not match the active tenant.");
    }

    public Task<TEntity?> GetAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        RequireSelectedTenant(tenantId);
        return Set.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken);
    }

    public async Task<PagedResult<TEntity>> PageAsync(Guid tenantId, PageRequest page, Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default)
    {
        RequireSelectedTenant(tenantId);
        IQueryable<TEntity> query = Set.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (filter is not null) query = query.Where(filter);
        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id).Skip((page.Page - 1) * page.PageSize).Take(page.PageSize).ToListAsync(cancellationToken);
        return new PagedResult<TEntity>(items, page.Page, page.PageSize, count);
    }

    public Task<bool> AnyAsync(Guid tenantId, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        RequireSelectedTenant(tenantId);
        return Set.AsNoTracking().Where(x => x.TenantId == tenantId).AnyAsync(filter, cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (Context.CurrentTenantId.HasValue && Context.CurrentTenantId.Value != entity.TenantId) throw new ForbiddenException("Cross-tenant write denied.");
        // No tenant selection is allowed only for the validated tenant-onboarding transaction in DbContext.SaveChanges.
        await Set.AddAsync(entity, cancellationToken);
    }
}

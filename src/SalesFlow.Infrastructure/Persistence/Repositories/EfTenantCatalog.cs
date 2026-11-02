using Microsoft.EntityFrameworkCore;
using SalesFlow.Application.Abstractions;
using SalesFlow.Application.Common.Errors;
using SalesFlow.Domain.Entities;

namespace SalesFlow.Infrastructure.Persistence.Repositories;

public sealed class EfTenantCatalog : ITenantCatalog
{
    private readonly SalesFlowDbContext _context;
    public EfTenantCatalog(SalesFlowDbContext context) => _context = context;

    public Task<Tenant?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) return Task.FromResult<Tenant?>(null);
        return _context.Tenants.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug)) return Task.FromResult<Tenant?>(null);
        var normalized = slug.Trim().ToLowerInvariant();
        return _context.Tenants.FirstOrDefaultAsync(x => x.Slug == normalized, cancellationToken);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        if (_context.CurrentTenantId.HasValue) throw new ForbiddenException("Create new workspaces without an active tenant selection.");
        await _context.Tenants.AddAsync(tenant, cancellationToken);
    }
}

using SalesFlow.Domain.Entities;

namespace SalesFlow.Application.Abstractions;
public interface ITenantCatalog
{
    Task<Tenant?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
}

using SalesFlow.Domain.Entities;

namespace SalesFlow.Application.Abstractions;
public interface ITenantMemberRepository : ITenantRepository<TenantMember>
{
    Task<TenantMember?> GetByIdentityAsync(Guid tenantId, string userId, CancellationToken cancellationToken = default);
}

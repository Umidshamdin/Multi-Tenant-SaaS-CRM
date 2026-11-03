using Microsoft.EntityFrameworkCore;
using SalesFlow.Application.Abstractions;
using SalesFlow.Domain.Entities;

namespace SalesFlow.Infrastructure.Persistence.Repositories;

public sealed class EfTenantMemberRepository : EfTenantRepository<TenantMember>, ITenantMemberRepository
{
    public EfTenantMemberRepository(SalesFlowDbContext context) : base(context) { }

    public Task<TenantMember?> GetByIdentityAsync(Guid tenantId, string userId, CancellationToken cancellationToken = default)
    {
        RequireSelectedTenant(tenantId);
        return Set.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.IdentityUserId == userId, cancellationToken);
    }
}

using SalesFlow.Application.Abstractions;
using SalesFlow.Application.Common.Errors;
using SalesFlow.Domain.Enums;

namespace SalesFlow.Application.Security;

public sealed class AccessGuard
{
    private readonly ICurrentUser _user;
    private readonly ITenantCatalog _tenants;
    private readonly ITenantMemberRepository _members;

    public AccessGuard(ICurrentUser user, ITenantCatalog tenants, ITenantMemberRepository members)
    {
        _user = user;
        _tenants = tenants;
        _members = members;
    }

    public async Task<AccessContext> RequireAsync(CancellationToken cancellationToken = default, params MemberRole[] roles)
    {
        var userId = _user.UserId;
        var selectedTenant = _user.TenantId;
        if (string.IsNullOrWhiteSpace(userId) || selectedTenant is null || selectedTenant == Guid.Empty)
            throw new UnauthorizedAccessException("An authenticated user and an active tenant selection are required.");

        var tenantId = selectedTenant.Value;
        var tenant = await _tenants.GetAsync(tenantId, cancellationToken);
        if (tenant is null || tenant.Status != TenantStatus.Active) throw new ForbiddenException("Tenant is not active or does not exist.");

        var member = await _members.GetByIdentityAsync(tenantId, userId, cancellationToken);
        if (member is null || !member.IsActive) throw new ForbiddenException("You are not an active member of this tenant.");
        if (roles.Length > 0 && !roles.Contains(member.Role)) throw new ForbiddenException("Insufficient permissions.");

        return new AccessContext(tenantId, userId, member.Id, member.Role);
    }
}

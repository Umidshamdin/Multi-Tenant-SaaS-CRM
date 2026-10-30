using SalesFlow.Domain.Common;
using SalesFlow.Domain.Enums;
using SalesFlow.Domain.Exceptions;

namespace SalesFlow.Domain.Entities;
public sealed class TenantMember : TenantEntity
{
    public string IdentityUserId { get; private set; } = string.Empty;
    public MemberRole Role { get; private set; }
    public bool IsActive { get; private set; }

    private TenantMember() { }

    public TenantMember(Guid tenantId, string identityUserId, MemberRole role) : base(tenantId)
    {
        IdentityUserId = DomainRules.Required(identityUserId, nameof(IdentityUserId), 450);
        if (!Enum.IsDefined(role)) throw new DomainException("Invalid member role.");
        Role = role;
        IsActive = true;
    }
    public void ChangeRole(MemberRole newRole)
    {
        if (!Enum.IsDefined(newRole)) throw new DomainException("Invalid member role.");
        if (Role == MemberRole.Owner || newRole == MemberRole.Owner)
            throw new DomainException("Owner role transfers require a dedicated secured operation.");

        Role = newRole;
        Touch();
    }
    public void Deactivate()
    {
        if (Role == MemberRole.Owner) throw new DomainException("Tenant owner cannot be deactivated.");
        if (!IsActive) return;
        IsActive = false;
        Touch();
    }
    public void Reactivate()
    {
        if (IsActive) return;
        IsActive = true;
        Touch();
    }
}
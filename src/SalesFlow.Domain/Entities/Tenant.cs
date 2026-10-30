using SalesFlow.Domain.Common;
using SalesFlow.Domain.Enums;

namespace SalesFlow.Domain.Entities;
public sealed class Tenant : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public TenantStatus Status { get; private set; }

    private Tenant() { }

    public Tenant(string name, string slug) : base(Guid.NewGuid())
    {
        Name = DomainRules.Required(name, nameof(Name), 160);
        Slug = DomainRules.Slug(slug);
        Status = TenantStatus.Active;
    }
    public void Rename(string name)
    {
        Name = DomainRules.Required(name, nameof(Name), 160);
        Touch();
    }
    public void Suspend()
    {
        if (Status == TenantStatus.Suspended) return;
        Status = TenantStatus.Suspended;
        Touch();
    }
    public void Activate()
    {
        if (Status == TenantStatus.Active) return;
        Status = TenantStatus.Active;
        Touch();
    }
}
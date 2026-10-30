namespace SalesFlow.Domain.Common;
public abstract class TenantEntity : Entity
{
    public Guid TenantId { get; private set; }
    protected TenantEntity() { }
    protected TenantEntity(Guid tenantId) : base(Guid.NewGuid())
    {
        DomainRules.RequiredId(tenantId, nameof(TenantId));
        TenantId = tenantId;
    }
}

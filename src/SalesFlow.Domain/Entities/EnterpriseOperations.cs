using SalesFlow.Domain.Common;
using SalesFlow.Domain.Enums;
using SalesFlow.Domain.Exceptions;

namespace SalesFlow.Domain.Entities;

public sealed class SalesGoal : TenantEntity
{
    public Guid MemberId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public decimal TargetAmount { get; private set; }
    private SalesGoal() { }
    public SalesGoal(Guid tenantId, Guid memberId, int year, int month, string currency, decimal amount) : base(tenantId)
    {
        DomainRules.RequiredId(memberId, nameof(memberId));
        if (year is < 2000 or > 2100 || month is < 1 or > 12 || amount < 0 || amount > 999_999_999_999m)
            throw new DomainException("Invalid sales goal.");
        MemberId = memberId;
        Year = year;
        Month = month;
        Currency = DomainRules.Currency(currency);
        TargetAmount = amount;
    }
    public void Change(decimal target)
    {
        if (target < 0 || target > 999_999_999_999m) throw new DomainException("Invalid target amount.");
        TargetAmount = target;
        Touch();
    }
}

public sealed class CrmAuditEvent : TenantEntity
{
    public Guid ActorMemberId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string Details { get; private set; } = string.Empty;
    private CrmAuditEvent() { }
    public CrmAuditEvent(Guid tenantId, Guid actorMemberId, string action, string entityType, Guid entityId, string details) : base(tenantId)
    {
        DomainRules.RequiredId(actorMemberId, nameof(actorMemberId));
        DomainRules.RequiredId(entityId, nameof(entityId));
        ActorMemberId = actorMemberId;
        Action = DomainRules.Required(action, nameof(Action), 80);
        EntityType = DomainRules.Required(entityType, nameof(EntityType), 100);
        Details = DomainRules.Required(details, nameof(Details), 2000);
    }
}

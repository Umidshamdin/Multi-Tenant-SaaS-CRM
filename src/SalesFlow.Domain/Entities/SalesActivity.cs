using SalesFlow.Domain.Common;
using SalesFlow.Domain.Enums;
using SalesFlow.Domain.Exceptions;

namespace SalesFlow.Domain.Entities;
public sealed class SalesActivity : TenantEntity
{
    public Guid CustomerId { get; private set; }
    public Guid? DealId { get; private set; }
    public Guid CreatedByMemberId { get; private set; }

    public ActivityKind Kind { get; private set; }

    public string Subject { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public DateTime OccurredAtUtc { get; private set; }

    private SalesActivity() { }

    public SalesActivity(Guid tenantId, Guid customerId, Guid? dealId, Guid createdByMemberId, ActivityKind kind, string subject, string? description,
        DateTime occurredAtUtc) : base(tenantId)
    {
        DomainRules.RequiredId(customerId, nameof(CustomerId));

        DomainRules.RequiredId(createdByMemberId, nameof(CreatedByMemberId));

        if (dealId == Guid.Empty) throw new DomainException("Invalid deal ID.");

        if (!Enum.IsDefined(kind)) throw new DomainException("Invalid activity kind.");

        if (occurredAtUtc.Kind != DateTimeKind.Utc) throw new DomainException("Activity date must be in UTC.");

        CustomerId = customerId;
        DealId = dealId;
        CreatedByMemberId = createdByMemberId;
        Kind = kind;
        Subject = DomainRules.Required(subject, nameof(Subject), 200);
        Description = DomainRules.Optional(description, nameof(Description), 4000);
        OccurredAtUtc = occurredAtUtc;
    }
}

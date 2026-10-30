using SalesFlow.Domain.Common;
using SalesFlow.Domain.Enums;
using SalesFlow.Domain.Exceptions;

namespace SalesFlow.Domain.Entities;

public sealed class SalesTask : TenantEntity
{
    public string Title { get; private set; } = string.Empty;
    public Guid? AssignedMemberId { get; private set; }
    public DateTime? DueAt { get; private set; }
    public SalesTaskStatus Status { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private SalesTask() { }

    public SalesTask(Guid tenantId, string title, Guid? assignedMemberId = null, DateTime? dueAt = null) : base(tenantId)
    {
        Title = DomainRules.Required(title, nameof(Title), 200);
        if (assignedMemberId.HasValue) DomainRules.RequiredId(assignedMemberId.Value, nameof(AssignedMemberId));
        ValidateLocal(dueAt);
        AssignedMemberId = assignedMemberId;
        DueAt = dueAt;
        Status = SalesTaskStatus.Open;
    }
    public void AssignTo(Guid memberId)
    {
        EnsureOpen();
        DomainRules.RequiredId(memberId, nameof(memberId));
        AssignedMemberId = memberId;
        Touch();
    }
    public void SetDueDate(DateTime? dueAt)
    {
        EnsureOpen();
        ValidateLocal(dueAt);
        DueAt = dueAt;
        Touch();
    }
    public void Complete()
    {
        EnsureOpen();
        Status = SalesTaskStatus.Completed;
        CompletedAt = DateTime.Now;
        Touch();
    }
    private void EnsureOpen()
    {
        if (Status == SalesTaskStatus.Completed) throw new DomainException("Completed tasks cannot be modified.");
    }
    private static void ValidateLocal(DateTime? date)
    {
        if (date.HasValue && date.Value.Kind != DateTimeKind.Local) throw new DomainException("DueAt must be a local DateTime.");
    }
}
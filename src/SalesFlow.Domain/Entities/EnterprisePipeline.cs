using SalesFlow.Domain.Common;
using SalesFlow.Domain.Enums;
using SalesFlow.Domain.Exceptions;

namespace SalesFlow.Domain.Entities;

public sealed class CrmPipeline : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    private CrmPipeline() { }
    public CrmPipeline(Guid tenantId, string name) : base(tenantId)
    {
        Name = DomainRules.Required(name, nameof(Name), 120);
        IsActive = true;
    }
    public void Rename(string name) { Name = DomainRules.Required(name, nameof(Name), 120); Touch(); }
    public void Archive() { if (!IsActive) return; IsActive = false; Touch(); }
}

public sealed class CrmPipelineStage : TenantEntity
{
    public Guid PipelineId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DealStage Kind { get; private set; }
    public int Position { get; private set; }
    public int ProbabilityPercent { get; private set; }
    private CrmPipelineStage() { }
    public CrmPipelineStage(Guid tenantId, Guid pipelineId, string name, DealStage kind, int position, int probabilityPercent) : base(tenantId)
    {
        DomainRules.RequiredId(pipelineId, nameof(pipelineId));
        if (!Enum.IsDefined(kind) || position < 1 || position > 100 || probabilityPercent is < 0 or > 100)
            throw new DomainException("Invalid pipeline stage configuration.");
        PipelineId = pipelineId;
        Name = DomainRules.Required(name, nameof(Name), 100);
        Kind = kind;
        Position = position;
        ProbabilityPercent = probabilityPercent;
    }
}

public sealed class CrmDealPipelinePosition : TenantEntity
{
    public Guid DealId { get; private set; }
    public Guid PipelineId { get; private set; }
    public Guid StageId { get; private set; }
    private CrmDealPipelinePosition() { }
    public CrmDealPipelinePosition(Guid tenantId, Guid dealId, Guid pipelineId, Guid stageId) : base(tenantId)
    {
        DomainRules.RequiredId(dealId, nameof(dealId));
        DomainRules.RequiredId(pipelineId, nameof(pipelineId));
        DomainRules.RequiredId(stageId, nameof(stageId));
        DealId = dealId;
        PipelineId = pipelineId;
        StageId = stageId;
    }
    public void Move(Guid pipelineId, Guid stageId)
    {
        DomainRules.RequiredId(pipelineId, nameof(pipelineId));
        DomainRules.RequiredId(stageId, nameof(stageId));
        PipelineId = pipelineId;
        StageId = stageId;
        Touch();
    }
}

using SalesFlow.Application.Abstractions;
using SalesFlow.Application.Common.Errors;
using SalesFlow.Application.Common.Pagination;
using SalesFlow.Application.Security;
using SalesFlow.Domain.Entities;
using SalesFlow.Domain.Enums;

namespace SalesFlow.Application.Features.Enterprise;

public sealed class EnterprisePipelineService : EnterpriseServiceBase
{
    private readonly ITenantRepository<CrmPipeline> _pipelines;
    private readonly ITenantRepository<CrmPipelineStage> _stages;
    private readonly ITenantRepository<CrmDealPipelinePosition> _positions;
    private readonly ITenantRepository<Deal> _deals;
    public EnterprisePipelineService(AccessGuard g, IUnitOfWork u, ITenantRepository<CrmAuditEvent> a,
        ITenantRepository<CrmPipeline> pipelines, ITenantRepository<CrmPipelineStage> stages,
        ITenantRepository<CrmDealPipelinePosition> positions, ITenantRepository<Deal> deals) : base(g, u, a)
        => (_pipelines, _stages, _positions, _deals) = (pipelines, stages, positions, deals);
    public async Task<PagedResult<CrmPipeline>> List(PageRequest page, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        return await _pipelines.PageAsync(a.TenantId, page, null, ct);
    }
    public async Task<PagedResult<CrmPipelineStage>> Stages(Guid pipelineId, PageRequest page, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        await Required(_pipelines, a.TenantId, pipelineId, ct);
        return await _stages.PageAsync(a.TenantId, page, x => x.PipelineId == pipelineId, ct);
    }
    public async Task<CrmPipeline> Create(CreatePipelineRequest req, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct, MemberRole.Owner, MemberRole.Admin, MemberRole.Manager);
        var p = new CrmPipeline(a.TenantId, req.Name);
        if (await _pipelines.AnyAsync(a.TenantId, x => x.Name == p.Name, ct)) throw new ConflictException("Pipeline name already exists.");
        await _pipelines.AddAsync(p, ct);
        await Audit(a, "create", nameof(CrmPipeline), p.Id, "Pipeline created", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return p;
    }
    public async Task<CrmPipelineStage> CreateStage(Guid pipelineId, CreatePipelineStageRequest req, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct, MemberRole.Owner, MemberRole.Admin, MemberRole.Manager);
        var p = await Required(_pipelines, a.TenantId, pipelineId, ct);
        if (!p.IsActive) throw new ConflictException("Pipeline is archived.");
        if (await _stages.AnyAsync(a.TenantId, x => x.PipelineId == pipelineId && x.Position == req.Position, ct))
            throw new ConflictException("Stage position already exists.");
        var stage = new CrmPipelineStage(a.TenantId, pipelineId, req.Name, req.Kind, req.Position, req.ProbabilityPercent);
        await _stages.AddAsync(stage, ct);
        await Audit(a, "add-stage", nameof(CrmPipeline), p.Id, "Pipeline stage created", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return stage;
    }
    public async Task<CrmDealPipelinePosition> PlaceDeal(Guid dealId, PlaceDealRequest req, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        var deal = await Required(_deals, a.TenantId, dealId, ct);
        var pipeline = await Required(_pipelines, a.TenantId, req.PipelineId, ct);
        var stage = await Required(_stages, a.TenantId, req.StageId, ct);
        if (!pipeline.IsActive || stage.PipelineId != pipeline.Id) throw new ConflictException("Stage does not belong to an active pipeline.");
        if (deal.Stage != stage.Kind)
        {
            if (stage.Kind == DealStage.Won) deal.MarkWon();
            else if (stage.Kind == DealStage.Lost) deal.MarkLost();
            else deal.MoveTo(stage.Kind);
        }
        var exists = await _positions.PageAsync(a.TenantId, new PageRequest(1, 1), x => x.DealId == deal.Id, ct);
        var position = exists.Items.FirstOrDefault();
        if (position is not null) position = await Required(_positions, a.TenantId, position.Id, ct);
        if (position is not null && (deal.Stage is DealStage.Won or DealStage.Lost) &&
            (position.PipelineId != pipeline.Id || position.StageId != stage.Id))
            throw new ConflictException("Closed deals cannot change pipeline position.");
        if (position is null)
        {
            position = new CrmDealPipelinePosition(a.TenantId, deal.Id, pipeline.Id, stage.Id);
            await _positions.AddAsync(position, ct);
        }
        else position.Move(pipeline.Id, stage.Id);
        await Audit(a, "move", nameof(Deal), dealId, $"Pipeline stage changed to {stage.Name}", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return position;
    }
}

public sealed class EnterpriseRecordGuard
{
    private readonly ITenantRepository<Customer> _customers;
    private readonly ITenantRepository<CustomerContact> _contacts;
    private readonly ITenantRepository<Lead> _leads;
    private readonly ITenantRepository<Deal> _deals;
    private readonly ITenantRepository<Product> _products;
    public EnterpriseRecordGuard(ITenantRepository<Customer> customers, ITenantRepository<CustomerContact> contacts,
        ITenantRepository<Lead> leads, ITenantRepository<Deal> deals, ITenantRepository<Product> products)
        => (_customers, _contacts, _leads, _deals, _products) = (customers, contacts, leads, deals, products);
    public async Task EnsureExists(Guid tenantId, CrmRecordKind kind, Guid id, CancellationToken ct)
    {
        var found = kind switch
        {
            CrmRecordKind.Customer => await _customers.GetAsync(tenantId, id, ct) is not null,
            CrmRecordKind.Contact => await _contacts.GetAsync(tenantId, id, ct) is not null,
            CrmRecordKind.Lead => await _leads.GetAsync(tenantId, id, ct) is not null,
            CrmRecordKind.Deal => await _deals.GetAsync(tenantId, id, ct) is not null,
            CrmRecordKind.Product => await _products.GetAsync(tenantId, id, ct) is not null,
            _ => false
        };
        if (!found) throw new NotFoundException("CRM record does not exist in the selected tenant.");
    }
}

public sealed class EnterpriseCollaborationService : EnterpriseServiceBase
{
    private readonly EnterpriseRecordGuard _records;
    private readonly ITenantRepository<CrmNote> _notes;
    private readonly ITenantRepository<CrmTag> _tags;
    private readonly ITenantRepository<CrmRecordTag> _assigned;
    public EnterpriseCollaborationService(AccessGuard g, IUnitOfWork u, ITenantRepository<CrmAuditEvent> a,
        EnterpriseRecordGuard records, ITenantRepository<CrmNote> notes, ITenantRepository<CrmTag> tags,
        ITenantRepository<CrmRecordTag> assigned) : base(g, u, a)
        => (_records, _notes, _tags, _assigned) = (records, notes, tags, assigned);
    public async Task<PagedResult<CrmNote>> Notes(CrmRecordKind kind, Guid recordId, PageRequest page, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        await _records.EnsureExists(a.TenantId, kind, recordId, ct);
        return await _notes.PageAsync(a.TenantId, page, x => x.RecordKind == kind && x.RecordId == recordId && x.IsActive, ct);
    }
    public async Task<CrmNote> AddNote(CreateNoteRequest req, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        await _records.EnsureExists(a.TenantId, req.RecordKind, req.RecordId, ct);
        var n = new CrmNote(a.TenantId, req.RecordKind, req.RecordId, a.MemberId, req.Body);
        await _notes.AddAsync(n, ct);
        await Audit(a, "add-note", nameof(CrmNote), n.Id, "Note added", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return n;
    }
    public async Task<CrmNote> EditNote(Guid id, EditNoteRequest req, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        var n = await Required(_notes, a.TenantId, id, ct);
        if (n.AuthorMemberId != a.MemberId && a.Role is not (MemberRole.Owner or MemberRole.Admin))
            throw new ForbiddenException("Only the author or an administrator may edit a note.");
        n.Edit(req.Body);
        await Audit(a, "edit-note", nameof(CrmNote), n.Id, "Note edited", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return n;
    }
    public async Task<CrmTag> CreateTag(CreateTagRequest req, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct, MemberRole.Owner, MemberRole.Admin, MemberRole.Manager);
        var tag = new CrmTag(a.TenantId, req.Name, req.Color);
        if (await _tags.AnyAsync(a.TenantId, x => x.Name == tag.Name, ct)) throw new ConflictException("Tag name already exists.");
        await _tags.AddAsync(tag, ct);
        await Audit(a, "create-tag", nameof(CrmTag), tag.Id, "Tag created", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return tag;
    }
    public async Task<CrmRecordTag> AssignTag(AssignTagRequest req, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        await Required(_tags, a.TenantId, req.TagId, ct);
        await _records.EnsureExists(a.TenantId, req.RecordKind, req.RecordId, ct);
        var match = await _assigned.PageAsync(a.TenantId, new PageRequest(1, 1),
            x => x.TagId == req.TagId && x.RecordKind == req.RecordKind && x.RecordId == req.RecordId, ct);
        var link = match.Items.FirstOrDefault();
        if (link is not null) link = await Required(_assigned, a.TenantId, link.Id, ct);
        if (link is null)
        {
            link = new CrmRecordTag(a.TenantId, req.TagId, req.RecordKind, req.RecordId);
            await _assigned.AddAsync(link, ct);
        }
        else link.Activate();
        await Audit(a, "assign-tag", nameof(CrmRecordTag), link.Id, "Tag assigned", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return link;
    }
    public async Task<PagedResult<CrmRecordTag>> TagsFor(CrmRecordKind kind, Guid recordId, PageRequest page, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        await _records.EnsureExists(a.TenantId, kind, recordId, ct);
        return await _assigned.PageAsync(a.TenantId, page, x => x.RecordKind == kind && x.RecordId == recordId && x.IsActive, ct);
    }
}

public sealed class EnterpriseCustomFieldService : EnterpriseServiceBase
{
    private readonly EnterpriseRecordGuard _records;
    private readonly ITenantRepository<CrmCustomFieldDefinition> _definitions;
    private readonly ITenantRepository<CrmCustomFieldValue> _values;
    public EnterpriseCustomFieldService(AccessGuard g, IUnitOfWork u, ITenantRepository<CrmAuditEvent> a,
        EnterpriseRecordGuard records, ITenantRepository<CrmCustomFieldDefinition> definitions,
        ITenantRepository<CrmCustomFieldValue> values) : base(g, u, a)
        => (_records, _definitions, _values) = (records, definitions, values);
    public async Task<CrmCustomFieldDefinition> Define(CreateCustomFieldRequest req, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct, MemberRole.Owner, MemberRole.Admin, MemberRole.Manager);
        var definition = new CrmCustomFieldDefinition(a.TenantId, req.RecordKind, req.Key, req.Label, req.Type);
        if (await _definitions.AnyAsync(a.TenantId, x => x.RecordKind == definition.RecordKind && x.Key == definition.Key, ct))
            throw new ConflictException("Custom field already defined.");
        await _definitions.AddAsync(definition, ct);
        await Audit(a, "define", nameof(CrmCustomFieldDefinition), definition.Id, "Custom field defined", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return definition;
    }
    public async Task<CrmCustomFieldValue> Set(SetCustomValueRequest req, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        var definition = await Required(_definitions, a.TenantId, req.FieldId, ct);
        if (!definition.IsActive) throw new ConflictException("Custom field is disabled.");
        await _records.EnsureExists(a.TenantId, definition.RecordKind, req.RecordId, ct);
        var page = await _values.PageAsync(a.TenantId, new PageRequest(1, 1), x => x.FieldId == req.FieldId && x.RecordId == req.RecordId, ct);
        var value = page.Items.FirstOrDefault();
        if (value is not null) value = await Required(_values, a.TenantId, value.Id, ct);
        if (value is null)
        {
            value = new CrmCustomFieldValue(a.TenantId, definition, req.RecordId, req.Value);
            await _values.AddAsync(value, ct);
        }
        else value.Change(definition.FieldType, req.Value);
        await Audit(a, "set", nameof(CrmCustomFieldValue), value.Id, "Custom field value updated", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return value;
    }
    public async Task<PagedResult<CrmCustomFieldValue>> Values(CrmRecordKind kind, Guid recordId, PageRequest page, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        await _records.EnsureExists(a.TenantId, kind, recordId, ct);
        return await _values.PageAsync(a.TenantId, page, x => x.RecordKind == kind && x.RecordId == recordId, ct);
    }
}

public sealed class EnterpriseGoalService : EnterpriseServiceBase
{
    private readonly ITenantRepository<SalesGoal> _goals;
    private readonly ITenantRepository<TenantMember> _members;
    public EnterpriseGoalService(AccessGuard g, IUnitOfWork u, ITenantRepository<CrmAuditEvent> a,
        ITenantRepository<SalesGoal> goals, ITenantRepository<TenantMember> members) : base(g, u, a)
        => (_goals, _members) = (goals, members);
    public async Task<PagedResult<SalesGoal>> List(PageRequest page, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        return await _goals.PageAsync(a.TenantId, page, x => x.MemberId == a.MemberId ||
            a.Role == MemberRole.Owner || a.Role == MemberRole.Admin || a.Role == MemberRole.Manager, ct);
    }
    public async Task<SalesGoal> Create(CreateSalesGoalRequest req, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct, MemberRole.Owner, MemberRole.Admin, MemberRole.Manager);
        await Required(_members, a.TenantId, req.MemberId, ct);
        var goal = new SalesGoal(a.TenantId, req.MemberId, req.Year, req.Month, req.Currency, req.TargetAmount);
        if (await _goals.AnyAsync(a.TenantId, x => x.MemberId == goal.MemberId && x.Year == goal.Year && x.Month == goal.Month && x.Currency == goal.Currency, ct))
            throw new ConflictException("Goal for this member, month and currency already exists.");
        await _goals.AddAsync(goal, ct);
        await Audit(a, "create", nameof(SalesGoal), goal.Id, "Monthly sales goal created", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return goal;
    }
    public async Task<SalesGoal> Update(Guid id, ChangeSalesGoalRequest req, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct, MemberRole.Owner, MemberRole.Admin, MemberRole.Manager);
        var goal = await Required(_goals, a.TenantId, id, ct);
        goal.Change(req.TargetAmount);
        await Audit(a, "update", nameof(SalesGoal), id, "Sales goal updated", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return goal;
    }
}

public sealed class EnterpriseAuditService
{
    private readonly AccessGuard _guard;
    private readonly ITenantRepository<CrmAuditEvent> _audit;
    public EnterpriseAuditService(AccessGuard guard, ITenantRepository<CrmAuditEvent> audit) => (_guard, _audit) = (guard, audit);
    public async Task<PagedResult<CrmAuditEvent>> List(PageRequest page, CancellationToken ct)
    {
        var a = await _guard.RequireAsync(ct, MemberRole.Owner, MemberRole.Admin, MemberRole.Manager);
        return await _audit.PageAsync(a.TenantId, page, null, ct);
    }
}

using System.Linq.Expressions;
using SalesFlow.Application.Abstractions;
using SalesFlow.Application.Common.Data;
using SalesFlow.Application.Common.Errors;
using SalesFlow.Application.Common.Pagination;
using SalesFlow.Application.Common.Validation;
using SalesFlow.Application.Mapping;
using SalesFlow.Application.Security;
using SalesFlow.Domain.Entities;
using SalesFlow.Domain.Enums;

namespace SalesFlow.Application.Features.Tasks;

public sealed class TaskService
{
    private readonly AccessGuard _guard;
    private readonly ITenantRepository<SalesTask> _tasks;
    private readonly ITenantMemberRepository _members;
    private readonly IUnitOfWork _unitOfWork;

    public TaskService(AccessGuard guard, ITenantRepository<SalesTask> tasks, ITenantMemberRepository members, IUnitOfWork unitOfWork)
    {
        _guard = guard;
        _tasks = tasks;
        _members = members;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<TaskDto>> ListAsync(PageRequest page, SalesTaskStatus? status = null, Guid? memberId = null, CancellationToken cancellationToken = default)
    {
        if (status.HasValue) RequestRules.ValidEnum(status.Value, nameof(status));
        if (memberId.HasValue) RequestRules.RequiredId(memberId.Value, nameof(memberId));
        var access = await _guard.RequireAsync(cancellationToken);
        Expression<Func<SalesTask, bool>> filter = x => (!status.HasValue || x.Status == status.Value) && (!memberId.HasValue || x.AssignedMemberId == memberId.Value);
        var result = await _tasks.PageOwnedAsync(access.TenantId, page, filter, cancellationToken);
        return result.Map(x => x.ToDto());
    }

    public async Task<TaskDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var access = await _guard.RequireAsync(cancellationToken);
        return (await _tasks.GetRequiredAsync(access.TenantId, id, cancellationToken)).ToDto();
    }

    public async Task<TaskDto> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ValidationException("Request is required.");
        var access = await _guard.RequireAsync(cancellationToken);
        if (request.AssignedMemberId.HasValue) await EnsureActiveMemberAsync(access.TenantId, request.AssignedMemberId.Value, cancellationToken);
        var task = new SalesTask(access.TenantId, request.Title, request.AssignedMemberId, request.DueAt);
        await _tasks.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return task.ToDto();
    }

    public async Task<TaskDto> AssignAsync(Guid id, AssignTaskRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ValidationException("Request is required.");
        var access = await _guard.RequireAsync(cancellationToken, MemberRole.Owner, MemberRole.Admin, MemberRole.Manager);
        var task = await _tasks.GetRequiredAsync(access.TenantId, id, cancellationToken);
        await EnsureActiveMemberAsync(access.TenantId, request.MemberId, cancellationToken);
        task.AssignTo(request.MemberId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return task.ToDto();
    }

    public async Task<TaskDto> SetDueDateAsync(Guid id, SetTaskDueDateRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ValidationException("Request is required.");
        var access = await _guard.RequireAsync(cancellationToken);
        var task = await _tasks.GetRequiredAsync(access.TenantId, id, cancellationToken);
        task.SetDueDate(request.DueAt);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return task.ToDto();
    }

    public async Task<TaskDto> CompleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var access = await _guard.RequireAsync(cancellationToken);
        var task = await _tasks.GetRequiredAsync(access.TenantId, id, cancellationToken);
        if (access.Role == MemberRole.Sales && task.AssignedMemberId != access.MemberId) throw new ForbiddenException("Sales users can only complete their assigned tasks.");
        task.Complete();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return task.ToDto();
    }

    private async Task EnsureActiveMemberAsync(Guid tenantId, Guid memberId, CancellationToken cancellationToken)
    {
        var member = await _members.GetRequiredAsync(tenantId, memberId, cancellationToken);
        if (!member.IsActive) throw new ConflictException("Task cannot be assigned to an inactive member.");
    }
}

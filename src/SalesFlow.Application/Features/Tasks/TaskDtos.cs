using SalesFlow.Domain.Enums;

namespace SalesFlow.Application.Features.Tasks;

public sealed record CreateTaskRequest(string Title, Guid? AssignedMemberId = null, DateTime? DueAt = null);
public sealed record AssignTaskRequest(Guid MemberId);
public sealed record SetTaskDueDateRequest(DateTime? DueAt);
public sealed record TaskDto(Guid Id, Guid TenantId, string Title, Guid? AssignedMemberId, DateTime? DueAt, SalesTaskStatus Status, DateTime? CompletedAt, DateTime CreatedAt, DateTime? UpdatedAt);

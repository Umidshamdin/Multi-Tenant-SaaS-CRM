using SalesFlow.Domain.Enums;

namespace SalesFlow.Application.Features.Activities;
public sealed record CreateActivityRequest(
    Guid CustomerId,
    Guid? DealId,
    ActivityKind Kind,
    string Subject,
    string? Description,
    DateTime? OccurredAtUtc
);
public sealed record ActivityDto(
    Guid Id,
    Guid TenantId,
    Guid CustomerId,
    Guid? DealId,
    Guid CreatedByMemberId,
    ActivityKind Kind,
    string Subject,
    string? Description,
    DateTime OccurredAtUtc,
    DateTime CreatedAt
);

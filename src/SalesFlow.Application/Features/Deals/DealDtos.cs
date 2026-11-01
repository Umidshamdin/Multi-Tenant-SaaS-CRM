using SalesFlow.Domain.Enums;

namespace SalesFlow.Application.Features.Deals;
public sealed record CreateDealRequest(Guid CustomerId, string Title, decimal Amount, string Currency = "USD");
public sealed record UpdateDealTitleRequest(string Title);
public sealed record ChangeDealAmountRequest(decimal Amount);
public sealed record DealDto(Guid Id, Guid TenantId, Guid CustomerId, string Title, decimal Amount, string Currency, DealStage Stage, DateTime? ClosedAt, DateTime CreatedAt, DateTime? UpdatedAt);

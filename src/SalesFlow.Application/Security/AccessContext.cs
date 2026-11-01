using SalesFlow.Domain.Enums;

namespace SalesFlow.Application.Security;

public sealed record AccessContext(Guid TenantId, string UserId, Guid MemberId, MemberRole Role);

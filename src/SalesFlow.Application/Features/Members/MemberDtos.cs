using SalesFlow.Domain.Enums;

namespace SalesFlow.Application.Features.Members;

public sealed record AddMemberRequest(string IdentityUserId, MemberRole Role);
public sealed record ChangeMemberRoleRequest(MemberRole Role);
public sealed record MemberDto(Guid Id, Guid TenantId, string IdentityUserId, MemberRole Role, bool IsActive, DateTime CreatedAt);

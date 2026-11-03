using SalesFlow.Domain.Enums;

namespace SalesFlow.Api.Models;

public sealed record RegisterRequest(string Email, string Password, string DisplayName);
public sealed record LoginRequest(string Email, string Password);
public sealed record SelectTenantRequest(Guid TenantId);
public sealed record UserProfileDto(string UserId, string Email, string DisplayName, Guid? ActiveTenantId);
public sealed record WorkspaceDto(Guid TenantId, string Name, string Slug, MemberRole Role);
public sealed record SetActiveRequest(bool Active);

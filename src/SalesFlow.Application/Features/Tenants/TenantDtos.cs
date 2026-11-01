using SalesFlow.Domain.Enums;

namespace SalesFlow.Application.Features.Tenants;
public sealed record CreateTenantRequest(string Name, string Slug, string LegalName, string? CountryCode = null, string? WebsiteUrl = null);
public sealed record TenantDto(Guid Id, string Name, string Slug, TenantStatus Status, DateTime CreatedAt);
public sealed record TenantCreatedDto(TenantDto Tenant, Guid OrganizationId, Guid OwnerMemberId);
public sealed record RenameTenantRequest(string Name);
public sealed record UpdateOrganizationRequest(string LegalName, string? CountryCode, string? WebsiteUrl);
public sealed record OrganizationDto(Guid Id, Guid TenantId, string LegalName, string? CountryCode, string? WebsiteUrl, DateTime CreatedAt, DateTime? UpdatedAt);

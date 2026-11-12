using SalesFlow.Application.Features.Customers;
using SalesFlow.Application.Features.Deals;
using SalesFlow.Application.Features.Leads;
using SalesFlow.Application.Features.Members;
using SalesFlow.Application.Features.Products;
using SalesFlow.Application.Features.Tasks;
using SalesFlow.Application.Features.Tenants;
using SalesFlow.Domain.Entities;

namespace SalesFlow.Application.Mapping;

public static class EntityMappings
{
    public static TenantDto ToDto(this Tenant x) => new(x.Id, x.Name, x.Slug, x.Status, x.CreatedAt);
    public static OrganizationDto ToDto(this Organization x) => new(x.Id, x.TenantId, x.LegalName, x.CountryCode, x.WebsiteUrl, x.CreatedAt, x.UpdatedAt);
    public static MemberDto ToDto(this TenantMember x) => new(x.Id, x.TenantId, x.IdentityUserId, x.Role, x.IsActive, x.CreatedAt);
    public static CustomerDto ToDto(this Customer x) => new(x.Id, x.TenantId, x.Name, x.Email, x.Phone, x.IsActive, x.CreatedAt, x.UpdatedAt);
    public static ContactDto ToDto(this CustomerContact x) => new(x.Id, x.TenantId, x.CustomerId, x.FullName, x.Email, x.Phone, x.CreatedAt);
    public static LeadDto ToDto(this Lead x) => new(x.Id, x.TenantId, x.FullName, x.Email, x.CompanyName, x.Status, x.ConvertedCustomerId, x.CreatedAt, x.UpdatedAt);
    public static DealDto ToDto(this Deal x) => new(x.Id, x.TenantId, x.CustomerId, x.Title, x.Amount, x.Currency, x.Stage, x.ClosedAt, x.CreatedAt, x.UpdatedAt);
    public static TaskDto ToDto(this SalesTask x) => new(x.Id, x.TenantId, x.Title, x.AssignedMemberId, x.DueAt, x.Status, x.CompletedAt, x.CreatedAt, x.UpdatedAt);
    public static ProductDto ToDto(this Product x) => new(x.Id, x.TenantId, x.Sku, x.Name, x.Description, x.UnitPrice, x.Currency, x.IsActive, x.CreatedAt, x.UpdatedAt);
}
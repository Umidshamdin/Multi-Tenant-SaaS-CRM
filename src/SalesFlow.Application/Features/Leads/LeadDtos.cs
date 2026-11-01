using SalesFlow.Domain.Enums;

namespace SalesFlow.Application.Features.Leads;
public sealed record CreateLeadRequest(string FullName, string? Email = null, string? CompanyName = null);
public sealed record LeadDto(Guid Id, Guid TenantId, string FullName, string? Email, string? CompanyName, LeadStatus Status, Guid? ConvertedCustomerId, DateTime CreatedAt, DateTime? UpdatedAt);
public sealed record ConvertLeadResult(LeadDto Lead, Guid CustomerId);

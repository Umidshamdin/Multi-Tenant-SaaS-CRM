namespace SalesFlow.Application.Features.Customers;
public sealed record CreateCustomerRequest(string Name, string? Email = null, string? Phone = null);
public sealed record UpdateCustomerRequest(string Name, string? Email, string? Phone);
public sealed record CustomerDto(Guid Id, Guid TenantId, string Name, string? Email, string? Phone, bool IsActive, DateTime CreatedAt, DateTime? UpdatedAt);
public sealed record CreateContactRequest(Guid CustomerId, string FullName, string? Email = null, string? Phone = null);
public sealed record UpdateContactRequest(string FullName, string? Email, string? Phone);
public sealed record ContactDto(Guid Id, Guid TenantId, Guid CustomerId, string FullName, string? Email, string? Phone, DateTime CreatedAt);

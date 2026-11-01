using System.Linq.Expressions;
using SalesFlow.Application.Abstractions;
using SalesFlow.Application.Common.Data;
using SalesFlow.Application.Common.Errors;
using SalesFlow.Application.Common.Pagination;
using SalesFlow.Application.Mapping;
using SalesFlow.Application.Security;
using SalesFlow.Domain.Entities;
using SalesFlow.Domain.Enums;

namespace SalesFlow.Application.Features.Customers;

public sealed class CustomerService
{
    private readonly AccessGuard _guard;
    private readonly ITenantRepository<Customer> _customers;
    private readonly IUnitOfWork _unitOfWork;

    public CustomerService(AccessGuard guard, ITenantRepository<Customer> customers, IUnitOfWork unitOfWork)
    {
        _guard = guard;
        _customers = customers;
        _unitOfWork = unitOfWork;
    }
    public async Task<PagedResult<CustomerDto>> ListAsync(PageRequest page, string? search = null, bool? active = null, CancellationToken cancellationToken = default)
    {
        var access = await _guard.RequireAsync(cancellationToken);
        var term = search?.Trim();
        Expression<Func<Customer, bool>> filter = x => (string.IsNullOrEmpty(term) || x.Name.Contains(term)) && (!active.HasValue || x.IsActive == active.Value);
        var result = await _customers.PageOwnedAsync(access.TenantId, page, filter, cancellationToken);
        return result.Map(x => x.ToDto());
    }

    public async Task<CustomerDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var access = await _guard.RequireAsync(cancellationToken);
        return (await _customers.GetRequiredAsync(access.TenantId, id, cancellationToken)).ToDto();
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ValidationException("Request is required.");
        var access = await _guard.RequireAsync(cancellationToken);
        var customer = new Customer(access.TenantId, request.Name, request.Email, request.Phone);
        await _customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return customer.ToDto();
    }

    public async Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ValidationException("Request is required.");
        var access = await _guard.RequireAsync(cancellationToken);
        var customer = await _customers.GetRequiredAsync(access.TenantId, id, cancellationToken);
        if (!customer.IsActive) throw new ConflictException("Inactive customer cannot be updated.");
        customer.UpdateDetails(request.Name, request.Email, request.Phone);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return customer.ToDto();
    }

    public async Task<CustomerDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var access = await _guard.RequireAsync(cancellationToken, MemberRole.Owner, MemberRole.Admin, MemberRole.Manager);
        var customer = await _customers.GetRequiredAsync(access.TenantId, id, cancellationToken);
        customer.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return customer.ToDto();
    }
}

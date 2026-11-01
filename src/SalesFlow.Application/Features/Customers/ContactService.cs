using System.Linq.Expressions;
using SalesFlow.Application.Abstractions;
using SalesFlow.Application.Common.Data;
using SalesFlow.Application.Common.Errors;
using SalesFlow.Application.Common.Pagination;
using SalesFlow.Application.Mapping;
using SalesFlow.Application.Security;
using SalesFlow.Domain.Entities;

namespace SalesFlow.Application.Features.Customers;

public sealed class ContactService
{
    private readonly AccessGuard _guard;
    private readonly ITenantRepository<Customer> _customers;
    private readonly ITenantRepository<CustomerContact> _contacts;
    private readonly IUnitOfWork _unitOfWork;

    public ContactService(AccessGuard guard, ITenantRepository<Customer> customers, ITenantRepository<CustomerContact> contacts, IUnitOfWork unitOfWork)
    {
        _guard = guard;
        _customers = customers;
        _contacts = contacts;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ContactDto>> ListAsync(Guid customerId, PageRequest page, CancellationToken cancellationToken = default)
    {
        var access = await _guard.RequireAsync(cancellationToken);
        await _customers.GetRequiredAsync(access.TenantId, customerId, cancellationToken);
        Expression<Func<CustomerContact, bool>> filter = x => x.CustomerId == customerId;
        var result = await _contacts.PageOwnedAsync(access.TenantId, page, filter, cancellationToken);
        return result.Map(x => x.ToDto());
    }

    public async Task<ContactDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var access = await _guard.RequireAsync(cancellationToken);
        return (await _contacts.GetRequiredAsync(access.TenantId, id, cancellationToken)).ToDto();
    }

    public async Task<ContactDto> CreateAsync(CreateContactRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ValidationException("Request is required.");
        var access = await _guard.RequireAsync(cancellationToken);
        var customer = await _customers.GetRequiredAsync(access.TenantId, request.CustomerId, cancellationToken);
        if (!customer.IsActive) throw new ConflictException("Cannot add contact to an inactive customer.");
        var contact = new CustomerContact(access.TenantId, customer.Id, request.FullName, request.Email, request.Phone);
        await _contacts.AddAsync(contact, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return contact.ToDto();
    }

    public async Task<ContactDto> UpdateAsync(Guid id, UpdateContactRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ValidationException("Request is required.");
        var access = await _guard.RequireAsync(cancellationToken);
        var contact = await _contacts.GetRequiredAsync(access.TenantId, id, cancellationToken);
        var customer = await _customers.GetRequiredAsync(access.TenantId, contact.CustomerId, cancellationToken);
        if (!customer.IsActive) throw new ConflictException("Cannot edit contact for an inactive customer.");
        contact.UpdateDetails(request.FullName, request.Email, request.Phone);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return contact.ToDto();
    }
}

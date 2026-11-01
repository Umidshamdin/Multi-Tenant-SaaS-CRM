using System.Linq.Expressions;
using SalesFlow.Application.Abstractions;
using SalesFlow.Application.Common.Data;
using SalesFlow.Application.Common.Errors;
using SalesFlow.Application.Common.Pagination;
using SalesFlow.Application.Common.Validation;
using SalesFlow.Application.Mapping;
using SalesFlow.Application.Security;
using SalesFlow.Domain.Entities;
using SalesFlow.Domain.Enums;

namespace SalesFlow.Application.Features.Leads;

public sealed class LeadService
{
    private readonly AccessGuard _guard;
    private readonly ITenantRepository<Lead> _leads;
    private readonly ITenantRepository<Customer> _customers;
    private readonly IUnitOfWork _unitOfWork;

    public LeadService(AccessGuard guard, ITenantRepository<Lead> leads, ITenantRepository<Customer> customers, IUnitOfWork unitOfWork)
    {
        _guard = guard;
        _leads = leads;
        _customers = customers;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<LeadDto>> ListAsync(PageRequest page, LeadStatus? status = null, CancellationToken cancellationToken = default)
    {
        if (status.HasValue) RequestRules.ValidEnum(status.Value, nameof(status));
        var access = await _guard.RequireAsync(cancellationToken);
        Expression<Func<Lead, bool>> filter = x => !status.HasValue || x.Status == status.Value;
        var result = await _leads.PageOwnedAsync(access.TenantId, page, filter, cancellationToken);
        return result.Map(x => x.ToDto());
    }

    public async Task<LeadDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var access = await _guard.RequireAsync(cancellationToken);
        return (await _leads.GetRequiredAsync(access.TenantId, id, cancellationToken)).ToDto();
    }

    public async Task<LeadDto> CreateAsync(CreateLeadRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ValidationException("Request is required.");
        var access = await _guard.RequireAsync(cancellationToken);
        var lead = new Lead(access.TenantId, request.FullName, request.Email, request.CompanyName);
        await _leads.AddAsync(lead, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return lead.ToDto();
    }

    public Task<LeadDto> MarkContactedAsync(Guid id, CancellationToken cancellationToken = default) => ChangeAsync(id, x => x.MarkContacted(), cancellationToken);
    public Task<LeadDto> QualifyAsync(Guid id, CancellationToken cancellationToken = default) => ChangeAsync(id, x => x.Qualify(), cancellationToken);
    public Task<LeadDto> MarkLostAsync(Guid id, CancellationToken cancellationToken = default) => ChangeAsync(id, x => x.MarkLost(), cancellationToken);

    public async Task<ConvertLeadResult> ConvertAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var access = await _guard.RequireAsync(cancellationToken);
        var lead = await _leads.GetRequiredAsync(access.TenantId, id, cancellationToken);
        if (lead.Status != LeadStatus.Qualified) throw new ConflictException("Lead must be qualified before conversion.");

        var customerName = string.IsNullOrWhiteSpace(lead.CompanyName) ? lead.FullName : lead.CompanyName;
        var customer = new Customer(access.TenantId, customerName, lead.Email);
        lead.ConvertToCustomer(customer.Id);
        await _customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new ConvertLeadResult(lead.ToDto(), customer.Id);
    }

    private async Task<LeadDto> ChangeAsync(Guid id, Action<Lead> change, CancellationToken cancellationToken)
    {
        var access = await _guard.RequireAsync(cancellationToken);
        var lead = await _leads.GetRequiredAsync(access.TenantId, id, cancellationToken);
        change(lead);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return lead.ToDto();
    }
}

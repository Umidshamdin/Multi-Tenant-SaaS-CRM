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

namespace SalesFlow.Application.Features.Deals;

public sealed class DealService
{
    private readonly AccessGuard _guard;
    private readonly ITenantRepository<Deal> _deals;
    private readonly ITenantRepository<Customer> _customers;
    private readonly IUnitOfWork _unitOfWork;

    public DealService(AccessGuard guard, ITenantRepository<Deal> deals, ITenantRepository<Customer> customers, IUnitOfWork unitOfWork)
    {
        _guard = guard;
        _deals = deals;
        _customers = customers;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<DealDto>> ListAsync(PageRequest page, Guid? customerId = null, DealStage? stage = null, CancellationToken cancellationToken = default)
    {
        if (customerId.HasValue) RequestRules.RequiredId(customerId.Value, nameof(customerId));
        if (stage.HasValue) RequestRules.ValidEnum(stage.Value, nameof(stage));
        var access = await _guard.RequireAsync(cancellationToken);
        Expression<Func<Deal, bool>> filter = x => (!customerId.HasValue || x.CustomerId == customerId.Value) && (!stage.HasValue || x.Stage == stage.Value);
        var result = await _deals.PageOwnedAsync(access.TenantId, page, filter, cancellationToken);
        return result.Map(x => x.ToDto());
    }

    public async Task<DealDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var access = await _guard.RequireAsync(cancellationToken);
        return (await _deals.GetRequiredAsync(access.TenantId, id, cancellationToken)).ToDto();
    }

    public async Task<DealDto> CreateAsync(CreateDealRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ValidationException("Request is required.");
        var access = await _guard.RequireAsync(cancellationToken);
        var customer = await _customers.GetRequiredAsync(access.TenantId, request.CustomerId, cancellationToken);
        if (!customer.IsActive) throw new ConflictException("Cannot create a deal for an inactive customer.");
        var deal = new Deal(access.TenantId, customer.Id, request.Title, request.Amount, request.Currency);
        await _deals.AddAsync(deal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return deal.ToDto();
    }

    public Task<DealDto> UpdateTitleAsync(Guid id, UpdateDealTitleRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ValidationException("Request is required.");
        return ChangeAsync(id, x => x.UpdateTitle(request.Title), cancellationToken);
    }

    public Task<DealDto> ChangeAmountAsync(Guid id, ChangeDealAmountRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ValidationException("Request is required.");
        return ChangeAsync(id, x => x.ChangeAmount(request.Amount), cancellationToken);
    }

    public Task<DealDto> MoveAsync(Guid id, DealStage stage, CancellationToken cancellationToken = default)
    {
        RequestRules.ValidEnum(stage, nameof(stage));
        return ChangeAsync(id, x => x.MoveTo(stage), cancellationToken);
    }

    public Task<DealDto> MarkWonAsync(Guid id, CancellationToken cancellationToken = default) => ChangeAsync(id, x => x.MarkWon(), cancellationToken);
    public Task<DealDto> MarkLostAsync(Guid id, CancellationToken cancellationToken = default) => ChangeAsync(id, x => x.MarkLost(), cancellationToken);

    private async Task<DealDto> ChangeAsync(Guid id, Action<Deal> change, CancellationToken cancellationToken)
    {
        var access = await _guard.RequireAsync(cancellationToken);
        var deal = await _deals.GetRequiredAsync(access.TenantId, id, cancellationToken);
        change(deal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return deal.ToDto();
    }
}

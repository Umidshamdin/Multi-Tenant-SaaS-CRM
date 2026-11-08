using System.Linq.Expressions;
using SalesFlow.Application.Abstractions;
using SalesFlow.Application.Common.Data;
using SalesFlow.Application.Common.Errors;
using SalesFlow.Application.Common.Pagination;
using SalesFlow.Application.Security;
using SalesFlow.Domain.Entities;
using SalesFlow.Domain.Enums;

namespace SalesFlow.Application.Features.Activities;
public sealed class ActivityService
{
    private readonly AccessGuard _guard;
    private readonly ITenantRepository<SalesActivity> _activities;
    private readonly ITenantRepository<Customer> _customers;
    private readonly ITenantRepository<Deal> _deals;
    private readonly IUnitOfWork _unitOfWork;

    public ActivityService(
        AccessGuard guard,
        ITenantRepository<SalesActivity> activities,
        ITenantRepository<Customer> customers,
        ITenantRepository<Deal> deals,
        IUnitOfWork unitOfWork)
    {
        _guard = guard;
        _activities = activities;
        _customers = customers;
        _deals = deals;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ActivityDto>> ListAsync(
        PageRequest page,
        Guid? customerId = null,
        Guid? dealId = null,
        ActivityKind? kind = null,
        CancellationToken cancellationToken = default)
    {
        if (customerId == Guid.Empty || dealId == Guid.Empty)
            throw new ValidationException("Invalid filter ID.");

        if (kind.HasValue && !Enum.IsDefined(kind.Value))
            throw new ValidationException("Invalid activity kind.");

        var access = await _guard.RequireAsync(cancellationToken);

        Expression<Func<SalesActivity, bool>> filter = x =>
            (!customerId.HasValue ||
             x.CustomerId == customerId.Value) &&
            (!dealId.HasValue ||
             x.DealId == dealId.Value) &&
            (!kind.HasValue ||
             x.Kind == kind.Value);

        var result = await _activities.PageOwnedAsync(
            access.TenantId,
            page,
            filter,
            cancellationToken);

        return result.Map(ToDto);
    }

    public async Task<ActivityDto> CreateAsync(
        CreateActivityRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ValidationException("Request is required.");

        var access = await _guard.RequireAsync(cancellationToken);

        var customer = await _customers.GetRequiredAsync(
            access.TenantId,
            request.CustomerId,
            cancellationToken);

        if (!customer.IsActive)
            throw new ConflictException(
                "Cannot add activity to an inactive customer.");

        if (request.DealId.HasValue)
        {
            var deal = await _deals.GetRequiredAsync(
                access.TenantId,
                request.DealId.Value,
                cancellationToken);

            if (deal.CustomerId != customer.Id)
                throw new ValidationException(
                    "Deal does not belong to this customer.");
        }

        var activity = new SalesActivity(
            access.TenantId,
            customer.Id,
            request.DealId,
            access.MemberId,
            request.Kind,
            request.Subject,
            request.Description,
            request.OccurredAtUtc ?? DateTime.UtcNow);

        await _activities.AddAsync(
            activity,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return ToDto(activity);
    }

    private static ActivityDto ToDto(SalesActivity item)
    {
        return new ActivityDto(
            item.Id,
            item.TenantId,
            item.CustomerId,
            item.DealId,
            item.CreatedByMemberId,
            item.Kind,
            item.Subject,
            item.Description,
            item.OccurredAtUtc,
            item.CreatedAt);
    }
}

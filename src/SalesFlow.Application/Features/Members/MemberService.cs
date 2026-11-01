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

namespace SalesFlow.Application.Features.Members;

public sealed class MemberService
{
    private readonly AccessGuard _guard;
    private readonly ITenantMemberRepository _members;
    private readonly IIdentityDirectory _identities;
    private readonly IUnitOfWork _unitOfWork;

    public MemberService(AccessGuard guard, ITenantMemberRepository members, IIdentityDirectory identities, IUnitOfWork unitOfWork)
    {
        _guard = guard;
        _members = members;
        _identities = identities;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<MemberDto>> ListAsync(PageRequest page, bool? active = null, CancellationToken cancellationToken = default)
    {
        var access = await _guard.RequireAsync(cancellationToken, MemberRole.Owner, MemberRole.Admin, MemberRole.Manager);
        Expression<Func<TenantMember, bool>> filter = x => !active.HasValue || x.IsActive == active.Value;
        var result = await _members.PageOwnedAsync(access.TenantId, page, filter, cancellationToken);
        return result.Map(x => x.ToDto());
    }

    public async Task<MemberDto> AddExistingUserAsync(AddMemberRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ValidationException("Request is required.");
        var access = await _guard.RequireAsync(cancellationToken, MemberRole.Owner, MemberRole.Admin);
        RequestRules.ValidEnum(request.Role, nameof(request.Role));
        if (request.Role == MemberRole.Owner || (access.Role == MemberRole.Admin && request.Role == MemberRole.Admin)) throw new ForbiddenException("You cannot assign this role.");
        var identityId = RequestRules.Required(request.IdentityUserId, nameof(request.IdentityUserId));
        if (!await _identities.ExistsAsync(identityId, cancellationToken)) throw new NotFoundException("Identity user not found.");
        if (await _members.GetByIdentityAsync(access.TenantId, identityId, cancellationToken) is not null) throw new ConflictException("User is already a member of this tenant.");

        var member = new TenantMember(access.TenantId, identityId, request.Role);
        await _members.AddAsync(member, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return member.ToDto();
    }

    public async Task<MemberDto> ChangeRoleAsync(Guid id, ChangeMemberRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ValidationException("Request is required.");
        var access = await _guard.RequireAsync(cancellationToken, MemberRole.Owner, MemberRole.Admin);
        RequestRules.ValidEnum(request.Role, nameof(request.Role));
        var member = await _members.GetRequiredAsync(access.TenantId, id, cancellationToken);
        if (member.Role == MemberRole.Owner || request.Role == MemberRole.Owner) throw new ForbiddenException("Owner role cannot be changed here.");
        if (access.Role == MemberRole.Admin && (member.Role == MemberRole.Admin || request.Role == MemberRole.Admin)) throw new ForbiddenException("Admins cannot manage other admins.");
        if (member.Id == access.MemberId) throw new ForbiddenException("You cannot change your own role.");
        member.ChangeRole(request.Role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return member.ToDto();
    }

    public async Task<MemberDto> SetActiveAsync(Guid id, bool active, CancellationToken cancellationToken = default)
    {
        var access = await _guard.RequireAsync(cancellationToken, MemberRole.Owner, MemberRole.Admin);
        var member = await _members.GetRequiredAsync(access.TenantId, id, cancellationToken);
        if (member.Role == MemberRole.Owner || member.Id == access.MemberId) throw new ForbiddenException("Cannot change owner or own membership status.");
        if (access.Role == MemberRole.Admin && member.Role == MemberRole.Admin) throw new ForbiddenException("Admins cannot manage other admins.");
        if (active) member.Reactivate(); else member.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return member.ToDto();
    }
}

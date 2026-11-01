using SalesFlow.Application.Abstractions;
using SalesFlow.Application.Common.Errors;
using SalesFlow.Application.Common.Validation;
using SalesFlow.Application.Mapping;
using SalesFlow.Domain.Entities;
using SalesFlow.Domain.Enums;

namespace SalesFlow.Application.Features.Tenants;
public sealed class TenantOnboardingService
{
    private readonly ICurrentUser _user;
    private readonly ITenantCatalog _tenants;
    private readonly ITenantRepository<Organization> _organizations;
    private readonly ITenantMemberRepository _members;
    private readonly IUnitOfWork _unitOfWork;

    public TenantOnboardingService(ICurrentUser user, ITenantCatalog tenants, ITenantRepository<Organization> organizations, ITenantMemberRepository members, IUnitOfWork unitOfWork)
    {
        _user = user;
        _tenants = tenants;
        _organizations = organizations;
        _members = members;
        _unitOfWork = unitOfWork;
    }

    public async Task<TenantCreatedDto> CreateAsync(CreateTenantRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _user.UserId;
        if (string.IsNullOrWhiteSpace(userId)) throw new UnauthorizedAccessException("Login is required to create a workspace.");
        if (request is null) throw new ValidationException("Request is required.");

        var tenant = new Tenant(request.Name, request.Slug);
        if (await _tenants.GetBySlugAsync(tenant.Slug, cancellationToken) is not null) throw new ConflictException("Tenant slug is already taken.");

        var organization = new Organization(tenant.Id, request.LegalName, request.CountryCode, request.WebsiteUrl);
        var owner = new TenantMember(tenant.Id, userId, MemberRole.Owner);
        await _tenants.AddAsync(tenant, cancellationToken);
        await _organizations.AddAsync(organization, cancellationToken);
        await _members.AddAsync(owner, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TenantCreatedDto(tenant.ToDto(), organization.Id, owner.Id);
    }
}

using SalesFlow.Application.Abstractions;
using SalesFlow.Application.Common.Data;
using SalesFlow.Application.Common.Errors;
using SalesFlow.Application.Common.Validation;
using SalesFlow.Application.Mapping;
using SalesFlow.Application.Security;
using SalesFlow.Domain.Entities;
using SalesFlow.Domain.Enums;

namespace SalesFlow.Application.Features.Tenants;
public sealed class TenantService
{
    private readonly AccessGuard _guard;
    private readonly ITenantCatalog _tenants;
    private readonly ITenantRepository<Organization> _organizations;
    private readonly IUnitOfWork _unitOfWork;

    public TenantService(AccessGuard guard, ITenantCatalog tenants, ITenantRepository<Organization> organizations, IUnitOfWork unitOfWork)
    {
        _guard = guard;
        _tenants = tenants;
        _organizations = organizations;
        _unitOfWork = unitOfWork;
    }

    public async Task<TenantDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var access = await _guard.RequireAsync(cancellationToken);
        var tenant = await _tenants.GetAsync(access.TenantId, cancellationToken);
        if (tenant is null) throw new NotFoundException("Tenant not found.");
        return tenant.ToDto();
    }

    public async Task<TenantDto> RenameAsync(RenameTenantRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ValidationException("Request is required.");
        var access = await _guard.RequireAsync(cancellationToken, MemberRole.Owner);
        var tenant = await _tenants.GetAsync(access.TenantId, cancellationToken);
        if (tenant is null) throw new NotFoundException("Tenant not found.");
        tenant.Rename(request.Name);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return tenant.ToDto();
    }

    public async Task<OrganizationDto> GetOrganizationAsync(CancellationToken cancellationToken = default)
    {
        var access = await _guard.RequireAsync(cancellationToken);
        var items = await _organizations.PageOwnedAsync(access.TenantId, new(1, 1), cancellationToken: cancellationToken);
        var organization = items.Items.FirstOrDefault();
        if (organization is null) throw new NotFoundException("Organization not found.");
        return organization.ToDto();
    }

    public async Task<OrganizationDto> UpdateOrganizationAsync(UpdateOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ValidationException("Request is required.");
        var access = await _guard.RequireAsync(cancellationToken, MemberRole.Owner, MemberRole.Admin);
        var items = await _organizations.PageOwnedAsync(access.TenantId, new(1, 1), cancellationToken: cancellationToken);
        var organization = items.Items.FirstOrDefault();
        if (organization is null) throw new NotFoundException("Organization not found.");
        organization.UpdateProfile(request.LegalName, request.CountryCode, request.WebsiteUrl);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return organization.ToDto();
    }
}

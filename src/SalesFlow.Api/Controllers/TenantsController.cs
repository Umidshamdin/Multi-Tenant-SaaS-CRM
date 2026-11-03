using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesFlow.Application.Features.Tenants;

namespace SalesFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tenants")]
public sealed class TenantsController : ControllerBase
{
    private readonly TenantOnboardingService _onboarding;
    private readonly TenantService _service;
    public TenantsController(TenantOnboardingService onboarding, TenantService service) { _onboarding = onboarding; _service = service; }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTenantRequest request, CancellationToken ct) => StatusCode(201, await _onboarding.CreateAsync(request, ct));

    [HttpGet("current")]
    public async Task<IActionResult> Current(CancellationToken ct) => Ok(await _service.GetAsync(ct));

    [HttpPut("current/name")]
    public async Task<IActionResult> Rename(RenameTenantRequest request, CancellationToken ct) => Ok(await _service.RenameAsync(request, ct));

    [HttpGet("current/organization")]
    public async Task<IActionResult> Organization(CancellationToken ct) => Ok(await _service.GetOrganizationAsync(ct));

    [HttpPut("current/organization")]
    public async Task<IActionResult> UpdateOrganization(UpdateOrganizationRequest request, CancellationToken ct) => Ok(await _service.UpdateOrganizationAsync(request, ct));
}

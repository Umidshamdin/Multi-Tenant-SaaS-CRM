using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SalesFlow.Api.Models;
using SalesFlow.Api.Security;
using SalesFlow.Domain.Enums;
using SalesFlow.Infrastructure.Identity;
using SalesFlow.Infrastructure.Persistence;

namespace SalesFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<SalesFlowUser> _users;
    private readonly SalesFlowDbContext _db;
    private readonly JwtTokenService _tokens;

    public AuthController(UserManager<SalesFlowUser> users, SalesFlowDbContext db, JwtTokenService tokens)
    {
        _users = users;
        _db = db;
        _tokens = tokens;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Email) || !new EmailAddressAttribute().IsValid(request.Email)) return BadRequest(new { message = "A valid email is required." });
        if (string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Trim().Length > 160) return BadRequest(new { message = "Display name must be between 1 and 160 characters." });
        if (string.IsNullOrWhiteSpace(request.Password)) return BadRequest(new { message = "Password is required." });

        var email = request.Email.Trim();
        if (await _users.FindByEmailAsync(email) is not null) return Conflict(new { message = "This email is already registered." });
        var user = new SalesFlowUser { UserName = email, Email = email, DisplayName = request.DisplayName.Trim() };
        var result = await _users.CreateAsync(user, request.Password);
        if (!result.Succeeded) return BadRequest(new { errors = result.Errors.Select(x => x.Description).ToArray() });
        return StatusCode(StatusCodes.Status201Created, _tokens.Create(user));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password)) return Unauthorized(new { message = "Invalid credentials." });
        var user = await _users.FindByEmailAsync(request.Email.Trim());
        if (user is null || await _users.IsLockedOutAsync(user)) return Unauthorized(new { message = "Invalid credentials." });
        if (!await _users.CheckPasswordAsync(user, request.Password))
        {
            await _users.AccessFailedAsync(user);
            return Unauthorized(new { message = "Invalid credentials." });
        }
        await _users.ResetAccessFailedCountAsync(user);
        return Ok(_tokens.Create(user));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = id is null ? null : await _users.FindByIdAsync(id);
        if (user is null) return Unauthorized();
        var active = Guid.TryParse(User.FindFirstValue("tenant_id"), out var tenantId) ? tenantId : (Guid?)null;
        return Ok(new UserProfileDto(user.Id, user.Email ?? string.Empty, user.DisplayName, active));
    }

    [HttpGet("workspaces")]
    [Authorize]
    public async Task<IActionResult> Workspaces(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        var result = await _db.TenantMembers.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.IdentityUserId == userId && x.IsActive)
            .Join(_db.Tenants.AsNoTracking().Where(x => x.Status == TenantStatus.Active), m => m.TenantId, t => t.Id,
                (m, t) => new WorkspaceDto(t.Id, t.Name, t.Slug, m.Role))
            .ToListAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("select-tenant")]
    [Authorize]
    public async Task<IActionResult> SelectTenant(SelectTenantRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        if (request is null || request.TenantId == Guid.Empty) return BadRequest(new { message = "A valid tenant ID is required." });
        var member = await _db.TenantMembers.IgnoreQueryFilters().AsNoTracking().AnyAsync(x => x.IdentityUserId == userId && x.TenantId == request.TenantId && x.IsActive, cancellationToken);
        var tenant = member ? await _db.Tenants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.TenantId && x.Status == TenantStatus.Active, cancellationToken) : null;
        if (tenant is null) return Forbid();
        var user = await _users.FindByIdAsync(userId);
        if (user is null) return Unauthorized();
        return Ok(_tokens.Create(user, tenant.Id));
    }

    [HttpPost("clear-tenant")]
    [Authorize]
    public async Task<IActionResult> ClearTenant()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = id is null ? null : await _users.FindByIdAsync(id);
        if (user is null) return Unauthorized();
        return Ok(_tokens.Create(user));
    }
}

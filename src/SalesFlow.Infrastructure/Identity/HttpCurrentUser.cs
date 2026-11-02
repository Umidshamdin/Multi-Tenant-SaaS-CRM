using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SalesFlow.Application.Abstractions;

namespace SalesFlow.Infrastructure.Identity;

public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public HttpCurrentUser(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;
    public string? UserId => Principal?.Identity?.IsAuthenticated == true ? Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Principal.FindFirst("sub")?.Value : null;
    public Guid? TenantId
    {
        get
        {
            if (Principal?.Identity?.IsAuthenticated != true) return null;
            var value = Principal.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(value, out var id) && id != Guid.Empty ? id : null;
        }
    }
}

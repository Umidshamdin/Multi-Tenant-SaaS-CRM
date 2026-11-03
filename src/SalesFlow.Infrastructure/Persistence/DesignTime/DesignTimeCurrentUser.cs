using SalesFlow.Application.Abstractions;

namespace SalesFlow.Infrastructure.Persistence.DesignTime;

internal sealed class DesignTimeCurrentUser : ICurrentUser
{
    public string? UserId => null;
    public Guid? TenantId => null;
}

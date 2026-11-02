using Microsoft.AspNetCore.Identity;

namespace SalesFlow.Infrastructure.Identity;

public sealed class SalesFlowUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
}

using Microsoft.EntityFrameworkCore;
using SalesFlow.Application.Abstractions;
using SalesFlow.Infrastructure.Persistence;

namespace SalesFlow.Infrastructure.Identity;

public sealed class IdentityDirectory : IIdentityDirectory
{
    private readonly SalesFlowDbContext _context;
    public IdentityDirectory(SalesFlowDbContext context) => _context = context;

    public Task<bool> ExistsAsync(string identityUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityUserId)) return Task.FromResult(false);
        return _context.Users.AsNoTracking().AnyAsync(x => x.Id == identityUserId, cancellationToken);
    }
}

namespace SalesFlow.Application.Abstractions;
public interface IIdentityDirectory
{
    Task<bool> ExistsAsync(string identityUserId, CancellationToken cancellationToken = default);
}

namespace SalesFlow.Application.Abstractions;
public interface ICurrentUser
{
    string? UserId { get; }
    Guid? TenantId { get; }
}

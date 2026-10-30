using SalesFlow.Domain.Exceptions;

namespace SalesFlow.Domain.Common;
public abstract class Entity
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected Entity() { }
    protected Entity(Guid id)
    {
        if (id == Guid.Empty) throw new DomainException("Entity ID cannot be empty.");
        Id = id;
        CreatedAt = DateTime.Now;
    }
    protected void Touch() => UpdatedAt = DateTime.Now;
}

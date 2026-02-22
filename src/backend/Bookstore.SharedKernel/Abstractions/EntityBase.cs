namespace Bookstore.SharedKernel.Abstractions;

public abstract class EntityBase
{
    public Guid Id { get; protected set; }
}

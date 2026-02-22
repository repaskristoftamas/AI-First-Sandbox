namespace Bookstore.SharedKernel.Abstractions;

public abstract class EntityBase<TId> where TId : notnull
{
    public TId Id { get; protected set; } = default!;
}

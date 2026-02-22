namespace Bookstore.SharedKernel.Abstractions;

public abstract class AuditableEntity<TId> : EntityBase<TId>, IAuditable where TId : notnull
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

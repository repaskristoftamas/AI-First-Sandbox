using Bookstore.Domain.Users.Events;
using Bookstore.SharedKernel.Abstractions;
using Bookstore.SharedKernel.Results;

namespace Bookstore.Domain.Users;

/// <summary>
/// Domain entity representing a user in the system.
/// </summary>
public sealed class User : AuditableEntity<UserId>, ISoftDeletable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <remarks>
    /// Required by EF Core for materialization.
    /// </remarks>
    private User() { }

    private readonly List<Role> _roles = [];

    /// <summary>
    /// Email address of the user.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Hashed password of the user.
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// Roles assigned to this user.
    /// </summary>
    public IReadOnlyCollection<Role> Roles => _roles;

    /// <inheritdoc />
    public bool IsDeleted { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Factory method that creates a new user with a generated identifier.
    /// </summary>
    /// <param name="email">Email address of the user.</param>
    /// <param name="passwordHash">Pre-hashed password for the user.</param>
    /// <param name="roles">Roles to assign to the user.</param>
    /// <returns>A successful result containing the new <see cref="User"/>, or a validation error.</returns>
    public static Result<User> Create(string email, string passwordHash, IReadOnlyCollection<Role> roles)
    {
        var validation = Validate(email, passwordHash, roles);
        if (validation.IsFailure)
            return Result.Failure<User>(validation.Error);

        var user = new User
        {
            Id = UserId.New(),
            Email = NormalizeEmail(email),
            PasswordHash = passwordHash
        };

        user._roles.AddRange(roles);

        user.AddDomainEvent(new UserCreatedEvent(user.Id));

        return Result.Success(user);
    }

    /// <summary>
    /// Updates the email address and roles of the user.
    /// </summary>
    /// <param name="email">New email address.</param>
    /// <param name="roles">New set of roles to assign.</param>
    /// <returns>A success result, or a validation error if any value is invalid.</returns>
    public Result Update(string email, IReadOnlyCollection<Role> roles)
    {
        var validation = ValidateEmailAndRoles(email, roles);
        if (validation.IsFailure)
            return validation;

        Email = NormalizeEmail(email);
        _roles.Clear();
        _roles.AddRange(roles);

        AddDomainEvent(new UserUpdatedEvent(Id));

        return Result.Success();
    }

    /// <summary>
    /// Updates the password hash of the user.
    /// </summary>
    /// <param name="passwordHash">New pre-hashed password.</param>
    /// <returns>A success result, or a validation error if the hash is empty.</returns>
    public Result UpdatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Failure(new ValidationError([new FieldValidationFailure(nameof(PasswordHash), UserErrorCodes.PasswordHashRequired, "Password hash is required.")]));

        PasswordHash = passwordHash;

        AddDomainEvent(new UserUpdatedEvent(Id));

        return Result.Success();
    }

    /// <summary>
    /// Marks this user as soft-deleted and raises the <see cref="UserDeletedEvent"/>.
    /// </summary>
    /// <param name="timeProvider">Provides the current time used to stamp <see cref="DeletedAt"/>.</param>
    public void Delete(TimeProvider timeProvider)
    {
        IsDeleted = true;
        DeletedAt = timeProvider.GetUtcNow();
        AddDomainEvent(new UserDeletedEvent(Id));
    }

    /// <summary>
    /// Normalizes an email address to lowercase for case-insensitive storage and comparison.
    /// </summary>
    private static string NormalizeEmail(string email) => email.ToLowerInvariant();

    /// <summary>
    /// Last-resort invariant guard that protects structural integrity regardless of entry point.
    /// </summary>
    /// <remarks>
    /// Primary validation is handled by FluentValidation at the application boundary.
    /// </remarks>
    private static Result Validate(string email, string passwordHash, IReadOnlyCollection<Role> roles)
    {
        var validation = ValidateEmailAndRoles(email, roles);
        if (validation.IsFailure)
            return validation;

        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Failure(new ValidationError([new FieldValidationFailure(nameof(PasswordHash), UserErrorCodes.PasswordHashRequired, "Password hash is required.")]));

        return Result.Success();
    }

    /// <summary>
    /// Validates email and roles fields shared by both <see cref="Create"/> and <see cref="Update"/>.
    /// </summary>
    private static Result ValidateEmailAndRoles(string email, IReadOnlyCollection<Role> roles)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure(new ValidationError([new FieldValidationFailure(nameof(Email), UserErrorCodes.EmailRequired, "Email is required.")]));

        if (roles.Count == 0)
            return Result.Failure(new ValidationError([new FieldValidationFailure(nameof(Roles), UserErrorCodes.RolesRequired, "At least one role is required.")]));

        return Result.Success();
    }
}

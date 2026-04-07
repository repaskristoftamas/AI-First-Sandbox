namespace Bookstore.Domain.Users;

/// <summary>
/// Represents the set of roles a user can be assigned within the system.
/// </summary>
public enum Role
{
    /// <summary>Standard user with default permissions.</summary>
    User,

    /// <summary>Administrator with elevated permissions.</summary>
    Admin
}

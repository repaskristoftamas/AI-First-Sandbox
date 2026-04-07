namespace Bookstore.Domain.Users;

/// <summary>
/// Machine-readable error codes for User domain operations.
/// </summary>
public static class UserErrorCodes
{
    /// <summary>The requested user does not exist.</summary>
    public const string NotFound = "USER_NOT_FOUND";

    /// <summary>A user with the given email already exists.</summary>
    public const string EmailConflict = "USER_EMAIL_CONFLICT";

    /// <summary>Email is missing or whitespace.</summary>
    public const string EmailRequired = "USER_EMAIL_REQUIRED";

    /// <summary>Email exceeds the maximum allowed length.</summary>
    public const string EmailTooLong = "USER_EMAIL_TOO_LONG";

    /// <summary>Email is not in a valid format.</summary>
    public const string EmailInvalidFormat = "USER_EMAIL_INVALID_FORMAT";

    /// <summary>Password hash is missing or whitespace.</summary>
    public const string PasswordHashRequired = "USER_PASSWORD_HASH_REQUIRED";

    /// <summary>No roles were assigned to the user.</summary>
    public const string RolesRequired = "USER_ROLES_REQUIRED";
}

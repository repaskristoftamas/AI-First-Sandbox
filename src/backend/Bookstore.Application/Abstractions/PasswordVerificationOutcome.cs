namespace Bookstore.Application.Abstractions;

/// <summary>
/// Outcome of verifying a plaintext password against a stored hash.
/// </summary>
public enum PasswordVerificationOutcome
{
    /// <summary>The password does not match the hash.</summary>
    Failed,

    /// <summary>The password matches the hash.</summary>
    Success,

    /// <summary>The password matches, but the hash was created with an older algorithm version and should be rehashed.</summary>
    SuccessRehashNeeded
}

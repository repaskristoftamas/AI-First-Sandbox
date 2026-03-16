namespace Bookstore.WebApi.Authorization;

/// <summary>
/// Defines the names of authorization policies used across the application.
/// </summary>
internal static class AuthorizationPolicies
{
    /// <summary>
    /// Policy that requires the user to have the Admin role.
    /// </summary>
    internal const string AdminOnly = "AdminOnly";
}

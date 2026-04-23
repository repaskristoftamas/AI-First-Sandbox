using Bookstore.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace Bookstore.Infrastructure.Identity;

/// <summary>
/// Password hasher that delegates to ASP.NET Core Identity's <see cref="PasswordHasher{TUser}"/>.
/// </summary>
internal sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _inner = new();

    /// <inheritdoc />
    public string Hash(string password) => _inner.HashPassword(null!, password);

    /// <inheritdoc />
    public PasswordVerificationOutcome Verify(string password, string passwordHash) =>
        _inner.VerifyHashedPassword(null!, passwordHash, password) switch
        {
            PasswordVerificationResult.Success => PasswordVerificationOutcome.Success,
            PasswordVerificationResult.SuccessRehashNeeded => PasswordVerificationOutcome.SuccessRehashNeeded,
            _ => PasswordVerificationOutcome.Failed
        };
}

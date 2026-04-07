namespace Bookstore.Application.Abstractions;

/// <summary>
/// Abstraction for password hashing operations, keeping the domain layer free from identity framework dependencies.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plaintext password using a secure algorithm.
    /// </summary>
    /// <param name="password">The plaintext password to hash.</param>
    /// <returns>The hashed password string.</returns>
    string Hash(string password);

    /// <summary>
    /// Verifies a plaintext password against a previously computed hash.
    /// </summary>
    /// <param name="password">The plaintext password to verify.</param>
    /// <param name="passwordHash">The stored password hash to compare against.</param>
    /// <returns><c>true</c> if the password matches the hash; otherwise, <c>false</c>.</returns>
    bool Verify(string password, string passwordHash);
}

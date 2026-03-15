using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Bookstore.WebApi.Tests.Helpers;

/// <summary>
/// Generates JWT tokens for integration testing with configurable claims and roles.
/// Uses the same signing key and issuer/audience as appsettings.Testing.json.
/// </summary>
internal static class JwtTokenHelper
{
    private const string SigningKey = "test-signing-key-that-is-at-least-32-characters-long-for-hmac-sha256";
    private const string Issuer = "bookstore-api";
    private const string Audience = "bookstore-api-clients";

    /// <summary>
    /// Creates a valid JWT token with the specified roles.
    /// </summary>
    /// <param name="roles">The role claims to include in the token.</param>
    /// <returns>A signed JWT token string.</returns>
    internal static string CreateToken(params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: TimeProvider.System.GetUtcNow().AddHours(1).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

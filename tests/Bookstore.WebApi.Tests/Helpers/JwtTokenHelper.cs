using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Bookstore.WebApi.Tests.Helpers;

/// <summary>
/// Generates JWT tokens for integration testing with configurable claims and roles.
/// Reads signing key, issuer, and audience from appsettings.Testing.json to stay in sync with the application.
/// </summary>
internal static class JwtTokenHelper
{
    /// <summary>
    /// Creates a valid JWT token with the specified roles.
    /// </summary>
    /// <param name="roles">The role claims to include in the token.</param>
    /// <returns>A signed JWT token string.</returns>
    internal static string CreateToken(params string[] roles)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Testing.json")
            .Build();

        var jwtSection = configuration.GetSection("Jwt");
        var signingKey = jwtSection["SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey is missing from appsettings.Testing.json.");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: TimeProvider.System.GetUtcNow().AddHours(1).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

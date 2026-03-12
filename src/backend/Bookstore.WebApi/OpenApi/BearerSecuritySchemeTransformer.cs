using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Bookstore.WebApi.OpenApi;

/// <summary>
/// Adds the JWT Bearer security scheme definition to the OpenAPI document.
/// </summary>
internal sealed class BearerSecuritySchemeTransformer(
    IAuthenticationSchemeProvider authenticationSchemeProvider,
    ILogger<BearerSecuritySchemeTransformer> logger) : IOpenApiDocumentTransformer
{
    /// <inheritdoc />
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (!authenticationSchemes.Any(scheme => scheme.Name == JwtBearerDefaults.AuthenticationScheme))
        {
            logger.LogWarning(
                "Authentication scheme '{Scheme}' is not registered. The Bearer security scheme will not be added to the OpenAPI document",
                JwtBearerDefaults.AuthenticationScheme);
            return;
        }

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };
    }
}

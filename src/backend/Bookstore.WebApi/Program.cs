using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Bookstore.Application;
using Bookstore.Infrastructure;
using Bookstore.WebApi.Endpoints;
using Bookstore.WebApi.Endpoints.Authors;
using Bookstore.WebApi.Endpoints.Books;
using Bookstore.WebApi.Authorization;
using Bookstore.WebApi.Extensions;
using Bookstore.WebApi.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddTransient<IEndpointDefinition, AuthorEndpoints>();
builder.Services.AddTransient<IEndpointDefinition, BookEndpoints>();

var signingKey = builder.Configuration["Jwt:SigningKey"];
if (string.IsNullOrWhiteSpace(signingKey))
    throw new InvalidOperationException("Jwt:SigningKey must be configured. Use environment variables or dotnet user-secrets.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSection = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.AdminOnly, policy => policy.RequireRole("Admin"));

//TODO: AllowedOrigins array should only contain https:// origins in production. The config doesn't enforce this — consider validating at startup.
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

if (allowedOrigins.Length == 0)
    throw new InvalidOperationException("Cors:AllowedOrigins must contain at least one origin. Use environment variables or dotnet user-secrets.");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        //TODO consider restricting methods to what you actually use (e.g., only allow GET, POST, PUT, DELETE) to reduce attack surface
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<AuthorizationSecurityTransformer>();
    options.AddOperationTransformer<RateLimitResponseTransformer>();
});

var rateLimitingConfig = builder.Configuration.GetSection("RateLimiting");
var anonymousPermitLimit = rateLimitingConfig.GetValue("Anonymous:PermitLimit", 10);
var anonymousWindowSeconds = rateLimitingConfig.GetValue("Anonymous:WindowInSeconds", 60);
var authenticatedPermitLimit = rateLimitingConfig.GetValue("Authenticated:PermitLimit", 100);
var authenticatedWindowSeconds = rateLimitingConfig.GetValue("Authenticated:WindowInSeconds", 60);

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;

        return isAuthenticated
            ? RateLimitPartition.GetFixedWindowLimiter(
                $"authenticated:{ipAddress}",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = authenticatedPermitLimit,
                    Window = TimeSpan.FromSeconds(authenticatedWindowSeconds),
                    QueueLimit = 0
                })
            : RateLimitPartition.GetFixedWindowLimiter(
                $"anonymous:{ipAddress}",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = anonymousPermitLimit,
                    Window = TimeSpan.FromSeconds(anonymousWindowSeconds),
                    QueueLimit = 0
                });
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        var response = context.HttpContext.Response;
        response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            response.Headers.RetryAfter =
                ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();
        }

        await response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too Many Requests",
                Detail = "Rate limit exceeded. Please try again later.",
                Type = "https://tools.ietf.org/html/rfc6585#section-4"
            },
            (JsonSerializerOptions?)null,
            "application/problem+json",
            cancellationToken);
    };
});

var app = builder.Build();

await app.Services.MigrateDatabaseAsync(app.Lifetime.ApplicationStopping);

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
        });
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapGet("/scalar", () => Results.Content("""
        <!doctype html>
        <html>
        <head>
            <title>Bookstore API</title>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1" />
        </head>
        <body>
            <script id="api-reference" data-url="/openapi/v1.json"></script>
            <script src="https://cdn.jsdelivr.net/npm/@scalar/api-reference"></script>
        </body>
        </html>
        """, "text/html")).ExcludeFromDescription();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.RegisterEndpointDefinitions();
//TODO: Logger, Serilog or ILogger

app.Run();

public partial class Program { }

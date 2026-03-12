using System.Text;
using Bookstore.Application;
using Bookstore.Infrastructure;
using Bookstore.WebApi.Endpoints;
using Bookstore.WebApi.Endpoints.Authors;
using Bookstore.WebApi.Endpoints.Books;
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
builder.Services.AddAuthorizationBuilder();

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
});

//TODO: builder.Services.AddRateLimiter

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

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.RegisterEndpointDefinitions();
//TODO: app.UseRateLimiter
//TODO: Logger, Serilog or ILogger

app.Run();

public partial class Program { }

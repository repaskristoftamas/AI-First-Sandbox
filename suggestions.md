# ASP.NET Core Minimal API Abstraction — Best Practices & Meta Design Guide

> A comprehensive reference for architects and senior developers on how to design a production-grade ASP.NET Core Minimal API system. Not a CRUD tutorial — a *meta* guide about the abstractions, patterns, and structural decisions that real-world projects use.

---

## Table of Contents

1. [The Abstraction Spectrum](#1-the-abstraction-spectrum)
2. [Result Pattern — First-Class Error Handling](#2-result-pattern--first-class-error-handling)
3. [CQRS and MediatR — Command/Query Separation](#3-cqrs-and-mediatr--commandquery-separation)
4. [Entity Abstractions — What To Standardize](#4-entity-abstractions--what-to-standardize)
5. [Endpoint Abstractions — IEndpointDefinition and Beyond](#5-endpoint-abstractions--iendpointdefinition-and-beyond)
6. [Request / Response / DTO Separation](#6-request--response--dto-separation)
7. [Validation Abstractions](#7-validation-abstractions)
8. [Authentication & Authorization](#8-authentication--authorization)
9. [Global Exception Handling](#9-global-exception-handling)
10. [Structured Logging & Observability](#10-structured-logging--observability)
11. [Configuration — Options Pattern](#11-configuration--options-pattern)
12. [API Versioning](#12-api-versioning)
13. [Idempotency](#13-idempotency)
14. [Pagination Abstractions](#14-pagination-abstractions)
15. [Domain Events and Outbox Pattern](#15-domain-events-and-outbox-pattern)
16. [Cross-Cutting Concerns via Pipeline Behaviors](#16-cross-cutting-concerns-via-pipeline-behaviors)
17. [Folder & Project Structure Deep Dive](#17-folder--project-structure-deep-dive)
18. [When NOT To Abstract](#18-when-not-to-abstract)
19. [Summary Checklist](#19-summary-checklist)

---

## 1. The Abstraction Spectrum

The central question in any API project is: *how much abstraction is the right amount?*

```
LESS ABSTRACTION                                    MORE ABSTRACTION
|---------------------------------------------------|
Controllers → Minimal API → CQRS → DDD + CQRS + SharedKernel → Ports & Adapters
```

| Level | Description | Good For |
|---|---|---|
| **Minimal** | Direct CRUD in `Program.cs` | Tiny services, demos, scripts |
| **Grouped endpoints** | `RouteGroupBuilder`, one file per resource | Small-to-medium APIs |
| **CQRS with handlers** | Commands/Queries dispatched via MediatR | Medium-to-large APIs |
| **Full Clean Architecture** | Domain + Application + Infrastructure layers | Enterprise, team projects |
| **DDD + CQRS + Event Sourcing** | Aggregates, domain events, event store | Complex business domains |

**Rule of thumb:** Abstract to the level your *current team* can maintain. Abstraction has a cost — it's paid in indirection, onboarding time, and cognitive load. Every layer added must justify its existence with a clear separation of concern.

---

## 2. Result Pattern — First-Class Error Handling

### The Problem With Exceptions

Using exceptions for expected failure cases (`NotFound`, `Conflict`, `Validation error`) is expensive, hard to compose, and leaks implementation details to the caller.

### Typed Error Hierarchy

Use an abstract `Error` base with typed subclasses. The error *type* carries the semantic meaning — no string codes needed.

```csharp
public abstract record Error(string Description);

public sealed record NotFoundError(string Description) : Error(Description);
public sealed record ConflictError(string Description) : Error(Description);
public sealed record ValidationError(string Description) : Error(Description);
```

### The Result Type

`Result` wraps success/failure. `Error` and `Value` throw on invalid access — symmetric guards prevent misuse.

```csharp
public class Result
{
    private readonly Error? _error;

    protected Result(bool isSuccess, Error? error) { ... }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("The error of a success result cannot be accessed.");

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, null);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

public class Result<TValue> : Result
{
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result cannot be accessed.");
}
```

### Usage in Handlers

```csharp
if (book is null)
    return Result.Failure(new NotFoundError("The book with the specified identifier was not found."));

if (isbnExists)
    return Result.Failure<Guid>(new ConflictError($"A book with ISBN '{request.ISBN}' already exists."));
```

### Mapping Errors to HTTP — Extension Method

Centralize error-to-HTTP mapping in one extension method. Endpoints stay `Task<IResult>` (type-safe, Swagger-friendly). The error type maps to the status code via pattern matching.

```csharp
public static class ErrorExtensions
{
    public static IResult ToProblemResult(this Error error) => error switch
    {
        NotFoundError e   => Results.Problem(statusCode: 404, title: "NotFound", detail: e.Description),
        ConflictError e   => Results.Problem(statusCode: 409, title: "Conflict", detail: e.Description),
        ValidationError e => Results.Problem(statusCode: 400, title: "ValidationError", detail: e.Description),
        _                 => Results.Problem(statusCode: 500, title: "InternalError", detail: error.Description)
    };
}
```

Endpoint usage:

```csharp
private static async Task<IResult> GetBookById(Guid id, ISender sender, CancellationToken ct)
{
    var result = await sender.Send(new GetBookByIdQuery(id), ct);
    return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblemResult();
}
```

All error responses use RFC 7807 Problem Details — consistent, machine-readable, standard.

### Why Not Other Approaches?

| Approach | Problem |
|---|---|
| Flat `Error(string Code, string Description)` | String-based matching (`Code.StartsWith("NotFound")`), no type safety |
| Per-entity error classes (`BookErrors.NotFound`) | Boilerplate that scales linearly with entities |
| `Error.None` sentinel object | Redundant when `IsSuccess` already exists — just make `Error` nullable |
| Endpoint filters returning `object?` | Loses type safety, breaks Swagger inference |
| Enum error codes | Leaks domain knowledge into SharedKernel, violates open/closed |

### Variants in the Wild

- **Railway-oriented programming (ROP)** — `Result.Bind()`, `Result.Map()` for chaining operations
- **`OneOf<T, Error>`** — discriminated unions from the `OneOf` NuGet package
- **`ErrorOr<T>`** — popular alternative library with built-in `Then()` / `FailWhen()` methods
- **`FluentResults`** — richer results with messages, metadata, reasons

---

## 3. CQRS and MediatR — Command/Query Separation

### Core Interfaces

```csharp
// Commands — change state, return Result (or Result<T> for created resource ID)
public interface ICommand : IRequest<Result> {}
public interface ICommand<TResponse> : IRequest<Result<TResponse>> {}

// Queries — read state, never mutate
public interface IQuery<TResponse> : IRequest<Result<TResponse>> {}
```

### Handler Structure

Each handler lives in a folder named for the use case:

```
Books/
  Commands/
    CreateBook/
      CreateBookCommand.cs
      CreateBookCommandHandler.cs
      CreateBookCommandValidator.cs   ← optional, FluentValidation
  Queries/
    GetBookById/
      GetBookByIdQuery.cs
      GetBookByIdQueryHandler.cs
```

This is the **Vertical Slices** pattern — all code for one use case lives together.

### When to Use MediatR vs. Direct Dispatch

| | MediatR | Direct Handler Injection |
|---|---|---|
| **Pros** | Pipeline behaviors, zero coupling, easy to test | Less indirection, simpler stack traces |
| **Cons** | Magic dispatch, harder debugging | Handlers must be manually registered and injected |
| **Best for** | Projects needing cross-cutting pipeline behaviors | Tiny services or internal tools |

---

## 4. Entity Abstractions — What To Standardize

### EntityBase

```csharp
public abstract class EntityBase
{
    public Guid Id { get; protected set; }
}
```

### AuditableEntity

```csharp
public abstract class AuditableEntity : EntityBase
{
    public DateTimeOffset CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
}
```

Populate audit fields via EF Core `SaveChangesAsync` override or an interceptor — *never* manually in handlers.

### ISoftDeletable

```csharp
public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTimeOffset? DeletedAt { get; }
    void Delete();
}
```

EF Core global query filter: `modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted)`.

### IAggregateRoot and Domain Events

```csharp
public abstract class AggregateRoot : AuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

Domain events are raised inside the aggregate and dispatched *after* `SaveChangesAsync` via a `PublishDomainEventsInterceptor`.

### What Should NOT Be in EntityBase

- Navigation properties or collection initializations
- Business logic
- Infrastructure concerns (e.g., row version, EF-specific annotations)

---

## 5. Endpoint Abstractions — IEndpointDefinition and Beyond

### The `IEndpointDefinition` Pattern (Used in This Project)

```csharp
public interface IEndpointDefinition
{
    void RegisterEndpoints(IEndpointRouteBuilder app);
}
```

Each resource owns its own endpoint class. A reflection-based auto-registrar discovers and wires all implementations at startup. This avoids a monolithic `Program.cs`.

### Carter — The Popular Library Alternative

[Carter](https://github.com/CarterCommunity/Carter) is a community library that formalizes this pattern:

```csharp
public class BookModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/books", GetAllBooks);
    }
}
```

`builder.Services.AddCarter()` + `app.MapCarter()` — done.

### MapGroup and OpenAPI Metadata

```csharp
var group = app.MapGroup("/api/books")
    .WithTags("Books")
    .WithOpenApi()
    .RequireAuthorization();   // applies to all routes in the group
```

### Typed Results for Better OpenAPI

```csharp
// Instead of: Produces<BookDto>()
app.MapGet("/{id}", GetById)
    .Produces<BookDto>()
    .ProducesProblem(StatusCodes.Status404NotFound);
```

With .NET 9+, use `TypedResults` for compile-time OpenAPI metadata:

```csharp
private static async Task<Results<Ok<BookDto>, NotFound>> GetBookById(
    Guid id, ISender sender, CancellationToken ct)
{
    var result = await sender.Send(new GetBookByIdQuery(id), ct);
    return result.IsSuccess ? TypedResults.Ok(result.Value) : TypedResults.NotFound();
}
```

---

## 6. Request / Response / DTO Separation

This is where many projects diverge significantly. Here are the main schools of thought:

### Option A — One DTO Per Operation (Most Explicit)

```
Books/
  DTOs/
    CreateBookRequest.cs
    UpdateBookRequest.cs
    BookResponse.cs
    BookSummaryResponse.cs    ← for list endpoints (fewer fields)
```

**Pros:** Maximum clarity, each type has a single purpose, easy to version independently.
**Cons:** Many files, some duplication.

### Option B — Shared Request Record + Command (Used in This Project)

```csharp
// Command IS the request
app.MapPost("/api/books", async ([FromBody] CreateBookCommand command, ISender sender) => ...);
```

**Pros:** Less ceremony, fewer files.
**Cons:** Couples the HTTP contract to the application layer. A WebApi concern (e.g., `[JsonPropertyName]`) bleeds into Application.

### Option C — Separate Request → Map to Command

```csharp
// WebApi layer
public sealed record CreateBookRequest(string Title, string Author, string ISBN, decimal Price, int Year);

// Mapped in endpoint handler
var command = new CreateBookCommand(request.Title, request.Author, request.ISBN, request.Price, request.Year);
```

**Pros:** Clean separation of HTTP schema from use-case schema. Best practice for large teams.
**Cons:** More boilerplate. Consider Mapperly or AutoMapper to reduce mapping code.

### Naming Conventions

| Type | Naming | Example |
|---|---|---|
| Inbound HTTP payload | `*Request` | `CreateBookRequest` |
| Outbound HTTP payload | `*Response` | `BookResponse`, `BookSummaryResponse` |
| Application DTO (cross-layer) | `*Dto` | `BookDto` |
| MediatR command | `*Command` | `CreateBookCommand` |
| MediatR query | `*Query` | `GetBookByIdQuery` |

### How Deep to Go

**Shallow (startup/small team):**
One `BookDto` used everywhere. One `CreateBookRequest` for create, one `UpdateBookRequest` for update.

**Medium (growth stage):**
Separate read models from write models. `BookDto` for reads, `CreateBookCommand`/`UpdateBookCommand` as write contracts.

**Deep (enterprise/multi-version):**
Version-namespaced DTOs (`V1.BookResponse`, `V2.BookResponse`). Separate DTOs per query (list vs. detail). Explicit mapping classes (Mapperly or custom static mappers).

---

## 7. Validation Abstractions

### The Problem

Every layer of your application has a different validation responsibility, but most projects either validate everything in one place (missing some concerns) or validate the same thing in multiple places (duplicating logic). The architectural question is: *what should be validated where, and how should validation failures be communicated?*

### The Three Layers of Validation

Validation is not a single concern — it spans three distinct layers, each with a different purpose:

```
HTTP Boundary (WebApi)        →  "Is this a well-formed request?"
Application Layer (Handlers)  →  "Does this operation make business sense?"
Domain Layer (Entities)        →  "Does this violate an invariant?"
```

| Layer | What It Validates | Examples | Failure Means |
|---|---|---|---|
| **HTTP Boundary** | Shape, format, required fields | Missing `Title`, `Price` is negative, `ISBN` doesn't match regex | 400 Bad Request |
| **Application** | Business rules requiring external state | ISBN already exists (DB check), user has no permission for this operation | 409 Conflict, 403 Forbidden |
| **Domain** | Entity invariants — things that must *always* be true | `Price` cannot be set below zero, `Title` cannot be empty | Exception (should never reach here if upper layers validate) |

**Key insight:** HTTP boundary validation and domain invariants often *look* similar (both check `Title` is not empty), but they serve different purposes. The HTTP check produces a user-friendly error response. The domain check is a safety net that throws if a developer bypasses the upper layers.

### Approach A — FluentValidation + MediatR Pipeline Behavior (Recommended)

The most popular approach in CQRS projects. Validators live alongside commands/queries and run automatically via a pipeline behavior before the handler executes.

```csharp
// 1. Define a validator alongside the command
public class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    public CreateBookCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.ISBN).NotEmpty().Matches(@"^978-\d{10}$");
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.PublicationYear).InclusiveBetween(1450, DateTime.UtcNow.Year);
    }
}

// 2. Pipeline behavior that runs all validators before the handler
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .SelectMany(v => v.Validate(context).Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
            return (TResponse)Result.Failure(
                new ValidationError(string.Join("; ", failures.Select(f => f.ErrorMessage))));

        return await next();
    }
}
```

Registration:

```csharp
services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

**Pros:** Declarative, composable, auto-discovered, testable in isolation, integrates with Result pattern.
**Cons:** Additional NuGet dependency, learning curve for complex rules, async validators need care.

### Approach B — Data Annotations

Built into .NET, no extra packages. Works well for simple models but becomes unwieldy for complex rules.

```csharp
public sealed record CreateBookRequest
{
    [Required, MaxLength(250)]
    public string Title { get; init; } = string.Empty;

    [Required, RegularExpression(@"^978-\d{10}$")]
    public string ISBN { get; init; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; init; }
}
```

**Pros:** Zero dependencies, familiar to ASP.NET developers, tooling support.
**Cons:** Attribute-based — can't express conditional rules, cross-property rules, or rules requiring injected services. No clean way to integrate with CQRS pipeline.

### Approach C — Manual Validation in Handlers

No framework — just `if` statements at the top of the handler.

```csharp
public async Task<Result<Guid>> Handle(CreateBookCommand cmd, CancellationToken ct)
{
    if (string.IsNullOrWhiteSpace(cmd.Title))
        return Result.Failure<Guid>(new ValidationError("Title is required."));

    if (cmd.Price <= 0)
        return Result.Failure<Guid>(new ValidationError("Price must be positive."));

    // ... proceed with handler logic
}
```

**Pros:** No dependencies, explicit, easy to debug.
**Cons:** Mixes validation with business logic, can't reuse across handlers, doesn't scale.

### Approach D — Minimal API Endpoint Filters

For projects not using MediatR, endpoint filters provide validation at the HTTP boundary:

```csharp
public sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<T>().FirstOrDefault();
        if (request is null) return Results.BadRequest();

        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is null) return await next(context);

        var result = await validator.ValidateAsync(request);
        return result.IsValid
            ? await next(context)
            : Results.ValidationProblem(result.ToDictionary());
    }
}

// Usage
app.MapPost("/api/books", CreateBook)
    .AddEndpointFilter<ValidationFilter<CreateBookRequest>>();
```

**Pros:** Runs before the handler, HTTP-native, works without MediatR.
**Cons:** Only validates at HTTP boundary — doesn't protect against invalid commands dispatched internally.

### Comparison

| Approach | Best For | Complexity | Testability | Scales To |
|---|---|---|---|---|
| **FluentValidation + Pipeline** | CQRS projects with cross-cutting validation | Medium | Excellent | Large |
| **Data Annotations** | Simple DTOs, small APIs | Low | Limited | Small |
| **Manual in Handler** | One-off rules, prototype stage | Low | Good (but coupled) | Small |
| **Endpoint Filters** | Non-MediatR minimal APIs | Medium | Good | Medium |

### Domain Invariants — The Last Line of Defense

Domain validation is *not* the same as input validation. Domain invariants are enforced inside the entity and should throw if violated — they represent states that must *never* exist:

```csharp
public class Book : AuditableEntity
{
    public static Book Create(string title, string author, string isbn, decimal price, int year)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(isbn);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);

        return new Book { Title = title, Author = author, ISBN = isbn, Price = price, Year = year };
    }
}
```

These guard clauses are *not* for producing user-friendly messages — that's the job of FluentValidation. These exist to catch programming errors where a developer bypasses validation and passes invalid data directly.

### What NOT to Do

- **Don't validate only at the domain level** — users get exceptions instead of structured error responses
- **Don't throw `ValidationException` from pipeline behaviors** — return a `Result.Failure` with a `ValidationError` to stay consistent with your error handling strategy
- **Don't duplicate identical rules across layers** — validate shape/format at the boundary, validate business rules in the handler, enforce invariants in the domain
- **Don't use async validators for rules that don't need I/O** — `ValidateAsync` has overhead; use sync validation when no database or service call is required
- **Don't skip validation for internal commands** — if a background job dispatches a command, it should still pass through the validation pipeline

---

## 8. Authentication & Authorization

### JWT Bearer — The Standard for APIs

```csharp
// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// Middleware ordering matters
app.UseAuthentication();
app.UseAuthorization();
```

### Applying Auth to Endpoints

```csharp
// Entire group
var group = app.MapGroup("/api/books")
    .RequireAuthorization();

// Single endpoint
group.MapDelete("/{id:guid}", DeleteBook)
    .RequireAuthorization("AdminOnly");

// Allow anonymous on specific endpoints within a protected group
group.MapGet("/", GetAllBooks)
    .AllowAnonymous();
```

### Policy-Based Authorization

```csharp
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    .AddPolicy("CanManageBooks", policy =>
        policy.RequireClaim("permission", "books:write"));
```

For complex rules, implement `IAuthorizationRequirement` + `IAuthorizationHandler`:

```csharp
public sealed record ResourceOwnerRequirement : IAuthorizationRequirement;

public sealed class ResourceOwnerHandler : AuthorizationHandler<ResourceOwnerRequirement, Book>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOwnerRequirement requirement,
        Book resource)
    {
        if (context.User.FindFirstValue(ClaimTypes.NameIdentifier) == resource.OwnerId.ToString())
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
```

### API Key Authentication (for Service-to-Service)

```csharp
public sealed class ApiKeyEndpointFilter : IEndpointFilter
{
    private readonly IConfiguration _config;

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue("X-Api-Key", out var key)
            || key != _config["ApiKey"])
            return Results.Unauthorized();

        return await next(context);
    }
}
```

### Which Approach to Use

| Approach | Best For |
|---|---|
| JWT Bearer | User-facing APIs, SPAs, mobile apps |
| API Key | Service-to-service, webhooks, simple internal APIs |
| OAuth 2.0 / OpenID Connect | Delegated access, third-party integrations, SSO |
| Cookie auth | Server-rendered apps (Razor Pages, Blazor Server) |

---

## 9. Global Exception Handling

The Result pattern handles **expected** failures (not found, conflict, validation). But **unexpected** exceptions (database connection drops, null references, third-party SDK failures) still need a catch-all that returns a consistent Problem Details response.

### `IExceptionHandler` (.NET 8+)

The modern replacement for exception-handling middleware:

```csharp
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "InternalError",
            Detail = "An unexpected error occurred."
        }, cancellationToken);

        return true;
    }
}
```

Registration:

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// In the pipeline — must come before routing
app.UseExceptionHandler();
```

### How This Complements the Result Pattern

```
Expected failures (domain logic)  →  Result pattern  →  ToProblemResult()
Unexpected failures (crashes)     →  IExceptionHandler → ProblemDetails 500
```

Both produce RFC 7807 Problem Details — API consumers get a consistent error shape regardless of the failure source.

### What NOT to Do

- Don't catch `Exception` inside handlers — let unexpected errors bubble up to `IExceptionHandler`
- Don't return stack traces in production — log them, but return a generic message
- Don't use exception middleware for expected failures — that's what the Result pattern is for

---

## 10. Structured Logging & Observability

### The Problem

Most projects start with `Console.WriteLine` or `_logger.LogInformation("Something happened")` — unstructured text that's impossible to search, filter, or correlate at scale. When a production incident occurs, you need to answer questions like *"show me all requests from user X that touched the Orders service in the last hour"* — and flat text logs can't do that.

The architectural question is: *what should you log, where should logs go, and how do you connect logs across services and layers?*

### Structured vs. Unstructured Logging

The single most impactful logging decision is whether to use structured logging.

```csharp
// Unstructured — impossible to query programmatically
_logger.LogInformation($"Book {book.Id} created by user {userId} with price {book.Price}");

// Structured — each value is a searchable, filterable property
_logger.LogInformation("Book {BookId} created by {UserId} with price {Price}",
    book.Id, userId, book.Price);
```

The structured version produces a log entry with discrete properties (`BookId`, `UserId`, `Price`) that log aggregation tools can index. This is the foundation — everything else builds on top of it.

**Critical rule:** Never use string interpolation (`$"..."`) with `ILogger`. It defeats structured logging — the values become part of the message template instead of separate properties.

### Choosing a Logging Framework

| Framework | Strengths | Weaknesses | Best For |
|---|---|---|---|
| **Built-in `ILogger`** | Zero dependencies, Microsoft-supported, DI-integrated | Limited sinks, no enrichment, basic formatting | Small projects, libraries |
| **Serilog** | Rich sink ecosystem, structured-first, config-driven, enrichers | NuGet dependency, learning curve for advanced config | Most production APIs |
| **NLog** | Mature, flexible layouts, many targets | XML-heavy config, less structured-first than Serilog | Legacy projects, teams familiar with NLog |

**Recommendation:** Use Serilog for application projects. Use `ILogger` (the Microsoft abstraction) as your injection surface so you're not tightly coupled to Serilog — Serilog plugs in as the *provider* behind `ILogger`.

### Serilog Setup

```csharp
// Program.cs
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));
```

```json
// appsettings.json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.Seq"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "Seq", "Args": { "serverUrl": "http://localhost:5341" } }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

**Key point:** Override noisy namespaces. ASP.NET Core and EF Core emit verbose `Information`-level logs for every request and query — suppress them to `Warning` and use dedicated middleware (below) for cleaner request logging.

### Log Level Strategy

Choosing the right log level is an architectural decision — it determines what's visible during normal operation vs. during incident investigation.

| Level | When to Use | Examples |
|---|---|---|
| **Trace** | Step-by-step execution detail — *never* in production | Entering/exiting methods, variable values |
| **Debug** | Diagnostic information useful during development | Cache hit/miss, SQL query generated, request deserialized |
| **Information** | Normal operational events — things you'd want on a dashboard | Request completed, command handled, user logged in |
| **Warning** | Something unexpected but recoverable happened | Retry triggered, deprecated endpoint called, rate limit approaching |
| **Error** | An operation failed — requires attention | Unhandled exception, external service timeout, database connection lost |
| **Fatal/Critical** | Application is about to crash or is in an unrecoverable state | Out of memory, configuration missing at startup, database migration failed |

**Production default:** `Information`. Set `Debug` per-namespace when investigating issues — Serilog's `MinimumLevel.Override` makes this possible without redeployment if you use a reloadable config source.

### What to Log vs. What NOT to Log

| Log This | Never Log This |
|---|---|
| Command/query names and duration | Passwords, tokens, API keys |
| User ID (not username/email unless needed) | Credit card numbers, SSNs |
| Request path, method, status code | Full request/response bodies in production |
| Correlation IDs, trace IDs | Connection strings |
| Error messages and exception types | PII (emails, phone numbers) without consent |
| Business events (order placed, book created) | Health check noise (log these at `Debug` only) |

If you accidentally log sensitive data, it persists in your log storage and may violate GDPR, HIPAA, or PCI-DSS. Treat log output as a security surface.

### Request Logging Middleware

Replace ASP.NET Core's built-in verbose request logging (multiple lines per request) with a single structured line:

```csharp
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("UserId",
            httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
    };

    // Don't log health check endpoints — they create noise
    options.GetLevel = (httpContext, elapsed, ex) =>
        httpContext.Request.Path.StartsWithSegments("/health")
            ? LogEventLevel.Debug
            : LogEventLevel.Information;
});
```

### Correlation IDs

Track a single logical operation across multiple services and layers. The first service generates the ID; downstream services propagate it.

```csharp
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
        ?? Guid.NewGuid().ToString();

    context.Response.Headers["X-Correlation-Id"] = correlationId;

    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});
```

When calling downstream services, forward the header:

```csharp
httpClient.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId);
```

Now every log entry across every service for the same user action shares a `CorrelationId` — you can filter an entire distributed operation in one query.

### MediatR Logging Behavior

See Section 16 for the `LoggingBehavior` pattern that logs command/query names and execution duration. This gives you application-level observability without touching endpoint code.

### Health Checks

Health checks are part of observability — they tell orchestrators (Docker, Kubernetes, load balancers) whether your service is ready to receive traffic.

```csharp
// Registration
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString, name: "database")
    .AddRedis(redisConnectionString, name: "cache");   // if applicable

// Endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

| Check Type | Purpose | Example |
|---|---|---|
| **Liveness** (`/health/live`) | Is the process alive? | Always returns 200 — if it doesn't, restart the container |
| **Readiness** (`/health/ready`) | Can the service handle requests? | Checks DB connection, cache connection, dependent services |
| **Startup** (`/health/startup`) | Has the service finished initializing? | Migrations complete, caches warmed, config loaded |

Docker Compose integration:

```yaml
healthcheck:
  test: curl --fail http://localhost:8080/health || exit 1
  interval: 15s
  timeout: 5s
  retries: 3
```

### Centralized Log Aggregation

In production, logs must leave the application and flow to a centralized system for search, alerting, and dashboards.

| Stack | Components | Best For |
|---|---|---|
| **Seq** | Seq server (single binary) | Small teams, .NET-native, structured logging built-in |
| **ELK** | Elasticsearch + Logstash + Kibana | Large scale, flexible, widely adopted |
| **Grafana + Loki** | Loki (log storage) + Grafana (dashboards) | Cost-effective, pairs with Prometheus metrics |
| **Application Insights** | Azure-native APM | Azure-hosted projects, integrated tracing + metrics + logs |
| **Datadog / New Relic** | SaaS APM platforms | Teams that prefer managed solutions, broad integrations |

**Recommendation for .NET projects:** Start with **Seq** for development (free single-user, excellent structured log viewer). Move to **Application Insights** or **Grafana + Loki** for production depending on your cloud provider.

### OpenTelemetry — The Three Pillars

OpenTelemetry is the vendor-neutral standard for observability. It covers all three pillars: **logs**, **traces**, and **metrics**.

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter());
```

| Pillar | What It Answers | Tool |
|---|---|---|
| **Logs** | What happened? | Serilog → Seq / Loki / Application Insights |
| **Traces** | How did a request flow across services? | OpenTelemetry → Jaeger / Zipkin / Application Insights |
| **Metrics** | What's the trend? (Request rate, error rate, latency) | OpenTelemetry → Prometheus / Grafana / Application Insights |

### How Deep to Go

| Concern | Shallow | Medium | Deep |
|---|---|---|---|
| **Framework** | Built-in `ILogger` with Console | Serilog with Console + file sink | Serilog with Seq/Loki + enrichers + filters |
| **Request logging** | Default ASP.NET Core logs | `UseSerilogRequestLogging()` | + Correlation IDs + user context enrichment |
| **Health checks** | None | `/health` with DB check | Liveness/readiness/startup separation |
| **Aggregation** | Console output | Seq (development) | Centralized stack (ELK / Loki / App Insights) |
| **Tracing** | None | OpenTelemetry basic instrumentation | Distributed tracing with custom spans |
| **Alerting** | Manual log review | Log-level-based alerts | SLO-based alerting with error budgets |

### What NOT to Do

- **Don't use string interpolation with `ILogger`** — `$"User {name}"` defeats structured logging. Use message templates: `"User {Name}"`.
- **Don't log at `Information` level in hot paths** — a loop logging 10,000 items at `Information` will flood your log store. Use `Debug` or `Trace` for high-volume paths.
- **Don't log full request/response bodies in production** — they may contain sensitive data and consume enormous storage.
- **Don't ignore log volume costs** — centralized logging is priced by ingestion volume. Suppress noisy namespaces and use sampling for high-throughput services.
- **Don't skip health checks** — without them, your orchestrator can't distinguish a healthy service from a crashed one.
- **Don't treat logging as an afterthought** — instrument logging at the same time you write the feature, not after a production incident forces you to.

---

## 11. Configuration — Options Pattern

### Strongly-Typed Settings

```csharp
// Define a settings class
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public int ExpiryMinutes { get; init; }
}
```

```json
// appsettings.json
{
  "Jwt": {
    "Issuer": "bookstore-api",
    "Audience": "bookstore-client",
    "Key": "your-secret-key-here",
    "ExpiryMinutes": 60
  }
}
```

### Registration

```csharp
// Bind and validate at startup
builder.Services.AddOptions<JwtSettings>()
    .BindConfiguration(JwtSettings.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

### Injection

```csharp
// IOptions<T> — singleton, read once at startup
// IOptionsSnapshot<T> — scoped, re-reads per request (useful for reloadable config)
// IOptionsMonitor<T> — singleton, notifies on change

public class TokenService(IOptions<JwtSettings> options)
{
    private readonly JwtSettings _settings = options.Value;
}
```

### Why Not `IConfiguration` Directly?

| `IConfiguration["Jwt:Key"]` | `IOptions<JwtSettings>` |
|---|---|
| Returns `string?` — no type safety | Strongly typed, IDE autocomplete |
| No validation | `ValidateDataAnnotations()` catches issues at startup |
| Magic string keys | Compile-time property access |
| Scattered across codebase | Centralized in one settings class |

---

## 12. API Versioning

### URL-Based Versioning (Simplest)

```csharp
var v1 = app.MapGroup("/api/v1/books").WithTags("Books v1");
var v2 = app.MapGroup("/api/v2/books").WithTags("Books v2");

v1.MapGet("/", GetAllBooksV1);
v2.MapGet("/", GetAllBooksV2);  // returns different shape
```

**Pros:** Obvious, easy to route, cache-friendly.
**Cons:** URL changes break clients. Multiple route groups to maintain.

### Header-Based Versioning (with `Asp.Versioning`)

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version");
});
```

**Pros:** URLs stay clean. Easy to default.
**Cons:** Less discoverable. Harder to test in a browser.

### Which to Choose

| Approach | Best For |
|---|---|
| URL path (`/v1/`, `/v2/`) | Public APIs, breaking changes are rare |
| Header (`X-Api-Version`) | Internal APIs, gradual migration |
| Query string (`?api-version=2`) | Compromise — visible but doesn't change the path |

### When to Version

Version when you make **breaking changes** to the response shape, remove fields, or change behavior. Don't version for additive changes (new optional fields, new endpoints) — those are backwards-compatible.

---

## 13. Idempotency

### The Problem

`POST /api/orders` called twice due to a network retry creates two orders. Non-GET operations need idempotency guarantees for safety.

### Idempotency Keys

The client sends a unique key with the request. The server stores the result and returns the cached response on replay.

```csharp
public sealed class IdempotencyFilter : IEndpointFilter
{
    private readonly IIdempotencyStore _store;

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue("Idempotency-Key", out var key))
            return await next(context);

        var cached = await _store.GetAsync(key!);
        if (cached is not null)
            return cached;

        var result = await next(context);
        await _store.SetAsync(key!, result, TimeSpan.FromHours(24));

        return result;
    }
}
```

Apply to mutation endpoints:

```csharp
group.MapPost("/", CreateBook)
    .AddEndpointFilter<IdempotencyFilter>();
```

### Storage Options

| Store | Tradeoff |
|---|---|
| In-memory (`ConcurrentDictionary`) | Simple, lost on restart — fine for dev |
| Redis | Distributed, TTL support, production-ready |
| Database table | Transactional with the main operation — strongest guarantee |

### Which Operations Need It?

| Method | Naturally Idempotent? | Needs Key? |
|---|---|---|
| GET | Yes | No |
| PUT | Yes (full replace) | Usually no |
| DELETE | Yes | Usually no |
| POST | **No** | **Yes** |
| PATCH | Depends | Sometimes |

---

## 14. Pagination Abstractions

### The Problem

Returning all records from a database table in a single response doesn't scale. A `GET /api/books` endpoint returning 50,000 books will be slow, consume excessive memory, and overwhelm the client. Pagination solves this, but the *type* of pagination you choose has significant implications for performance, client complexity, and user experience.

The architectural question is: *which pagination strategy fits your data access patterns, and how do you standardize it across all list endpoints?*

### Offset-Based Pagination (Page + PageSize)

The most familiar approach — clients request a specific page number and size.

```csharp
// Shared abstractions (SharedKernel or Application layer)
public sealed record PagedQuery(int Page = 1, int PageSize = 20);

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
```

EF Core implementation:

```csharp
public async Task<PagedResult<BookDto>> Handle(GetBooksPagedQuery request, CancellationToken ct)
{
    var query = _context.Books.AsNoTracking();

    var totalCount = await query.CountAsync(ct);

    var items = await query
        .OrderBy(b => b.Title)
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .Select(b => new BookDto(b.Id, b.Title, b.Author, b.ISBN, b.Price, b.PublicationYear))
        .ToListAsync(ct);

    return new PagedResult<BookDto>
    {
        Items = items,
        TotalCount = totalCount,
        Page = request.Page,
        PageSize = request.PageSize
    };
}
```

**Pros:** Simple for clients, supports "jump to page N", familiar UX pattern.
**Cons:** `COUNT(*)` is expensive on large tables. `OFFSET` scans and discards rows — page 1000 is much slower than page 1. Inconsistent results if data is inserted/deleted between page requests.

### Cursor-Based Pagination (Keyset Pagination)

Instead of a page number, the client sends the last seen value (cursor) and asks for the next N items after it. The database uses a `WHERE` clause instead of `OFFSET`, which is consistently fast regardless of position.

```csharp
public sealed record CursorPagedQuery(Guid? After = null, int First = 20);

public sealed class CursorPagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public Guid? NextCursor { get; init; }
    public bool HasNextPage => NextCursor is not null;
}
```

EF Core implementation:

```csharp
public async Task<CursorPagedResult<BookDto>> Handle(GetBooksCursorQuery request, CancellationToken ct)
{
    var query = _context.Books.AsNoTracking().OrderBy(b => b.Id);

    if (request.After is not null)
        query = (IOrderedQueryable<Book>)query.Where(b => b.Id.CompareTo(request.After.Value) > 0);

    var items = await query
        .Take(request.First + 1)   // fetch one extra to detect if there's a next page
        .Select(b => new BookDto(b.Id, b.Title, b.Author, b.ISBN, b.Price, b.PublicationYear))
        .ToListAsync(ct);

    var hasNextPage = items.Count > request.First;
    if (hasNextPage) items.RemoveAt(items.Count - 1);

    return new CursorPagedResult<BookDto>
    {
        Items = items,
        NextCursor = hasNextPage ? items[^1].Id : null,
        HasNextPage = hasNextPage
    };
}
```

**Pros:** Constant-time performance regardless of position (no `OFFSET` scan). No inconsistency from concurrent inserts/deletes. Works well with infinite scroll UIs.
**Cons:** Can't "jump to page N". Requires a stable, unique, sortable column for the cursor. Slightly more complex for clients.

### Comparison

| Concern | Offset-Based | Cursor-Based |
|---|---|---|
| **Performance at depth** | Degrades (page 1000 is slow) | Constant (always fast) |
| **Jump to page N** | Yes | No |
| **Data consistency** | Gaps/duplicates possible between pages | Consistent — no items skipped or repeated |
| **Total count** | Available (but expensive) | Not available without a separate query |
| **Client complexity** | Simple (`?page=3&pageSize=20`) | Moderate (`?after=<guid>&first=20`) |
| **Best for** | Admin panels, dashboards, small datasets | Public APIs, mobile apps, large/real-time datasets |

### Filtering and Sorting Alongside Pagination

Pagination rarely exists in isolation — clients typically want to filter and sort as well. Standardize these as query parameters:

```csharp
public sealed record GetBooksPagedQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string? SortBy = "Title",
    bool SortDescending = false) : IQuery<PagedResult<BookDto>>;
```

Endpoint:

```
GET /api/books?page=2&pageSize=10&search=tolkien&sortBy=Price&sortDescending=true
```

EF Core dynamic sorting (safe approach — whitelist allowed columns):

```csharp
private static readonly Dictionary<string, Expression<Func<Book, object>>> SortColumns = new(StringComparer.OrdinalIgnoreCase)
{
    ["Title"] = b => b.Title,
    ["Author"] = b => b.Author,
    ["Price"] = b => b.Price,
    ["PublicationYear"] = b => b.PublicationYear
};

// In the handler
var sortExpression = SortColumns.GetValueOrDefault(request.SortBy ?? "Title", b => b.Title);
query = request.SortDescending
    ? query.OrderByDescending(sortExpression)
    : query.OrderBy(sortExpression);
```

**Never** pass user input directly into `OrderBy` as a raw string — this opens the door to injection or runtime exceptions. Always whitelist allowed sort columns.

### Pagination Limits

Always enforce server-side limits to prevent clients from requesting the entire dataset:

```csharp
public sealed record PagedQuery
{
    private const int MaxPageSize = 100;

    public int Page { get; init; } = 1;

    private int _pageSize = 20;
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = Math.Min(value, MaxPageSize);
    }
}
```

### How Deep to Go

| Concern | Shallow | Medium | Deep |
|---|---|---|---|
| **Strategy** | Return all items (no pagination) | Offset-based with `PagedResult<T>` | Cursor-based for public APIs, offset for admin |
| **Sorting** | Fixed sort order | Single `sortBy` parameter with whitelist | Multi-column sort, per-field direction |
| **Filtering** | None or basic `?search=` | Per-field filters (`?author=X&minPrice=10`) | Full filter expression language (OData, custom DSL) |
| **Metadata** | Items only | `TotalCount`, `HasNextPage`, `TotalPages` | Hypermedia links (`next`, `prev`, `first`, `last`) |
| **Limits** | None | Max page size enforced | Rate limiting + max page size + query timeout |

### What NOT to Do

- **Don't return unbounded collections** — every list endpoint should paginate, even if you "only have 50 books right now." Data grows.
- **Don't use `OFFSET` for deep pagination on large tables** — page 10,000 means scanning and discarding 200,000 rows. Use cursor-based instead.
- **Don't allow arbitrary page sizes** — a client requesting `pageSize=1000000` can bring down your service. Always cap with a server-side maximum.
- **Don't rely on `COUNT(*)` for cursor-based pagination** — it defeats the performance advantage. If you need total count, offer it as an optional parameter (`?includeTotalCount=true`).
- **Don't paginate over unstable sort orders** — sorting by a column that changes (e.g., `UpdatedAt`) causes items to shift between pages mid-traversal. Use a stable tiebreaker column (typically `Id`).
- **Don't build raw SQL sort expressions from user input** — whitelist allowed columns to prevent injection and unexpected errors.

---

## 15. Domain Events and Outbox Pattern

### Why Domain Events

Domain events decouple aggregate changes from their side effects. When a `Book` is created, you might want to:
- Send a notification
- Update a search index
- Emit a message to a queue

None of these belong inside the `Book` aggregate or `CreateBookCommandHandler`.

### Implementation

```csharp
public interface IDomainEvent : INotification { }

public sealed record BookCreatedDomainEvent(Guid BookId, string Title) : IDomainEvent;
```

Dispatch in `SaveChangesAsync`:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    var domainEvents = ChangeTracker.Entries<AggregateRoot>()
        .Select(e => e.Entity)
        .Where(e => e.DomainEvents.Any())
        .SelectMany(e => { var events = e.DomainEvents; e.ClearDomainEvents(); return events; })
        .ToList();

    var result = await base.SaveChangesAsync(ct);

    foreach (var domainEvent in domainEvents)
        await _publisher.Publish(domainEvent, ct);

    return result;
}
```

### Outbox Pattern for Reliability

For distributed systems, dispatch domain events via an **Outbox Table** instead of in-process to guarantee at-least-once delivery across failures:

1. Persist `OutboxMessage` (serialized event) in the same transaction as the aggregate change.
2. A background worker (`IHostedService`) polls the outbox and publishes to the message broker.
3. Mark messages as `ProcessedAt` after successful dispatch.

Libraries: **MassTransit**, **Wolverine**, **Quartz.NET** (for the worker).

---

## 16. Cross-Cutting Concerns via Pipeline Behaviors

MediatR pipeline behaviors are middleware for your application layer:

```csharp
// Execution order (registered left-to-right)
LoggingBehavior → ValidationBehavior → CachingBehavior → Handler
```

### Common Behaviors

| Behavior | Purpose |
|---|---|
| `LoggingBehavior` | Log command/query name and duration |
| `ValidationBehavior` | Run FluentValidation before handler |
| `CachingBehavior` | Return cached result for `ICacheableQuery` |
| `TransactionBehavior` | Wrap handler in a DB transaction |
| `AuthorizationBehavior` | Policy-based authorization at handler level |
| `RetryBehavior` | Polly-based retry for transient failures |

### Example: Logging Behavior

```csharp
public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var name = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName}", name);
        var sw = Stopwatch.StartNew();
        var response = await next();
        _logger.LogInformation("Handled {RequestName} in {Elapsed}ms", name, sw.ElapsedMilliseconds);
        return response;
    }
}
```

---

## 17. Folder & Project Structure Deep Dive

### Project Boundaries (What Belongs Where)

```
Bookstore.SharedKernel        ← Zero business logic. Base types, Result, Error, interfaces.
                                 No NuGet dependencies besides the BCL.

Bookstore.Domain              ← Pure C#. Entities, value objects, domain events.
                                 No EF Core, no MediatR, no HTTP.

Bookstore.Application         ← Use cases. Commands, queries, handlers, DTOs.
                                 Depends on: MediatR, FluentValidation, SharedKernel, Domain.
                                 Defines interfaces (IApplicationDbContext) implemented by Infrastructure.

Bookstore.Infrastructure      ← EF Core, external APIs, email, blob storage, etc.
                                 Depends on: Application, EF Core, drivers, SDKs.

Bookstore.WebApi              ← HTTP entry point. Minimal API endpoints, OpenAPI, auth middleware.
                                 Depends on: Application, Infrastructure (only for DI wiring).
```

### Folder Structure Within Application (Vertical Slices)

```
Application/
  Abstractions/
    IApplicationDbContext.cs
    ICacheService.cs
  Books/
    Commands/
      CreateBook/
        CreateBookCommand.cs
        CreateBookCommandHandler.cs
        CreateBookCommandValidator.cs
      UpdateBook/...
      DeleteBook/...
    Queries/
      GetBookById/
        GetBookByIdQuery.cs
        GetBookByIdQueryHandler.cs
      GetAllBooks/...
      GetBooksPaged/...
    DTOs/
      BookDto.cs
      BookSummaryDto.cs
  DependencyInjection.cs
```

### Alternative: Feature Folders (Full Vertical Slices)

Some teams put the endpoint *and* the handler in the same folder:

```
Features/
  Books/
    CreateBook/
      Endpoint.cs       ← Minimal API route registration
      Command.cs
      Handler.cs
      Validator.cs
    GetBookById/
      Endpoint.cs
      Query.cs
      Handler.cs
```

This maximizes locality — every piece of a feature is in one place. The tradeoff is less layer enforcement.

### How Deep Is "Deep Enough"?

| Concern | Shallow | Medium | Deep |
|---|---|---|---|
| DTOs | Single `BookDto` | Read vs. Write DTOs | Per-operation DTOs + versioning |
| Commands | One per CRUD op | Separate create/update/delete | Fine-grained domain operations (`PublishBookCommand`, `RetireBookCommand`) |
| Queries | One per endpoint | + Dedicated read models | + Separate read database (CQRS read side) |
| Errors | Single `Error` record | Typed error hierarchy (`NotFoundError`, `ConflictError`) | Rich error types with metadata per aggregate |
| Validators | Inline in handler | Separate validator class | Composite validators, async validators |
| Tests | Happy-path unit tests | Unit + integration tests | Unit + integration + contract tests + architecture tests |

---

## 18. When NOT To Abstract

Abstraction for its own sake is a liability. Here are anti-patterns to avoid:

### Over-Abstracting Small APIs

If your service has 3-4 resources and will stay that way, CQRS + Clean Architecture is overengineering. Start with grouped minimal API endpoints and introduce layers when complexity justifies them.

### Generic Repository on Top of EF Core

```csharp
// Pointless — DbContext IS already a unit of work + repository
public interface IRepository<T> { Task<T?> GetByIdAsync(Guid id); ... }
```

EF Core's `DbSet<T>` and `DbContext` already provide repository and unit-of-work semantics. Add abstraction only if you need to swap out the ORM, which is rare in practice. A thin `IApplicationDbContext` interface (as used in this project) is the right level.

### One-Line Handlers

If a handler is a one-liner, consider whether the use case needs a dedicated class:

```csharp
// This might not need a handler
public async Task<Result<Guid>> Handle(CreateBookCommand cmd, CancellationToken ct)
    => (await _context.Books.AddAsync(Book.Create(...))).Entity.Id;
```

### Mapping Every Layer

If Domain → Application → WebApi mappings are 1:1 identity transforms, consolidate DTOs. Mappings have value when the shapes meaningfully differ.

### Too Many Pipeline Behaviors

A chain of 6+ behaviors makes debugging a stack trace nightmare. Prefer behaviors for genuinely cross-cutting concerns; keep business logic in handlers.

---

## 19. Summary Checklist

Use this as a decision guide when starting a new API project:

- [ ] **Result Pattern** — typed error hierarchy + extension method for HTTP mapping
- [ ] **CQRS** — separate reads from writes using commands and queries
- [ ] **Vertical Slices** — group files by use case, not by layer type
- [ ] **IEndpointDefinition** — keep `Program.cs` clean; one class per resource
- [ ] **IApplicationDbContext** — thin interface over EF Core; no generic repository
- [ ] **EntityBase / AuditableEntity** — standardize Id, CreatedAt, UpdatedAt
- [ ] **Authentication** — JWT Bearer for user-facing, API keys for service-to-service
- [ ] **Authorization** — policy-based with `AddAuthorizationBuilder()`
- [ ] **Global exception handling** — `IExceptionHandler` for unexpected errors, Result pattern for expected
- [ ] **Structured logging** — Serilog + correlation IDs + request logging middleware
- [ ] **Options pattern** — `IOptions<T>` with `ValidateOnStart()` for all configuration
- [ ] **API versioning** — URL-based for public APIs, header-based for internal
- [ ] **Idempotency** — idempotency keys for POST operations
- [ ] **Domain Events** — decouple side effects from aggregates
- [ ] **Outbox Pattern** — reliable event dispatch in distributed systems
- [ ] **Pipeline Behaviors** — logging, validation, caching as cross-cutting middleware
- [ ] **FluentValidation** — declarative, composable validators wired into the pipeline
- [ ] **Typed Results** — use `Results<Ok<T>, NotFound>` for compile-time OpenAPI metadata
- [ ] **Separate Request/Response DTOs** — `*Request`, `*Response`, `*Dto` naming convention
- [ ] **Paged results** — standardize `PagedResult<T>` for list endpoints
- [ ] **Architecture tests** — use `NetArchTest` to enforce layer dependency rules
- [ ] **Do not over-abstract** — each abstraction must justify its indirection cost

---

*This document reflects patterns from the .NET Clean Architecture community, including practices popularized by Milan Jovanovic, Jason Taylor's CleanArchitecture template, the Ardalis.Result library, and production systems using MediatR, FluentValidation, and EF Core.*

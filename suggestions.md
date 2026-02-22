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
8. [Pagination Abstractions](#8-pagination-abstractions)
9. [Domain Events and Outbox Pattern](#9-domain-events-and-outbox-pattern)
10. [Cross-Cutting Concerns via Pipeline Behaviors](#10-cross-cutting-concerns-via-pipeline-behaviors)
11. [Folder & Project Structure Deep Dive](#11-folder--project-structure-deep-dive)
12. [When NOT To Abstract](#12-when-not-to-abstract)
13. [Summary Checklist](#13-summary-checklist)

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

### The Result Pattern

```csharp
public class Result
{
    protected Result(bool isSuccess, Error error) { ... }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

public class Result<TValue> : Result
{
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value of a failed Result.");
}
```

```csharp
public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}
```

### Mapping Result to HTTP in Minimal API

```csharp
// Centralized extension method
public static IResult ToHttpResult<T>(this Result<T> result) => result.IsSuccess
    ? Results.Ok(result.Value)
    : result.Error.Code.StartsWith("NotFound") ? Results.NotFound(result.Error)
    : result.Error.Code.StartsWith("Conflict") ? Results.Conflict(result.Error)
    : Results.Problem(result.Error.Description);
```

Or a more sophisticated approach using a `ProblemDetails`-aware mapper with an error code → HTTP status dictionary.

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
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
}
```

Populate audit fields via EF Core `SaveChangesAsync` override or an interceptor — *never* manually in handlers.

### ISoftDeletable

```csharp
public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedAt { get; }
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

### FluentValidation + MediatR Pipeline Behavior

```csharp
// 1. Define a validator alongside the command
public class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    public CreateBookCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.ISBN).NotEmpty().Matches(@"^978-\d{10}$");
        RuleFor(x => x.Price).GreaterThan(0);
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
            throw new ValidationException(failures); // or return a failed Result

        return await next();
    }
}
```

### Registering Validators

```csharp
services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

### Minimal API Filter Alternative

For projects not using MediatR, endpoint filters provide a similar capability:

```csharp
app.MapPost("/api/books", CreateBook)
    .AddEndpointFilter<ValidationFilter<CreateBookRequest>>();
```

---

## 8. Pagination Abstractions

### Standard Offset-Based Pagination

```csharp
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

### Cursor-Based Pagination (Preferred for Large Datasets)

```csharp
public sealed record CursorPagedQuery(string? After, int First = 20);

public sealed class CursorPagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public string? NextCursor { get; init; }
    public bool HasNextPage => NextCursor is not null;
}
```

EF Core implementation: order by a stable column (usually `Id` or `CreatedAt`), filter by `WHERE Id > @cursor`.

---

## 9. Domain Events and Outbox Pattern

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

## 10. Cross-Cutting Concerns via Pipeline Behaviors

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

## 11. Folder & Project Structure Deep Dive

### Project Boundaries (What Belongs Where)

```
Bookstore.SharedKernel        ← Zero business logic. Base types, Result, Error, interfaces.
                                 No NuGet dependencies besides the BCL.

Bookstore.Domain              ← Pure C#. Entities, value objects, domain events, domain errors.
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
| Errors | Generic `Error` record | Domain-specific error types per aggregate | Rich error types with metadata, error codes by HTTP status |
| Validators | Inline in handler | Separate validator class | Composite validators, async validators |
| Tests | Happy-path unit tests | Unit + integration tests | Unit + integration + contract tests + architecture tests |

---

## 12. When NOT To Abstract

Abstraction for its own sake is a liability. Here are anti-patterns to avoid:

### Over-Abstracting Small APIs

If your service has 3-4 resources and will stay that way, CQRS + Clean Architecture is overengineering. Start with grouped minimal API endpoints and introduce layers when complexity justifies them.

### Generic Repository on Top of EF Core

```csharp
// ❌ Pointless — DbContext IS already a unit of work + repository
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

## 13. Summary Checklist

Use this as a decision guide when starting a new API project:

- [ ] **Result Pattern** — avoid exceptions for expected failures; use `Result<T>`
- [ ] **CQRS** — separate reads from writes using commands and queries
- [ ] **Vertical Slices** — group files by use case, not by layer type
- [ ] **IEndpointDefinition** — keep `Program.cs` clean; one class per resource
- [ ] **IApplicationDbContext** — thin interface over EF Core; no generic repository
- [ ] **EntityBase / AuditableEntity** — standardize Id, CreatedAt, UpdatedAt
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

*This document reflects patterns from the .NET Clean Architecture community, including practices popularized by Milan Jovanović, Jason Taylor's CleanArchitecture template, the Ardalis.Result library, and production systems using MediatR, FluentValidation, and EF Core.*

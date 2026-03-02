# Bookstore API — Claude Code Instructions

## Project Layout

```
src/backend/
  Bookstore.SharedKernel/    # Result pattern, Error types, base entity abstractions
  Bookstore.Domain/          # Entities (Author, Book), value objects, domain logic
  Bookstore.Application/     # CQRS handlers, validators, DTOs, DI registration
  Bookstore.Infrastructure/  # EF Core DbContext, migrations, data configuration
  Bookstore.WebApi/          # ASP.NET Core minimal API endpoints
tests/
  Bookstore.Domain.Tests/
  Bookstore.Application.Tests/
```

## Architecture

- **Clean Architecture** — dependencies flow inward; Domain has zero external dependencies
- **CQRS** via [Mediator](https://github.com/martinothamar/Mediator) (`ICommand<T>` / `IQuery<T>`)
- **Result pattern** (`SharedKernel.Results`) over exceptions for expected failures (validation, not found, conflict)
- **FluentValidation** — one `IValidator<TCommand>` injected and called explicitly per handler (no pipeline behavior)
- **DDD** — entities have private setters and factory methods; IDs are strongly-typed value objects

## Tools & Stack

- .NET 10 / C# 13
- EF Core (SQL Server in production, InMemory for tests)
- FluentValidation
- Mediator (martinothamar)
- xUnit + Shouldly for tests
- Docker & docker-compose for local infrastructure
- GitHub for hosting, PRs, issues
- Project board: number `2`, owner `repaskristoftamas`
  - Status field ID: `PVTSSF_lAHOApxqws4BP2Z2zg-I7Tg`
  - Todo: `f75ad846` | In Progress: `47fc9ee4` | Ready for Review: `d9d9aaf5` | Done: `98236657`

## Code Style

- Clean, readable code over clever code
- Small functions with single responsibility
- Meaningful names — no abbreviations unless universally understood
- Comments only for "why", never for "what"
- No dead code, no commented-out code
- Always add XML summaries to public members (interfaces, classes, records, methods) and to private methods

## Testing

- **Framework**: xUnit
- **Assertions**: Shouldly (`.ShouldBe(...)`)
- **Mocking**: Moq
- **Pattern**: Arrange-Act-Assert
- **Naming**: `MethodName_Scenario_ExpectedResult`
- Use InMemory EF Core DbContext for handler tests (not mocked `IApplicationDbContext`)
- Use setups and object builders to avoid boilerplate, but keep test cases independent

## Git Conventions

- [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/)
- Imperative subject, max 72 chars, no trailing period
- Body explains "why", not "what"
- Feature branches off main: `issue-{number}-{short-kebab-description}`
- Never force push to main

## What NOT To Do

- Don't create abstractions for things used once
- Don't use regions
- Don't wrap everything in try-catch — let exceptions bubble to the global handler
- Don't add null checks defensively everywhere — trust internal code boundaries
- Don't refactor or "improve" code adjacent to the requested change
- Don't add backwards-compatibility shims (unused params, re-exports, `// removed` comments)
- Don't introduce a new pattern when the codebase already has an established one
- Don't add logging to every method — log at boundaries and on failures
- Don't add default values to parameters when the caller should be explicit
- Don't rename variables or reformat code you didn't functionally change
- Don't create helper/utility classes for one-off operations
- Don't add feature flags or configuration for things that should just be code
- Don't over-validate internal method arguments — validate at the API boundary, trust internally

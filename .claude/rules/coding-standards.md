# Coding Standards

## C# / .NET
- Use file-scoped namespaces
- Use primary constructors where supported
- Prefer `var` when the type is obvious from the right side
- Use collection expressions (`[1, 2, 3]`) over `new List<int> { 1, 2, 3 }`
- Use pattern matching over type checks + casts
- Use `sealed` on classes that aren't designed for inheritance
- Records for DTOs and value objects
- `readonly` on fields that don't change after construction
- Use `string.IsNullOrWhiteSpace()` over `== null || == ""`
- Use `is` for null checks (`if (obj is null)`) instead of `== null`
- Use `nameof()` instead of hardcoded strings for member references
- Use `required` keyword on properties that must be set at initialization
- Use `init` accessors for immutable-after-construction properties
- Use raw string literals (""") for multi-line strings and JSON
- Use `TimeProvider` over `DateTime.Now`/`DateTime.UtcNow` (testability)
- Use `IAsyncEnumerable<T>` for streaming large result sets
- Use `switch` expressions over `switch` statements where possible
- Use `ReadOnlySpan<T>` / `Memory<T>` for performance-sensitive string/array work
- Prefer `ValueTask<T>` over `Task<T>` in hot paths that often complete synchronously
- Use `[GeneratedRegex]` source generator over `new Regex()`
- Use `ArgumentException.ThrowIfNullOrEmpty()` over manual guard clauses
- Use target-typed `new()` when the type is clear from the left side
- Use `global using` in a single file for project-wide imports
- Use `const` over `static readonly` for compile-time constants

## Error Handling
- Result pattern for expected failures (validation, not found, conflict)
- Exceptions only for unexpected/unrecoverable errors
- Never catch `Exception` broadly — catch specific types
- Let framework handle unhandled exceptions via middleware

## Async
- Async all the way — never `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`
- Accept `CancellationToken` in all async methods
- Use `ConfigureAwait(false)` in library code, skip in application code
- Return `Task` not `void` for async methods (except event handlers)

## LINQ
- Prefer method syntax over query syntax
- Avoid multiple enumeration (materialize with `.ToList()` when needed)
- Use `.Any()` over `.Count() > 0`
- Keep LINQ chains readable — break long chains across lines

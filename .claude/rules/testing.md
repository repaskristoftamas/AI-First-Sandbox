# Testing Standards

## Framework & Libraries
- xUnit for test framework
- FluentAssertions for assertions (`.Should().Be(...)`)
- Moq for mocking
- InMemory EF Core DbContext for handler tests (preferred over mocking IApplicationDbContext)

## Test Structure
- Arrange-Act-Assert (AAA) pattern
- One test method = one scenario = one logical assertion
- Test naming: `MethodName_Scenario_ExpectedResult`
- Keep tests independent — no shared mutable state

## What to Test
- Public methods with logic (branching, calculations, transformations)
- Domain entities and value objects (invariants, business rules)
- Application handlers (command/query handlers)
- Validation rules
- Edge cases: null, empty, boundary values, duplicates

## What NOT to Test
- Private methods (test through public API)
- Simple property access
- Framework infrastructure (EF Core, ASP.NET pipeline)
- Third-party library behavior

## Test Quality
- Tests must be deterministic — no flaky tests
- Tests must be fast — mock external dependencies
- Tests should document behavior — a new developer should understand the feature from the tests
- Red-Green-Refactor: write failing test first when doing TDD

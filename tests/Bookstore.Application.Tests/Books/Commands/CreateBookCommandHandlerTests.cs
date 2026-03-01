using Xunit;
using Bookstore.Application.Books.Commands.CreateBook;
using Bookstore.Domain.Books;
using Bookstore.Infrastructure.Data;
using Bookstore.SharedKernel.Results;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace Bookstore.Application.Tests.Books.Commands;

public class CreateBookCommandHandlerTests : IDisposable
{
    private readonly BookstoreDbContext _context;
    private readonly CreateBookCommandHandler _handler;

    public CreateBookCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<BookstoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BookstoreDbContext(options, TimeProvider.System);
        _handler = new CreateBookCommandHandler(_context, new CreateBookCommandValidator(TimeProvider.System), TimeProvider.System);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithBookId_WhenBookIsCreated()
    {
        // Arrange
        var command = new CreateBookCommand("Clean Code", "Robert C. Martin", "978-0132350884", 35.99m, 2008);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenTitleIsEmpty()
    {
        // Arrange
        var command = new CreateBookCommand("", "Robert C. Martin", "978-0132350884", 35.99m, 2008);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenIsbnAlreadyExists()
    {
        // Arrange
        var command = new CreateBookCommand("Clean Code", "Robert C. Martin", "978-0132350884", 35.99m, 2008);
        await _handler.Handle(command, CancellationToken.None);

        var duplicateCommand = new CreateBookCommand("Clean Code 2nd Ed", "Robert C. Martin", "978-0132350884", 40m, 2020);

        // Act
        var result = await _handler.Handle(duplicateCommand, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ConflictError>();
    }

    [Fact]
    public async Task Handle_ShouldStampCreatedAt_WithFrozenTime()
    {
        // Arrange
        var frozenTime = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var fakeTimeProvider = new FakeTimeProvider(frozenTime);

        var options = new DbContextOptionsBuilder<BookstoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new BookstoreDbContext(options, fakeTimeProvider);
        var handler = new CreateBookCommandHandler(context, new CreateBookCommandValidator(fakeTimeProvider), fakeTimeProvider);

        var command = new CreateBookCommand("Clean Code", "Robert C. Martin", "978-0132350884", 35.99m, 2008);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var created = await context.Books.FindAsync(new BookId(result.Value));
        created!.CreatedAt.Should().Be(frozenTime);
    }

    public void Dispose() => _context.Dispose();
}

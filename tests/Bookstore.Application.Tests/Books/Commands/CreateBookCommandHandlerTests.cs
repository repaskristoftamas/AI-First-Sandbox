using Bookstore.Application.Books.Commands.CreateBook;
using Bookstore.Domain.Authors;
using Bookstore.Domain.Books;
using Bookstore.Infrastructure.Data;
using Bookstore.SharedKernel.Results;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Xunit;

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
        var author = SeedAuthor();
        var command = new CreateBookCommand("Clean Code", author.Id.Value, "978-0132350884", 35.99m, 2008);

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
        var author = SeedAuthor();
        var command = new CreateBookCommand("", author.Id.Value, "978-0132350884", 35.99m, 2008);

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
        var author = SeedAuthor();
        var command = new CreateBookCommand("Clean Code", author.Id.Value, "978-0132350884", 35.99m, 2008);
        await _handler.Handle(command, CancellationToken.None);

        var duplicateCommand = new CreateBookCommand("Clean Code 2nd Ed", author.Id.Value, "978-0132350884", 40m, 2020);

        // Act
        var result = await _handler.Handle(duplicateCommand, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ConflictError>();
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenAuthorDoesNotExist()
    {
        // Arrange
        var command = new CreateBookCommand("Clean Code", Guid.NewGuid(), "978-0132350884", 35.99m, 2008);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>()
            .Which.Code.Should().Be(BookErrorCodes.AuthorNotFound);
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

        var author = Author.Create("Robert", "Martin", new DateOnly(1952, 12, 5)).Value;
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        var command = new CreateBookCommand("Clean Code", author.Id.Value, "978-0132350884", 35.99m, 2008);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var created = await context.Books.FindAsync(new BookId(result.Value));
        created!.CreatedAt.Should().Be(frozenTime);
    }

    /// <summary>
    /// Creates and persists an author to satisfy the foreign key requirement.
    /// </summary>
    private Author SeedAuthor()
    {
        var author = Author.Create("Robert", "Martin", new DateOnly(1952, 12, 5)).Value;
        _context.Authors.Add(author);
        _context.SaveChanges();
        return author;
    }

    public void Dispose() => _context.Dispose();
}

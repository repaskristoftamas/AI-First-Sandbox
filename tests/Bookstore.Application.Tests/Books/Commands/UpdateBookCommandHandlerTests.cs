using Bookstore.Application.Books.Commands.UpdateBook;
using Bookstore.Domain.Authors;
using Bookstore.Domain.Books;
using Bookstore.Infrastructure.Data;
using Bookstore.SharedKernel.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Shouldly;
using Xunit;

namespace Bookstore.Application.Tests.Books.Commands;

public class UpdateBookCommandHandlerTests : IAsyncDisposable
{
    private readonly BookstoreDbContext _context;
    private readonly UpdateBookCommandHandler _handler;

    public UpdateBookCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<BookstoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BookstoreDbContext(options, TimeProvider.System, new Mock<IPublisher>().Object);
        _handler = new UpdateBookCommandHandler(_context, new UpdateBookCommandValidator(TimeProvider.System), TimeProvider.System);
    }

    [Fact]
    public async Task Handle_ShouldUpdateBook_WhenBookExists()
    {
        // Arrange
        var author = await SeedAuthor("Robert", "Martin");
        var newAuthor = await SeedAuthor("David", "Thomas");
        var book = Book.Create("Clean Code", author.Id, Isbn.Create("9780132350884").Value, 35.99m, 2008, TimeProvider.System).Value;
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var command = new UpdateBookCommand(book.Id, "The Pragmatic Programmer", newAuthor.Id.Value, "9780135957059", 45.99m, 1999);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var updated = await _context.Books.FindAsync(book.Id);
        updated!.Title.ShouldBe("The Pragmatic Programmer");
        updated.AuthorId.ShouldBe(newAuthor.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var author = await SeedAuthor("Robert", "Martin");
        var command = new UpdateBookCommand(BookId.New(), "Clean Code", author.Id.Value, "9780132350884", 35.99m, 2008);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<NotFoundError>();
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenIsbnAlreadyExistsOnAnotherBook()
    {
        // Arrange
        var author = await SeedAuthor("Robert", "Martin");
        var existingBook = Book.Create("Clean Code", author.Id, Isbn.Create("9780132350884").Value, 35.99m, 2008, TimeProvider.System).Value;
        var bookToUpdate = Book.Create("Refactoring", author.Id, Isbn.Create("9780201485677").Value, 49.99m, 1999, TimeProvider.System).Value;
        _context.Books.AddRange(existingBook, bookToUpdate);
        await _context.SaveChangesAsync();

        var command = new UpdateBookCommand(bookToUpdate.Id, "Refactoring 2nd Ed", author.Id.Value, "9780132350884", 49.99m, 2018);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ConflictError>();
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenTitleIsEmpty()
    {
        // Arrange
        var author = await SeedAuthor("Robert", "Martin");
        var command = new UpdateBookCommand(BookId.New(), "", author.Id.Value, "9780132350884", 35.99m, 2008);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenAuthorDoesNotExist()
    {
        // Arrange
        var author = await SeedAuthor("Robert", "Martin");
        var book = Book.Create("Clean Code", author.Id, Isbn.Create("9780132350884").Value, 35.99m, 2008, TimeProvider.System).Value;
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var command = new UpdateBookCommand(book.Id, "Clean Code", Guid.NewGuid(), "9780132350884", 35.99m, 2008);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<NotFoundError>()
            .Code.ShouldBe(BookErrorCodes.AuthorNotFound);
    }

    [Fact]
    public async Task Handle_ShouldStampUpdatedAt_WithFrozenTime()
    {
        // Arrange
        var createTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var updateTime = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var fakeTimeProvider = new FakeTimeProvider(createTime);

        var options = new DbContextOptionsBuilder<BookstoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new BookstoreDbContext(options, fakeTimeProvider, new Mock<IPublisher>().Object);
        var handler = new UpdateBookCommandHandler(context, new UpdateBookCommandValidator(fakeTimeProvider), fakeTimeProvider);

        var author = Author.Create("Robert", "Martin", new DateOnly(1952, 12, 5), TimeProvider.System).Value;
        context.Authors.Add(author);

        var book = Book.Create("Clean Code", author.Id, Isbn.Create("9780132350884").Value, 35.99m, 2008, fakeTimeProvider).Value;
        context.Books.Add(book);
        await context.SaveChangesAsync();

        fakeTimeProvider.SetUtcNow(updateTime);

        var command = new UpdateBookCommand(book.Id, "The Pragmatic Programmer", author.Id.Value, "9780135957059", 45.99m, 1999);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var updated = await context.Books.FindAsync(book.Id);
        updated!.UpdatedAt.ShouldBe(updateTime);
    }

    /// <summary>
    /// Creates and persists an author to satisfy the foreign key requirement.
    /// </summary>
    private async Task<Author> SeedAuthor(string firstName, string lastName)
    {
        var author = Author.Create(firstName, lastName, new DateOnly(1952, 12, 5), TimeProvider.System).Value;
        _context.Authors.Add(author);
        await _context.SaveChangesAsync();
        return author;
    }

    public async ValueTask DisposeAsync() => await _context.DisposeAsync();
}

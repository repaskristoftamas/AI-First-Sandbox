using Bookstore.Application.Books.Commands.UpdateBook;
using Bookstore.Domain.Books;
using Bookstore.Infrastructure.Data;
using Bookstore.SharedKernel.Results;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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

        _context = new BookstoreDbContext(options);
        _handler = new UpdateBookCommandHandler(_context, new UpdateBookCommandValidator());
    }

    [Fact]
    public async Task Handle_ShouldUpdateBook_WhenBookExists()
    {
        // Arrange
        var book = Book.Create("Clean Code", "Robert C. Martin", "978-0132350884", 35.99m, 2008).Value;
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var command = new UpdateBookCommand(book.Id, "The Pragmatic Programmer", "David Thomas", "978-0135957059", 45.99m, 1999);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updated = await _context.Books.FindAsync(book.Id);
        updated!.Title.Should().Be("The Pragmatic Programmer");
        updated.Author.Should().Be("David Thomas");
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var command = new UpdateBookCommand(BookId.New(), "Clean Code", "Robert C. Martin", "978-0132350884", 35.99m, 2008);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenIsbnAlreadyExistsOnAnotherBook()
    {
        // Arrange
        var existingBook = Book.Create("Clean Code", "Robert C. Martin", "978-0132350884", 35.99m, 2008).Value;
        var bookToUpdate = Book.Create("Refactoring", "Martin Fowler", "978-0201485677", 49.99m, 1999).Value;
        _context.Books.AddRange(existingBook, bookToUpdate);
        await _context.SaveChangesAsync();

        var command = new UpdateBookCommand(bookToUpdate.Id, "Refactoring 2nd Ed", "Martin Fowler", "978-0132350884", 49.99m, 2018);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ConflictError>();
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenTitleIsEmpty()
    {
        // Arrange
        var command = new UpdateBookCommand(BookId.New(), "", "Robert C. Martin", "978-0132350884", 35.99m, 2008);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
    }

    public async ValueTask DisposeAsync() => await _context.DisposeAsync();
}

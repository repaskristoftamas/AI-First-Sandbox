using Bookstore.Application.Books.Commands.DeleteBook;
using Bookstore.Domain.Authors;
using Bookstore.Domain.Books;
using Bookstore.Infrastructure.Data;
using Bookstore.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Bookstore.Application.Tests.Books.Commands;

public sealed class DeleteBookCommandHandlerTests : IAsyncDisposable
{
    private readonly BookstoreDbContext _context;
    private readonly DeleteBookCommandHandler _handler;

    public DeleteBookCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<BookstoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BookstoreDbContext(options, TimeProvider.System);
        _handler = new DeleteBookCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldDeleteBook_WhenBookExists()
    {
        // Arrange
        var author = await SeedAuthor();
        var book = Book.Create("Clean Code", author.Id, "9780132350884", 29.99m, 2008, TimeProvider.System).Value;
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var command = new DeleteBookCommand(book.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var deleted = await _context.Books.FindAsync(book.Id);
        deleted.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var command = new DeleteBookCommand(BookId.New());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<NotFoundError>();
    }

    /// <summary>
    /// Creates and persists an author to satisfy the foreign key requirement.
    /// </summary>
    private async Task<Author> SeedAuthor()
    {
        var author = Author.Create("Robert", "Martin", new DateOnly(1952, 12, 5)).Value;
        _context.Authors.Add(author);
        await _context.SaveChangesAsync();
        return author;
    }

    public async ValueTask DisposeAsync() => await _context.DisposeAsync();
}

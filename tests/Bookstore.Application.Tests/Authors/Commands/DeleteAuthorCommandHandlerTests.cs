using Bookstore.Application.Authors.Commands.DeleteAuthor;
using Bookstore.Domain.Authors;
using Bookstore.Domain.Books;
using Bookstore.Infrastructure.Data;
using Bookstore.SharedKernel.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;
using Xunit;

namespace Bookstore.Application.Tests.Authors.Commands;

public class DeleteAuthorCommandHandlerTests : IAsyncDisposable
{
    private readonly BookstoreDbContext _context;
    private readonly DeleteAuthorCommandHandler _handler;

    public DeleteAuthorCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<BookstoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BookstoreDbContext(options, TimeProvider.System, new Mock<IPublisher>().Object);
        _handler = new DeleteAuthorCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldDeleteAuthor_WhenAuthorExists()
    {
        // Arrange
        var author = Author.Create("Robert C.", "Martin", new DateOnly(1952, 12, 5), TimeProvider.System).Value;
        _context.Authors.Add(author);
        await _context.SaveChangesAsync();

        var command = new DeleteAuthorCommand(author.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var deleted = await _context.Authors.FindAsync(author.Id);
        deleted.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenAuthorDoesNotExist()
    {
        // Arrange
        var command = new DeleteAuthorCommand(AuthorId.New());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<NotFoundError>();
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenAuthorHasAssociatedBooks()
    {
        // Arrange
        var author = Author.Create("Robert C.", "Martin", new DateOnly(1952, 12, 5), TimeProvider.System).Value;
        _context.Authors.Add(author);

        var book = Book.Create("Clean Code", author.Id, Isbn.Create("9780132350884").Value, 29.99m, 2008, TimeProvider.System).Value;
        _context.Books.Add(book);

        await _context.SaveChangesAsync();

        var command = new DeleteAuthorCommand(author.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ConflictError>();
        result.Error.Code.ShouldBe(AuthorErrorCodes.HasAssociatedBooks);

        var authorStillExists = await _context.Authors.AnyAsync(a => a.Id == author.Id);
        authorStillExists.ShouldBeTrue();
    }

    public async ValueTask DisposeAsync() => await _context.DisposeAsync();
}

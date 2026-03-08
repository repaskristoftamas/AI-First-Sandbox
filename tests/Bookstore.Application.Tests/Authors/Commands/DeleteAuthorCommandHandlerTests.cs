using Bookstore.Application.Authors.Commands.DeleteAuthor;
using Bookstore.Domain.Authors;
using Bookstore.Infrastructure.Data;
using Bookstore.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
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

        _context = new BookstoreDbContext(options, TimeProvider.System);
        _handler = new DeleteAuthorCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldDeleteAuthor_WhenAuthorExists()
    {
        // Arrange
        var author = Author.Create("Robert C.", "Martin", new DateOnly(1952, 12, 5)).Value;
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

    public async ValueTask DisposeAsync() => await _context.DisposeAsync();
}

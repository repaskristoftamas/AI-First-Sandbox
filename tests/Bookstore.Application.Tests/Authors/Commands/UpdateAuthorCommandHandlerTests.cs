using Bookstore.Application.Authors.Commands.UpdateAuthor;
using Bookstore.Domain.Authors;
using Bookstore.Infrastructure.Data;
using Bookstore.SharedKernel.Results;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Bookstore.Application.Tests.Authors.Commands;

public class UpdateAuthorCommandHandlerTests : IAsyncDisposable
{
    private readonly BookstoreDbContext _context;
    private readonly UpdateAuthorCommandHandler _handler;

    public UpdateAuthorCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<BookstoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BookstoreDbContext(options);
        _handler = new UpdateAuthorCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldUpdateAuthor_WhenAuthorExists()
    {
        // Arrange
        var author = Author.Create("Robert C.", "Martin", new DateOnly(1952, 12, 5)).Value;
        _context.Authors.Add(author);
        await _context.SaveChangesAsync();

        var command = new UpdateAuthorCommand(author.Id, "Bob", "Uncle", new DateOnly(1952, 12, 5));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updated = await _context.Authors.FindAsync(author.Id);
        updated!.FirstName.Should().Be("Bob");
        updated.LastName.Should().Be("Uncle");
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenAuthorDoesNotExist()
    {
        // Arrange
        var command = new UpdateAuthorCommand(AuthorId.New(), "Bob", "Uncle", new DateOnly(1952, 12, 5));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenFirstNameIsEmpty()
    {
        // Arrange
        var author = Author.Create("Robert C.", "Martin", new DateOnly(1952, 12, 5)).Value;
        _context.Authors.Add(author);
        await _context.SaveChangesAsync();

        var command = new UpdateAuthorCommand(author.Id, "", "Uncle", new DateOnly(1952, 12, 5));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
    }

    public async ValueTask DisposeAsync() => await _context.DisposeAsync();
}

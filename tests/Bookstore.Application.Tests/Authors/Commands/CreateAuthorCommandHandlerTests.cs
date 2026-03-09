using Bookstore.Application.Authors.Commands.CreateAuthor;
using Bookstore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Bookstore.Application.Tests.Authors.Commands;

public class CreateAuthorCommandHandlerTests : IAsyncDisposable
{
    private readonly BookstoreDbContext _context;
    private readonly CreateAuthorCommandHandler _handler;

    public CreateAuthorCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<BookstoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BookstoreDbContext(options, TimeProvider.System);
        _handler = new CreateAuthorCommandHandler(_context, new CreateAuthorCommandValidator(TimeProvider.System), TimeProvider.System);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithAuthorId_WhenAuthorIsCreated()
    {
        // Arrange
        var command = new CreateAuthorCommand("Robert C.", "Martin", new DateOnly(1952, 12, 5));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ShouldPersistAuthorToDatabase_WhenAuthorIsCreated()
    {
        // Arrange
        var command = new CreateAuthorCommand("Martin", "Fowler", new DateOnly(1963, 12, 18));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var persisted = await _context.Authors.FindAsync(new Domain.Authors.AuthorId(result.Value));
        persisted.ShouldNotBeNull();
        persisted.FirstName.ShouldBe("Martin");
        persisted.LastName.ShouldBe("Fowler");
        persisted.DateOfBirth.ShouldBe(new DateOnly(1963, 12, 18));
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenFirstNameIsEmpty()
    {
        // Arrange
        var command = new CreateAuthorCommand("", "Martin", new DateOnly(1952, 12, 5));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<SharedKernel.Results.ValidationError>();
    }

    public async ValueTask DisposeAsync() => await _context.DisposeAsync();
}

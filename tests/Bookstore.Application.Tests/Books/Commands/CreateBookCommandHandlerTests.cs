using Xunit;
using Bookstore.Application.Books.Commands.CreateBook;
using Bookstore.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

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

        _context = new BookstoreDbContext(options);
        _handler = new CreateBookCommandHandler(_context);
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
        result.Error.Code.Should().Be("Book.Conflict");
    }

    public void Dispose() => _context.Dispose();
}

using Bookstore.Application.Authors.Queries.GetAuthorById;
using Bookstore.Domain.Authors;
using Bookstore.Infrastructure.Data;
using Bookstore.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Bookstore.Application.Tests.Authors.Queries;

public sealed class GetAuthorByIdQueryHandlerTests : IAsyncDisposable
{
    private readonly BookstoreDbContext _context;
    private readonly GetAuthorByIdQueryHandler _handler;

    public GetAuthorByIdQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<BookstoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BookstoreDbContext(options, TimeProvider.System);
        _handler = new GetAuthorByIdQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldReturnAuthorDto_WhenAuthorExists()
    {
        // Arrange
        var author = Author.Create("Robert", "Martin", new DateOnly(1952, 12, 5), TimeProvider.System).Value;
        _context.Authors.Add(author);
        await _context.SaveChangesAsync();

        var query = new GetAuthorByIdQuery(author.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(author.Id.Value);
        result.Value.FirstName.ShouldBe("Robert");
        result.Value.LastName.ShouldBe("Martin");
        result.Value.DateOfBirth.ShouldBe(new DateOnly(1952, 12, 5));
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenAuthorDoesNotExist()
    {
        // Arrange
        var query = new GetAuthorByIdQuery(AuthorId.New());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<NotFoundError>();
    }

    public async ValueTask DisposeAsync() => await _context.DisposeAsync();
}

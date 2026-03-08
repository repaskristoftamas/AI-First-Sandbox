using Bookstore.Application.Books.Queries.GetBookById;
using Bookstore.Domain.Authors;
using Bookstore.Domain.Books;
using Bookstore.Infrastructure.Data;
using Bookstore.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Bookstore.Application.Tests.Books.Queries;

public sealed class GetBookByIdQueryHandlerTests : IAsyncDisposable
{
    private readonly BookstoreDbContext _context;
    private readonly GetBookByIdQueryHandler _handler;

    public GetBookByIdQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<BookstoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BookstoreDbContext(options, TimeProvider.System);
        _handler = new GetBookByIdQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldReturnBookDto_WhenBookExists()
    {
        // Arrange
        var author = await SeedAuthor();
        var book = Book.Create("Clean Architecture", author.Id, "9780134494166", 39.99m, 2017, TimeProvider.System).Value;
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var query = new GetBookByIdQuery(book.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(book.Id.Value);
        result.Value.Title.ShouldBe("Clean Architecture");
        result.Value.AuthorId.ShouldBe(author.Id.Value);
        result.Value.ISBN.ShouldBe("9780134494166");
        result.Value.Price.ShouldBe(39.99m);
        result.Value.PublicationYear.ShouldBe(2017);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var query = new GetBookByIdQuery(BookId.New());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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

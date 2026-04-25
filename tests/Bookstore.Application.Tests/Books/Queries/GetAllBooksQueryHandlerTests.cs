using Bookstore.Application.Books.Queries.GetAllBooks;
using Bookstore.Domain.Authors;
using Bookstore.Domain.Books;
using Bookstore.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;
using Xunit;

namespace Bookstore.Application.Tests.Books.Queries;

public sealed class GetAllBooksQueryHandlerTests : IAsyncDisposable
{
    private readonly BookstoreDbContext _context;
    private readonly GetAllBooksQueryHandler _handler;

    public GetAllBooksQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<BookstoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BookstoreDbContext(options, TimeProvider.System, new Mock<IPublisher>().Object);
        _handler = new GetAllBooksQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldReturnFirstPage_WhenBooksExceedPageSize()
    {
        // Arrange
        var author = await SeedAuthor();
        await SeedBooks(author, count: 5);

        var query = new GetAllBooksQuery(Page: 1, PageSize: 3);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(3);
    }

    [Fact]
    public async Task Handle_ShouldReturnSecondPage_WhenPageTwoIsRequested()
    {
        // Arrange
        var author = await SeedAuthor();
        await SeedBooks(author, count: 5);

        var query = new GetAllBooksQuery(Page: 2, PageSize: 3);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyItems_WhenPageExceedsTotalBooks()
    {
        // Arrange
        var author = await SeedAuthor();
        await SeedBooks(author, count: 2);

        var query = new GetAllBooksQuery(Page: 5, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyItems_WhenNoBooksExist()
    {
        // Arrange
        var query = new GetAllBooksQuery(Page: 1, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginationMetadata_WhenMiddlePageIsRequested()
    {
        // Arrange
        var author = await SeedAuthor();
        await SeedBooks(author, count: 7);

        var query = new GetAllBooksQuery(Page: 2, PageSize: 3);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(7);
        result.Value.Page.ShouldBe(2);
        result.Value.PageSize.ShouldBe(3);
        result.Value.TotalPages.ShouldBe(3);
        result.Value.HasPreviousPage.ShouldBeTrue();
        result.Value.HasNextPage.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReportNoNeighbouringPages_WhenSinglePageFitsAllBooks()
    {
        // Arrange
        var author = await SeedAuthor();
        await SeedBooks(author, count: 2);

        var query = new GetAllBooksQuery(Page: 1, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(2);
        result.Value.TotalPages.ShouldBe(1);
        result.Value.HasPreviousPage.ShouldBeFalse();
        result.Value.HasNextPage.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReportZeroTotalPages_WhenNoBooksExist()
    {
        // Arrange
        var query = new GetAllBooksQuery(Page: 1, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(0);
        result.Value.TotalPages.ShouldBe(0);
        result.Value.HasPreviousPage.ShouldBeFalse();
        result.Value.HasNextPage.ShouldBeFalse();
    }

    /// <summary>
    /// Creates and persists an author to satisfy the foreign key requirement.
    /// </summary>
    private async Task<Author> SeedAuthor()
    {
        var author = Author.Create("Robert", "Martin", new DateOnly(1952, 12, 5), TimeProvider.System).Value;
        _context.Authors.Add(author);
        await _context.SaveChangesAsync();
        return author;
    }

    /// <summary>
    /// Seeds a given number of books with unique ISBNs for the specified author.
    /// </summary>
    private async Task SeedBooks(Author author, int count)
    {
        for (var i = 0; i < count; i++)
        {
            var isbn = GenerateTestIsbn(i);
            var book = Book.Create($"Book {i}", author.Id, isbn, 10m + i, 2000 + i, TimeProvider.System).Value;
            _context.Books.Add(book);
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Generates a valid ISBN-13 with a correct check digit for the given index.
    /// </summary>
    private static Isbn GenerateTestIsbn(int index)
    {
        var prefix = $"97800000{index:D4}";
        var sum = 0;
        for (var i = 0; i < 12; i++)
        {
            var digit = prefix[i] - '0';
            sum += i % 2 == 0 ? digit : digit * 3;
        }

        var check = (10 - sum % 10) % 10;
        return Isbn.Create($"{prefix}{check}").Value;
    }

    public async ValueTask DisposeAsync() => await _context.DisposeAsync();
}

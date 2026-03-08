using Bookstore.Application.Authors.Queries.GetAllAuthors;
using Bookstore.Domain.Authors;
using Bookstore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Bookstore.Application.Tests.Authors.Queries;

public sealed class GetAllAuthorsQueryHandlerTests : IAsyncDisposable
{
    private readonly BookstoreDbContext _context;
    private readonly GetAllAuthorsQueryHandler _handler;

    public GetAllAuthorsQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<BookstoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BookstoreDbContext(options, TimeProvider.System);
        _handler = new GetAllAuthorsQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldReturnFirstPage_WhenAuthorsExceedPageSize()
    {
        // Arrange
        await SeedAuthors(count: 5);

        var query = new GetAllAuthorsQuery(Page: 1, PageSize: 3);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(3);
    }

    [Fact]
    public async Task Handle_ShouldReturnSecondPage_WhenPageTwoIsRequested()
    {
        // Arrange
        await SeedAuthors(count: 5);

        var query = new GetAllAuthorsQuery(Page: 2, PageSize: 3);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenPageExceedsTotalAuthors()
    {
        // Arrange
        await SeedAuthors(count: 2);

        var query = new GetAllAuthorsQuery(Page: 5, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoAuthorsExist()
    {
        // Arrange
        var query = new GetAllAuthorsQuery(Page: 1, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    /// <summary>
    /// Seeds a given number of authors with unique names.
    /// </summary>
    private async Task SeedAuthors(int count)
    {
        for (var i = 0; i < count; i++)
        {
            var author = Author.Create($"First{i}", $"Last{i}", new DateOnly(1950 + i, 1, 1)).Value;
            _context.Authors.Add(author);
        }

        await _context.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync() => await _context.DisposeAsync();
}

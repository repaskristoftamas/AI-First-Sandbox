using Bookstore.Application.Authors.Queries.GetAllAuthors;
using Bookstore.Domain.Authors;
using Bookstore.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Moq;
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

        _context = new BookstoreDbContext(options, TimeProvider.System, new Mock<IPublisher>().Object);
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
        result.Value.Items.Count.ShouldBe(3);
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
        result.Value.Items.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyItems_WhenPageExceedsTotalAuthors()
    {
        // Arrange
        await SeedAuthors(count: 2);

        var query = new GetAllAuthorsQuery(Page: 5, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyItems_WhenNoAuthorsExist()
    {
        // Arrange
        var query = new GetAllAuthorsQuery(Page: 1, PageSize: 20);

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
        await SeedAuthors(count: 7);

        var query = new GetAllAuthorsQuery(Page: 2, PageSize: 3);

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
    public async Task Handle_ShouldReportNoNeighbouringPages_WhenSinglePageFitsAllAuthors()
    {
        // Arrange
        await SeedAuthors(count: 2);

        var query = new GetAllAuthorsQuery(Page: 1, PageSize: 20);

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
    public async Task Handle_ShouldReportZeroTotalPages_WhenNoAuthorsExist()
    {
        // Arrange
        var query = new GetAllAuthorsQuery(Page: 1, PageSize: 20);

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
    /// Seeds a given number of authors with unique names.
    /// </summary>
    private async Task SeedAuthors(int count)
    {
        for (var i = 0; i < count; i++)
        {
            var author = Author.Create($"First{i}", $"Last{i}", new DateOnly(1950 + i, 1, 1), TimeProvider.System).Value;
            _context.Authors.Add(author);
        }

        await _context.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync() => await _context.DisposeAsync();
}

using Bookstore.Application.Authors.Specifications;
using Bookstore.Domain.Authors;
using Bookstore.SharedKernel.Specifications;
using Shouldly;
using Xunit;

namespace Bookstore.Application.Tests.Authors.Specifications;

public sealed class PaginatedAuthorsSpecificationTests
{
    [Fact]
    public void Apply_ShouldReturnFirstPage_WhenMultipleAuthorsExist()
    {
        // Arrange
        var authors = CreateAuthors(5);
        var specification = new PaginatedAuthorsSpecification(page: 1, pageSize: 3);

        // Act
        var result = SpecificationEvaluator.Apply(authors.AsQueryable(), specification).ToList();

        // Assert
        result.Count.ShouldBe(3);
    }

    [Fact]
    public void Apply_ShouldReturnSecondPage_WhenPageTwoIsRequested()
    {
        // Arrange
        var authors = CreateAuthors(5);
        var specification = new PaginatedAuthorsSpecification(page: 2, pageSize: 3);

        // Act
        var result = SpecificationEvaluator.Apply(authors.AsQueryable(), specification).ToList();

        // Assert
        result.Count.ShouldBe(2);
    }

    [Fact]
    public void Apply_ShouldReturnEmpty_WhenPageExceedsTotalAuthors()
    {
        // Arrange
        var authors = CreateAuthors(2);
        var specification = new PaginatedAuthorsSpecification(page: 5, pageSize: 20);

        // Act
        var result = SpecificationEvaluator.Apply(authors.AsQueryable(), specification).ToList();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Apply_ShouldOrderAuthorsByCreatedAt()
    {
        // Arrange
        var authors = CreateAuthors(3);
        var baseTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        authors[0].CreatedAt = baseTime.AddDays(2);
        authors[1].CreatedAt = baseTime;
        authors[2].CreatedAt = baseTime.AddDays(1);
        var specification = new PaginatedAuthorsSpecification(page: 1, pageSize: 10);

        // Act
        var result = SpecificationEvaluator.Apply(authors.AsQueryable(), specification).ToList();

        // Assert
        result[0].ShouldBe(authors[1]);
        result[1].ShouldBe(authors[2]);
        result[2].ShouldBe(authors[0]);
    }

    /// <summary>
    /// Creates a list of valid authors for in-memory specification testing.
    /// </summary>
    private static List<Author> CreateAuthors(int count)
    {
        var authors = new List<Author>();

        for (var i = 0; i < count; i++)
        {
            var author = Author.Create($"First{i}", $"Last{i}", new DateOnly(1950 + i, 1, 1), TimeProvider.System).Value;
            authors.Add(author);
        }

        return authors;
    }
}

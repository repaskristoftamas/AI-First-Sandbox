using Bookstore.Application.Books.Specifications;
using Bookstore.Domain.Authors;
using Bookstore.Domain.Books;
using Bookstore.SharedKernel.Specifications;
using Shouldly;
using Xunit;

namespace Bookstore.Application.Tests.Books.Specifications;

public sealed class PaginatedBooksSpecificationTests
{
    [Fact]
    public void Apply_ShouldReturnFirstPage_WhenMultipleBooksExist()
    {
        // Arrange
        var books = CreateBooks(5);
        var specification = new PaginatedBooksSpecification(page: 1, pageSize: 3);

        // Act
        var result = SpecificationEvaluator.Apply(books.AsQueryable(), specification).ToList();

        // Assert
        result.Count.ShouldBe(3);
    }

    [Fact]
    public void Apply_ShouldReturnSecondPage_WhenPageTwoIsRequested()
    {
        // Arrange
        var books = CreateBooks(5);
        var specification = new PaginatedBooksSpecification(page: 2, pageSize: 3);

        // Act
        var result = SpecificationEvaluator.Apply(books.AsQueryable(), specification).ToList();

        // Assert
        result.Count.ShouldBe(2);
    }

    [Fact]
    public void Apply_ShouldReturnEmpty_WhenPageExceedsTotalBooks()
    {
        // Arrange
        var books = CreateBooks(2);
        var specification = new PaginatedBooksSpecification(page: 5, pageSize: 20);

        // Act
        var result = SpecificationEvaluator.Apply(books.AsQueryable(), specification).ToList();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Apply_ShouldOrderBooksById()
    {
        // Arrange
        var books = CreateBooks(3);
        var specification = new PaginatedBooksSpecification(page: 1, pageSize: 10);

        // Act
        var result = SpecificationEvaluator.Apply(books.AsQueryable(), specification).ToList();

        // Assert
        var ids = result.Select(b => b.Id.Value).ToList();
        ids.ShouldBe(ids.OrderBy(id => id).ToList());
    }

    /// <summary>
    /// Creates a list of valid books for in-memory specification testing.
    /// </summary>
    private static List<Book> CreateBooks(int count)
    {
        var authorId = AuthorId.New();
        var books = new List<Book>();

        for (var i = 0; i < count; i++)
        {
            var isbn = GenerateTestIsbn(i);
            var book = Book.Create($"Book {i}", authorId, isbn, 10m + i, 2000 + i, TimeProvider.System).Value;
            books.Add(book);
        }

        return books;
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
}

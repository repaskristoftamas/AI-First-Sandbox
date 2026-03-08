using Bookstore.Domain.Authors;
using Bookstore.Infrastructure.Data;

namespace Bookstore.Application.Tests.Helpers;

/// <summary>
/// Provides shared seed methods for test data setup across handler tests.
/// </summary>
internal static class TestDataSeeder
{
    /// <summary>
    /// Creates and persists an author to satisfy the foreign key requirement.
    /// </summary>
    internal static async Task<Author> SeedAuthorAsync( //TODO: check where to use this method over current references
        BookstoreDbContext context,
        string firstName = "Robert",
        string lastName = "Martin",
        int birthYear = 1952,
        int birthMonth = 12,
        int birthDay = 5)
    {
        var author = Author.Create(firstName, lastName, new DateOnly(birthYear, birthMonth, birthDay)).Value;
        context.Authors.Add(author);
        await context.SaveChangesAsync();
        return author;
    }
}

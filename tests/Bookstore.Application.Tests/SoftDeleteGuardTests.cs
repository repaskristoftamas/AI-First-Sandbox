using Bookstore.Domain.Authors;
using Bookstore.Domain.Books;
using Bookstore.Domain.Users;
using Bookstore.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;
using Xunit;

namespace Bookstore.Application.Tests;

/// <summary>
/// Verifies that <see cref="BookstoreDbContext"/> blocks hard deletion of soft-deletable entities.
/// </summary>
public sealed class SoftDeleteGuardTests : IAsyncDisposable
{
    private readonly BookstoreDbContext _context;

    public SoftDeleteGuardTests()
    {
        var options = new DbContextOptionsBuilder<BookstoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BookstoreDbContext(options, TimeProvider.System, new Mock<IPublisher>().Object);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldThrow_WhenAuthorIsHardDeleted()
    {
        // Arrange
        var author = Author.Create("Robert C.", "Martin", new DateOnly(1952, 12, 5), TimeProvider.System).Value;
        _context.Authors.Add(author);
        await _context.SaveChangesAsync();

        // Act
        _context.Authors.Remove(author);

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(() => _context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldThrow_WhenBookIsHardDeleted()
    {
        // Arrange
        var author = Author.Create("Robert C.", "Martin", new DateOnly(1952, 12, 5), TimeProvider.System).Value;
        _context.Authors.Add(author);

        var book = Book.Create("Clean Code", author.Id, Isbn.Create("9780132350884").Value, 29.99m, 2008, TimeProvider.System).Value;
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        // Act
        _context.Books.Remove(book);

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(() => _context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldThrow_WhenUserIsHardDeleted()
    {
        // Arrange
        var user = User.Create("john@example.com", "hashed-password", [Role.User]).Value;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        _context.Users.Remove(user);

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(() => _context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldSucceed_WhenEntityIsSoftDeletedViaDomainMethod()
    {
        // Arrange
        var author = Author.Create("Robert C.", "Martin", new DateOnly(1952, 12, 5), TimeProvider.System).Value;
        _context.Authors.Add(author);
        await _context.SaveChangesAsync();

        // Act
        author.Delete(TimeProvider.System);
        await _context.SaveChangesAsync();

        // Assert
        var softDeleted = await _context.Authors
            .IgnoreQueryFilters()
            .FirstAsync(a => a.Id == author.Id);
        softDeleted.IsDeleted.ShouldBeTrue();
    }

    public async ValueTask DisposeAsync() => await _context.DisposeAsync();
}

using Bookstore.Domain.Authors;
using Bookstore.Domain.Books;
using Bookstore.Domain.Users;
using Bookstore.Infrastructure.Data;
using Bookstore.Infrastructure.Retention;
using Mediator;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Shouldly;
using Xunit;

namespace Bookstore.Application.Tests.Retention;

/// <summary>
/// Verifies that <see cref="RetentionPurgeService"/> permanently deletes soft-deleted records
/// older than the retention period while respecting foreign key ordering.
/// </summary>
/// <remarks>
/// Uses SQLite in-memory rather than the EF Core InMemory provider because InMemory does not
/// support <c>ExecuteDeleteAsync</c>. SQLite also enforces the <c>Book → Author</c> foreign key,
/// giving us a true assertion of the FK ordering requirement.
/// </remarks>
public sealed class RetentionPurgeServiceTests : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly BookstoreDbContext _context;
    private readonly FakeTimeProvider _timeProvider;
    private readonly RetentionOptions _options;
    private readonly RetentionPurgeService _service;

    public RetentionPurgeServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var dbOptions = new DbContextOptionsBuilder<BookstoreDbContext>()
            .UseSqlite(_connection)
            .Options;

        _timeProvider = new FakeTimeProvider(startDateTime: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        _context = new BookstoreDbContext(dbOptions, _timeProvider, new Mock<IPublisher>().Object);
        _context.Database.EnsureCreated();

        _options = new RetentionOptions
        {
            RetentionPeriod = TimeSpan.FromDays(30),
            SweepInterval = TimeSpan.FromHours(1)
        };

        _service = new RetentionPurgeService(
            _context,
            Options.Create(_options),
            _timeProvider,
            NullLogger<RetentionPurgeService>.Instance);
    }

    [Fact]
    public async Task PurgeAsync_ShouldPermanentlyDeleteRecords_WhenDeletedBeyondRetention()
    {
        // Arrange
        var (author, book) = await SeedAuthorWithBookAsync();
        var user = await SeedUserAsync("expired@example.com");

        book.Delete(_timeProvider);
        author.Delete(_timeProvider);
        user.Delete(_timeProvider);
        await _context.SaveChangesAsync();

        _timeProvider.Advance(TimeSpan.FromDays(31));

        // Act
        var result = await _service.PurgeAsync(CancellationToken.None);

        // Assert
        result.BooksPurged.ShouldBe(1);
        result.AuthorsPurged.ShouldBe(1);
        result.UsersPurged.ShouldBe(1);

        (await _context.Books.IgnoreQueryFilters().CountAsync()).ShouldBe(0);
        (await _context.Authors.IgnoreQueryFilters().CountAsync()).ShouldBe(0);
        (await _context.Users.IgnoreQueryFilters().CountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task PurgeAsync_ShouldNotDeleteRecords_WhenWithinRetentionPeriod()
    {
        // Arrange
        var (_, book) = await SeedAuthorWithBookAsync();
        var user = await SeedUserAsync("recent@example.com");

        book.Delete(_timeProvider);
        user.Delete(_timeProvider);
        await _context.SaveChangesAsync();

        _timeProvider.Advance(TimeSpan.FromDays(29));

        // Act
        var result = await _service.PurgeAsync(CancellationToken.None);

        // Assert
        result.BooksPurged.ShouldBe(0);
        result.UsersPurged.ShouldBe(0);
        (await _context.Books.IgnoreQueryFilters().CountAsync()).ShouldBe(1);
        (await _context.Users.IgnoreQueryFilters().CountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task PurgeAsync_ShouldNotDeleteActiveRecords_WhenNotSoftDeleted()
    {
        // Arrange
        await SeedAuthorWithBookAsync();
        await SeedUserAsync("active@example.com");

        _timeProvider.Advance(TimeSpan.FromDays(365));

        // Act
        var result = await _service.PurgeAsync(CancellationToken.None);

        // Assert
        result.BooksPurged.ShouldBe(0);
        result.AuthorsPurged.ShouldBe(0);
        result.UsersPurged.ShouldBe(0);
        (await _context.Books.IgnoreQueryFilters().CountAsync()).ShouldBe(1);
        (await _context.Authors.IgnoreQueryFilters().CountAsync()).ShouldBe(1);
        (await _context.Users.IgnoreQueryFilters().CountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task PurgeAsync_ShouldPurgeBooksBeforeAuthors_WhenBothAreExpired()
    {
        // Arrange — Book references Author with DeleteBehavior.Restrict. If the service purged
        // authors first, SQLite's FK enforcement would fail. This test proves the ordering.
        var (author, book) = await SeedAuthorWithBookAsync();

        book.Delete(_timeProvider);
        author.Delete(_timeProvider);
        await _context.SaveChangesAsync();

        _timeProvider.Advance(TimeSpan.FromDays(31));

        // Act
        var result = await _service.PurgeAsync(CancellationToken.None);

        // Assert
        result.BooksPurged.ShouldBe(1);
        result.AuthorsPurged.ShouldBe(1);
        (await _context.Books.IgnoreQueryFilters().CountAsync()).ShouldBe(0);
        (await _context.Authors.IgnoreQueryFilters().CountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task PurgeAsync_ShouldOnlyDeleteExpiredRecords_WhenMixedAgesExist()
    {
        // Arrange
        var oldUser = await SeedUserAsync("old@example.com");
        oldUser.Delete(_timeProvider);
        await _context.SaveChangesAsync();

        _timeProvider.Advance(TimeSpan.FromDays(31));

        var recentUser = await SeedUserAsync("recent@example.com");
        recentUser.Delete(_timeProvider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.PurgeAsync(CancellationToken.None);

        // Assert
        result.UsersPurged.ShouldBe(1);
        var remaining = await _context.Users.IgnoreQueryFilters().ToListAsync();
        remaining.Count.ShouldBe(1);
        remaining[0].Email.ShouldBe("recent@example.com");
    }

    private async Task<(Author Author, Book Book)> SeedAuthorWithBookAsync()
    {
        var author = Author.Create("Robert", "Martin", new DateOnly(1952, 12, 5), _timeProvider).Value;
        _context.Authors.Add(author);
        var book = Book.Create("Clean Code", author.Id, Isbn.Create("9780132350884").Value, 29.99m, 2008, _timeProvider).Value;
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        return (author, book);
    }

    private async Task<User> SeedUserAsync(string email)
    {
        var user = User.Create(email, "hashed-password", [Role.User]).Value;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}

namespace Bookstore.Infrastructure.Retention;

/// <summary>
/// Number of records permanently deleted per entity type during a single retention sweep.
/// </summary>
/// <param name="BooksPurged">Number of books permanently deleted.</param>
/// <param name="AuthorsPurged">Number of authors permanently deleted.</param>
/// <param name="UsersPurged">Number of users permanently deleted.</param>
public readonly record struct RetentionPurgeResult(int BooksPurged, int AuthorsPurged, int UsersPurged);

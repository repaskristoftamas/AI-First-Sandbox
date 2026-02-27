namespace Bookstore.WebApi.Endpoints.Authors;

/// <summary>
/// API response model representing an author returned to the client.
/// </summary>
/// <param name="Id">Unique identifier of the author.</param>
/// <param name="FirstName">First name of the author.</param>
/// <param name="LastName">Last name of the author.</param>
/// <param name="DateOfBirth">Date of birth of the author.</param>
/// <param name="CreatedAt">Timestamp when the author was added to the catalog.</param>
/// <param name="UpdatedAt">Timestamp of the last update, or <c>null</c> if never updated.</param>
public sealed record AuthorResponse(
    Guid Id,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

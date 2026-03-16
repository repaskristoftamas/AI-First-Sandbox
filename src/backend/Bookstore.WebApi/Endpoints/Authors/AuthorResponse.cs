namespace Bookstore.WebApi.Endpoints.Authors;

/// <summary>
/// API response model representing an author returned to the client.
/// </summary>
/// <param name="Id" example="a1b2c3d4-e5f6-7890-abcd-ef1234567890">Unique identifier of the author.</param>
/// <param name="FirstName" example="Harper">First name of the author.</param>
/// <param name="LastName" example="Lee">Last name of the author.</param>
/// <param name="DateOfBirth" example="1926-04-28">Date of birth of the author.</param>
/// <param name="CreatedAt" example="2024-01-15T10:30:00+00:00">Timestamp when the author was added to the catalog.</param>
/// <param name="UpdatedAt" example="2024-06-20T14:45:00+00:00">Timestamp of the last update, or <c>null</c> if never updated.</param>
public sealed record AuthorResponse(
    Guid Id,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

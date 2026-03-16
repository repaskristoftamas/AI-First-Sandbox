namespace Bookstore.WebApi.Endpoints.Authors;

/// <summary>
/// API request model for creating a new author in the catalog.
/// </summary>
/// <param name="FirstName" example="Harper">First name of the author.</param>
/// <param name="LastName" example="Lee">Last name of the author.</param>
/// <param name="DateOfBirth" example="1926-04-28">Date of birth of the author.</param>
public sealed record CreateAuthorRequest(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth);

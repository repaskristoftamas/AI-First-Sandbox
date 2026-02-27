using System.ComponentModel.DataAnnotations;

namespace Bookstore.WebApi.Endpoints.Authors;

/// <summary>
/// API request model for updating an existing author's properties.
/// </summary>
/// <param name="FirstName">First name of the author.</param>
/// <param name="LastName">Last name of the author.</param>
/// <param name="DateOfBirth">Date of birth of the author.</param>
public sealed record UpdateAuthorRequest(
    [Required, StringLength(100, MinimumLength = 1)] string FirstName,
    [Required, StringLength(100, MinimumLength = 1)] string LastName,
    DateOnly DateOfBirth);

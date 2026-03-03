using System.Text.RegularExpressions;
using Bookstore.Domain.Books;
using FluentValidation;

namespace Bookstore.Application.Books.Commands.CreateBook;

/// <summary>
/// Validates a <see cref="CreateBookCommand"/> before it reaches the handler.
/// </summary>
public sealed partial class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    /// <summary>Initializes the validation rules.</summary>
    /// <param name="timeProvider">Provides the current date for publication year validation.</param>
    public CreateBookCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithErrorCode(BookErrorCodes.TitleRequired)
            .MaximumLength(250).WithErrorCode(BookErrorCodes.TitleTooLong);

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithErrorCode(BookErrorCodes.AuthorRequired);

        RuleFor(x => x.ISBN)
            .NotEmpty().WithErrorCode(BookErrorCodes.IsbnRequired)
            .MaximumLength(13).WithErrorCode(BookErrorCodes.IsbnTooLong)
            .Matches(Isbn13Regex()).WithErrorCode(BookErrorCodes.IsbnInvalidFormat);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithErrorCode(BookErrorCodes.PriceInvalid);

        RuleFor(x => x.PublicationYear)
            .InclusiveBetween(1450, timeProvider.GetUtcNow().Year)
            .WithErrorCode(BookErrorCodes.PublicationYearInvalid);
    }

    /// <summary>
    /// Matches a valid ISBN-13: exactly 13 digits starting with 978 or 979.
    /// </summary>
    [GeneratedRegex(@"^(978|979)\d{10}$")]
    private static partial Regex Isbn13Regex();
}

using Bookstore.Domain.Books;
using FluentValidation;

namespace Bookstore.Application.Books.Commands.UpdateBook;

/// <summary>
/// Validates an <see cref="UpdateBookCommand"/> before it reaches the handler.
/// </summary>
public sealed class UpdateBookCommandValidator : AbstractValidator<UpdateBookCommand>
{
    /// <summary>Initializes the validation rules.</summary>
    /// <param name="timeProvider">Provides the current date for publication year validation.</param>
    public UpdateBookCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithErrorCode(BookErrorCodes.TitleRequired)
            .MaximumLength(250).WithErrorCode(BookErrorCodes.TitleTooLong);

        RuleFor(x => x.Author)
            .NotEmpty().WithErrorCode(BookErrorCodes.AuthorRequired)
            .MaximumLength(200).WithErrorCode(BookErrorCodes.AuthorTooLong);

        RuleFor(x => x.ISBN)
            .NotEmpty().WithErrorCode(BookErrorCodes.IsbnRequired)
            .MaximumLength(20).WithErrorCode(BookErrorCodes.IsbnTooLong);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithErrorCode(BookErrorCodes.PriceInvalid);

        RuleFor(x => x.PublicationYear)
            .InclusiveBetween(1450, timeProvider.GetLocalNow().Year)
            .WithErrorCode(BookErrorCodes.PublicationYearInvalid);
    }
}

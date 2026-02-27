using FluentValidation;

namespace Bookstore.Application.Books.Commands.UpdateBook;

/// <summary>
/// Validates an <see cref="UpdateBookCommand"/> before it reaches the handler.
/// </summary>
public sealed class UpdateBookCommandValidator : AbstractValidator<UpdateBookCommand>
{
    /// <summary>Initializes the validation rules.</summary>
    public UpdateBookCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(250);

        RuleFor(x => x.Author)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ISBN)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.PublicationYear)
            .InclusiveBetween(1450, DateTime.Today.Year);
    }
}

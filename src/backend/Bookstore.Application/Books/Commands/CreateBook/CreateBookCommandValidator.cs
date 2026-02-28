using FluentValidation;

namespace Bookstore.Application.Books.Commands.CreateBook;

/// <summary>
/// Validates a <see cref="CreateBookCommand"/> before it reaches the handler.
/// </summary>
public sealed class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    /// <summary>Initializes the validation rules.</summary>
    /// <param name="timeProvider">Provides the current date for publication year validation.</param>
    public CreateBookCommandValidator(TimeProvider timeProvider)
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
            .InclusiveBetween(1450, timeProvider.GetLocalNow().Year);
    }
}

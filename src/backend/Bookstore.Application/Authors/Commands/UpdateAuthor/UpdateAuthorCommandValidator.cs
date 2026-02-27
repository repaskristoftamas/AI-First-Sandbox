using FluentValidation;

namespace Bookstore.Application.Authors.Commands.UpdateAuthor;

/// <summary>
/// Validates an <see cref="UpdateAuthorCommand"/> before it reaches the handler.
/// </summary>
public sealed class UpdateAuthorCommandValidator : AbstractValidator<UpdateAuthorCommand>
{
    /// <summary>Initializes the validation rules.</summary>
    public UpdateAuthorCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Date of birth must be in the past.");
    }
}

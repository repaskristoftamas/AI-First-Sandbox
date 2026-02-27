using FluentValidation;

namespace Bookstore.Application.Authors.Commands.CreateAuthor;

/// <summary>
/// Validates a <see cref="CreateAuthorCommand"/> before it reaches the handler.
/// </summary>
public sealed class CreateAuthorCommandValidator : AbstractValidator<CreateAuthorCommand>
{
    /// <summary>Initializes the validation rules.</summary>
    public CreateAuthorCommandValidator()
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

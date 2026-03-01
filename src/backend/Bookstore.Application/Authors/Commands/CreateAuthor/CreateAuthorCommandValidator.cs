using Bookstore.Domain.Authors;
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
            .NotEmpty().WithErrorCode(AuthorErrorCodes.FirstNameRequired)
            .MaximumLength(100).WithErrorCode(AuthorErrorCodes.FirstNameTooLong);

        RuleFor(x => x.LastName)
            .NotEmpty().WithErrorCode(AuthorErrorCodes.LastNameRequired)
            .MaximumLength(100).WithErrorCode(AuthorErrorCodes.LastNameTooLong);

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Date of birth must be in the past.")
            .WithErrorCode(AuthorErrorCodes.DobInFuture);
    }
}

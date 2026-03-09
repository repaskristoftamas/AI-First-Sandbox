using Bookstore.Domain.Authors;
using FluentValidation;

namespace Bookstore.Application.Authors.Commands.CreateAuthor;

/// <summary>
/// Validates a <see cref="CreateAuthorCommand"/> before it reaches the handler.
/// </summary>
public sealed class CreateAuthorCommandValidator : AbstractValidator<CreateAuthorCommand>
{
    /// <summary>Initializes the validation rules.</summary>
    /// <param name="timeProvider">Provides the current date for date-of-birth validation.</param>
    public CreateAuthorCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithErrorCode(AuthorErrorCodes.FirstNameRequired)
            .MaximumLength(100).WithErrorCode(AuthorErrorCodes.FirstNameTooLong);

        RuleFor(x => x.LastName)
            .NotEmpty().WithErrorCode(AuthorErrorCodes.LastNameRequired)
            .MaximumLength(100).WithErrorCode(AuthorErrorCodes.LastNameTooLong);

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime))
            .WithMessage("Date of birth must be in the past.")
            .WithErrorCode(AuthorErrorCodes.DobInFuture);
    }
}

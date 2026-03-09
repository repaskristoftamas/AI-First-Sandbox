using Bookstore.Domain.Authors;
using FluentValidation;

namespace Bookstore.Application.Authors.Commands.UpdateAuthor;

/// <summary>
/// Validates an <see cref="UpdateAuthorCommand"/> before it reaches the handler.
/// </summary>
public sealed class UpdateAuthorCommandValidator : AbstractValidator<UpdateAuthorCommand>
{
    /// <summary>Initializes the validation rules.</summary>
    /// <param name="timeProvider">Provides the current date for date-of-birth validation.</param>
    public UpdateAuthorCommandValidator(TimeProvider timeProvider)
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

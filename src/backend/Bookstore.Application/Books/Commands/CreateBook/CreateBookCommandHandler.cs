using Bookstore.Application.Abstractions;
using Bookstore.Domain.Books;
using Bookstore.SharedKernel.Results;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Books.Commands.CreateBook;

/// <summary>
/// Handles creation of a new book.
/// </summary>
/// <remarks>
/// Enforces ISBN uniqueness before persisting.
/// </remarks>
internal sealed class CreateBookCommandHandler(
    IApplicationDbContext context,
    IValidator<CreateBookCommand> validator) : ICommandHandler<CreateBookCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;

    /// <summary>
    /// Creates a new book and returns its identifier.
    /// </summary>
    /// <remarks>
    /// Validates that the ISBN is not already in use before persisting the book.
    /// </remarks>
    /// <param name="command">The command containing the book details to create.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A result containing the new book's identifier, or a <see cref="ConflictError"/> if the ISBN is taken.</returns>
    public async ValueTask<Result<Guid>> Handle(CreateBookCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var failures = validationResult.Errors;
            return Result.Failure<Guid>(new ValidationError(string.Join("; ", failures.Select(f => f.ErrorMessage))));
        }

        bool isbnExists = await _context.Books
            .AnyAsync(b => b.ISBN == command.ISBN, cancellationToken);

        if (isbnExists)
            return Result.Failure<Guid>(new ConflictError($"A book with ISBN '{command.ISBN}' already exists."));

        var createResult = Book.Create(
            command.Title,
            command.Author,
            command.ISBN,
            command.Price,
            command.PublicationYear);

        if (createResult.IsFailure)
            return Result.Failure<Guid>(createResult.Error);

        var book = createResult.Value;
        _context.Books.Add(book);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(book.Id.Value);
    }
}

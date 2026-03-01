using Bookstore.Application.Abstractions;
using Bookstore.Application.Extensions;
using Bookstore.Domain.Authors;
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
/// Enforces author existence and ISBN uniqueness before persisting.
/// </remarks>
internal sealed class CreateBookCommandHandler(
    IApplicationDbContext context,
    IValidator<CreateBookCommand> validator,
    TimeProvider timeProvider) : ICommandHandler<CreateBookCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IValidator<CreateBookCommand> _validator = validator;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>
    /// Creates a new book and returns its identifier.
    /// </summary>
    /// <remarks>
    /// Validates that the author exists and the ISBN is not already in use before persisting the book.
    /// </remarks>
    /// <param name="command">The command containing the book details to create.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A result containing the new book's identifier, or a failure on validation/conflict errors.</returns>
    public async ValueTask<Result<Guid>> Handle(CreateBookCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToFailureResult<Guid>();

        var authorId = new AuthorId(command.AuthorId);

        bool authorExists = await _context.Authors
            .AnyAsync(a => a.Id == authorId, cancellationToken);

        if (!authorExists)
            return Result.Failure<Guid>(new NotFoundError(BookErrorCodes.AuthorNotFound, "The author with the specified identifier was not found."));

        bool isbnExists = await _context.Books
            .AnyAsync(b => b.ISBN == command.ISBN, cancellationToken);

        if (isbnExists)
            return Result.Failure<Guid>(new ConflictError(BookErrorCodes.IsbnConflict, $"A book with ISBN '{command.ISBN}' already exists."));

        var createResult = Book.Create(
            command.Title,
            authorId,
            command.ISBN,
            command.Price,
            command.PublicationYear,
            _timeProvider);

        if (createResult.IsFailure)
            return Result.Failure<Guid>(createResult.Error);

        var book = createResult.Value;
        _context.Books.Add(book);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(book.Id.Value);
    }
}

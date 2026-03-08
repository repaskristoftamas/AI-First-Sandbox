using Bookstore.Application.Authors.Commands.CreateAuthor;
using Bookstore.Application.Authors.Commands.DeleteAuthor;
using Bookstore.Application.Authors.Commands.UpdateAuthor;
using Bookstore.Application.Authors.Queries.GetAllAuthors;
using Bookstore.Application.Authors.Queries.GetAuthorById;
using Bookstore.Domain.Authors;
using Bookstore.WebApi.Extensions;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.WebApi.Endpoints.Authors;

/// <summary>
/// Defines the CRUD endpoints for managing authors in the catalog.
/// </summary>
public sealed class AuthorEndpoints : IEndpointDefinition
{
    /// <summary>
    /// Registers all author-related routes under the /api/authors group.
    /// </summary>
    /// <param name="app">The endpoint route builder to register routes on.</param>
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/authors").WithTags("Authors");

        group.MapGet("/", GetAllAuthors)
            .WithName("GetAllAuthors")
            .AllowAnonymous();

        group.MapGet("/{id:guid}", GetAuthorById)
            .WithName("GetAuthorById")
            .AllowAnonymous();

        group.MapPost("/", CreateAuthor)
            .WithName("CreateAuthor")
            .RequireAuthorization();

        group.MapPut("/{id:guid}", UpdateAuthor)
            .WithName("UpdateAuthor")
            .RequireAuthorization();

        group.MapDelete("/{id:guid}", DeleteAuthor)
            .WithName("DeleteAuthor")
            .RequireAuthorization();
    }

    /// <summary>
    /// Retrieves a page of authors from the catalog.
    /// </summary>
    /// <param name="page">One-based page number (default: 1).</param>
    /// <param name="pageSize">Number of results per page (default: 20).</param>
    /// <param name="sender">Mediator sender for dispatching the query.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>An OK result with the author page, or a problem response on failure.</returns>
    private static async Task<Results<Ok<List<AuthorResponse>>, ProblemHttpResult>> GetAllAuthors(
        ISender sender,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await sender.Send(new GetAllAuthorsQuery(page, pageSize), cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value.Select(d => d.ToResponse()).ToList())
            : result.Error.ToProblemHttpResult();
    }

    /// <summary>
    /// Retrieves a single author by their identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the author.</param>
    /// <param name="sender">Mediator sender for dispatching the query.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>An OK result with the author, or a problem response if not found.</returns>
    private static async Task<Results<Ok<AuthorResponse>, ProblemHttpResult>> GetAuthorById(
        Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAuthorByIdQuery(new AuthorId(id)), cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value.ToResponse())
            : result.Error.ToProblemHttpResult();
    }

    /// <summary>
    /// Creates a new author in the catalog from the provided request body.
    /// </summary>
    /// <param name="request">The request body containing the author details.</param>
    /// <param name="sender">Mediator sender for dispatching the command.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>A 201 Created result with the new author's identifier, or a problem response on failure.</returns>
    private static async Task<Results<CreatedAtRoute<Guid>, ProblemHttpResult>> CreateAuthor(
        [FromBody] CreateAuthorRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateAuthorCommand(request.FirstName, request.LastName, request.DateOfBirth);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(result.Value, "GetAuthorById", new { id = result.Value })
            : result.Error.ToProblemHttpResult();
    }

    /// <summary>
    /// Updates all properties of an existing author identified by the route parameter.
    /// </summary>
    /// <param name="id">The unique identifier of the author to update.</param>
    /// <param name="request">The request body containing the updated author properties.</param>
    /// <param name="sender">Mediator sender for dispatching the command.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>A 204 No Content result on success, or a problem response on failure.</returns>
    private static async Task<Results<NoContent, ProblemHttpResult>> UpdateAuthor(
        Guid id,
        [FromBody] UpdateAuthorRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAuthorCommand(new AuthorId(id), request.FirstName, request.LastName, request.DateOfBirth);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.Error.ToProblemHttpResult();
    }

    /// <summary>
    /// Deletes an author from the catalog by their identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the author to delete.</param>
    /// <param name="sender">Mediator sender for dispatching the command.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>A 204 No Content result on success, or a problem response if not found.</returns>
    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteAuthor(
        Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteAuthorCommand(new AuthorId(id)), cancellationToken);
        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.Error.ToProblemHttpResult();
    }
}

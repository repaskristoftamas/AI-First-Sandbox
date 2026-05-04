using Bookstore.Application.Authors.Commands.CreateAuthor;
using Bookstore.Application.Authors.Commands.DeleteAuthor;
using Bookstore.Application.Authors.Commands.UpdateAuthor;
using Bookstore.Application.Authors.Queries.GetAllAuthors;
using Bookstore.Application.Authors.Queries.GetAuthorById;
using Bookstore.Domain.Authors;
using Bookstore.WebApi.Authorization;
using Bookstore.WebApi.Extensions;
using Bookstore.WebApi.Filters;
using Bookstore.WebApi.Pagination;
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
    /// Registers all author-related routes under the /api/v{version}/authors group.
    /// </summary>
    /// <param name="routes">The endpoint route builder to register routes on.</param>
    public void RegisterEndpoints(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/authors").WithTags("Authors");

        group.MapGet("/", GetAllAuthors)
            .WithName("GetAllAuthors")
            .AllowAnonymous();

        group.MapGet("/{id:guid}", GetAuthorById)
            .WithName("GetAuthorById")
            .AddEndpointFilter<ETagEndpointFilter>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group.MapPost("/", CreateAuthor)
            .WithName("CreateAuthor")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        group.MapPut("/{id:guid}", UpdateAuthor)
            .WithName("UpdateAuthor")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapDelete("/{id:guid}", DeleteAuthor)
            .WithName("DeleteAuthor")
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
    }

    /// <summary>
    /// Retrieves a page of authors from the catalog.
    /// </summary>
    /// <param name="page">One-based page number (default: 1).</param>
    /// <param name="pageSize">Number of results per page (default: 20).</param>
    /// <param name="sender">Mediator sender for dispatching the query.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>An OK result with the author page, or a problem response on failure.</returns>
    private static async Task<Results<Ok<PagedResponse<AuthorResponse>>, ProblemHttpResult>> GetAllAuthors(
        ISender sender,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "ValidationError",
                detail: page < 1
                    ? "'page' must be ≥ 1."
                    : "'pageSize' must be between 1 and 100.");

        var result = await sender.Send(new GetAllAuthorsQuery(page, pageSize), cancellationToken);
        if (!result.IsSuccess)
            return result.Error.ToProblemHttpResult();

        var paged = result.Value;
        var items = (IReadOnlyList<AuthorResponse>)[.. paged.Items.Select(d => d.ToResponse())];
        return TypedResults.Ok(new PagedResponse<AuthorResponse>(
            items,
            paged.TotalCount,
            paged.Page,
            paged.PageSize,
            paged.TotalPages,
            paged.HasNextPage,
            paged.HasPreviousPage));
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

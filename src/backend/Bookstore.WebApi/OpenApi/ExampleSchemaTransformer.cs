using System.Text.Json.Nodes;
using Bookstore.WebApi.Endpoints.Authors;
using Bookstore.WebApi.Endpoints.Books;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Bookstore.WebApi.OpenApi;

/// <summary>
/// Adds representative example values to request and response schemas in the OpenAPI document.
/// </summary>
internal sealed class ExampleSchemaTransformer : IOpenApiSchemaTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        if (type == typeof(AuthorResponse))
            schema.Example = AuthorResponseExample();
        else if (type == typeof(CreateAuthorRequest))
            schema.Example = CreateAuthorRequestExample();
        else if (type == typeof(UpdateAuthorRequest))
            schema.Example = UpdateAuthorRequestExample();
        else if (type == typeof(BookResponse))
            schema.Example = BookResponseExample();
        else if (type == typeof(CreateBookRequest))
            schema.Example = CreateBookRequestExample();
        else if (type == typeof(UpdateBookRequest))
            schema.Example = UpdateBookRequestExample();

        return Task.CompletedTask;
    }

    private static JsonObject AuthorResponseExample() => new()
    {
        ["id"] = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        ["firstName"] = "Harper",
        ["lastName"] = "Lee",
        ["dateOfBirth"] = "1926-04-28",
        ["createdAt"] = "2024-01-15T10:30:00+00:00",
        ["updatedAt"] = "2024-06-20T14:45:00+00:00"
    };

    private static JsonObject CreateAuthorRequestExample() => new()
    {
        ["firstName"] = "Harper",
        ["lastName"] = "Lee",
        ["dateOfBirth"] = "1926-04-28"
    };

    private static JsonObject UpdateAuthorRequestExample() => new()
    {
        ["firstName"] = "Harper",
        ["lastName"] = "Lee",
        ["dateOfBirth"] = "1926-04-28"
    };

    private static JsonObject BookResponseExample() => new()
    {
        ["id"] = "b2c3d4e5-f6a7-8901-bcde-f12345678901",
        ["title"] = "To Kill a Mockingbird",
        ["authorId"] = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        ["isbn"] = "9780061120084",
        ["price"] = 19.99,
        ["publicationYear"] = 1960,
        ["createdAt"] = "2024-01-15T10:30:00+00:00",
        ["updatedAt"] = "2024-06-20T14:45:00+00:00"
    };

    private static JsonObject CreateBookRequestExample() => new()
    {
        ["title"] = "To Kill a Mockingbird",
        ["authorId"] = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        ["isbn"] = "9780061120084",
        ["price"] = 19.99,
        ["publicationYear"] = 1960
    };

    private static JsonObject UpdateBookRequestExample() => new()
    {
        ["title"] = "To Kill a Mockingbird",
        ["authorId"] = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        ["isbn"] = "9780061120084",
        ["price"] = 19.99,
        ["publicationYear"] = 1960
    };
}

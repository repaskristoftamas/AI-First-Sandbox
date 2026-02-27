using System.Reflection;
using FluentValidation;
using Mediator;
using Bookstore.SharedKernel.Results;

namespace Bookstore.Application.Behaviors;

/// <summary>
/// Pipeline behavior that runs all registered FluentValidation validators for a message
/// before dispatching it to its handler. Returns a validation failure result if any
/// rules are violated, without invoking the handler.
/// </summary>
/// <typeparam name="TMessage">The message type being dispatched.</typeparam>
/// <typeparam name="TResponse">The response type; must be <see cref="Result"/> or <see cref="Result{TValue}"/>.</typeparam>
public sealed class ValidationBehavior<TMessage, TResponse>(
    IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    /// <summary>
    /// Validates the message using all registered validators. If validation fails,
    /// returns a <see cref="ValidationError"/> result without reaching the handler.
    /// </summary>
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(message, cancellationToken);

        var failures = validators
            .Select(v => v.Validate(message))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count is 0)
            return await next(message, cancellationToken);

        var error = new ValidationError(
            string.Join("; ", failures.Select(f => f.ErrorMessage)));

        return ValidationFailureFactory<TResponse>.Create(error);
    }
}

/// <summary>
/// Caches a compiled factory delegate for creating failure results of type <typeparamref name="TResult"/>.
/// Supports both <see cref="Result"/> and <see cref="Result{TValue}"/>.
/// </summary>
file static class ValidationFailureFactory<TResult>
{
    private static readonly Func<Error, TResult> _factory = BuildFactory();

    /// <summary>Creates a failure result of <typeparamref name="TResult"/> from the given error.</summary>
    internal static TResult Create(Error error) => _factory(error);

    private static Func<Error, TResult> BuildFactory()
    {
        var type = typeof(TResult);

        if (type == typeof(Result))
            return static e => (TResult)(object)Result.Failure(e);

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = type.GetGenericArguments()[0];
            var method = typeof(Result).GetMethod(
                nameof(Result.Failure),
                1,
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: [typeof(Error)],
                modifiers: null)!.MakeGenericMethod(valueType);

            return e => (TResult)method.Invoke(null, [e])!;
        }

        throw new InvalidOperationException(
            $"ValidationBehavior only supports Result and Result<T> response types. Got: {type.Name}");
    }
}

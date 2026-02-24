namespace Bookstore.SharedKernel.Results;

/// <summary>
/// Represents the outcome of an operation that can succeed or fail, without a return value.
/// </summary>
public class Result
{
    private readonly Error? _error;

    /// <summary>
    /// Initializes a new result, enforcing that success and error state are consistent.
    /// </summary>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="error">The error describing the failure, or <c>null</c> for a success.</param>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="isSuccess"/> is <c>true</c> with a non-null error, or <c>false</c> with a null error.</exception>
    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException();
        if (!isSuccess && error is null)
            throw new InvalidOperationException();

        IsSuccess = isSuccess;
        _error = error;
    }

    /// <summary>
    /// Indicates whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The error describing why the operation failed. Throws if the result is a success.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessed on a successful result.</exception>
    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("The error of a success result cannot be accessed.");

    /// <summary>
    /// Creates a successful result with no value.
    /// </summary>
    /// <returns>A successful <see cref="Result"/>.</returns>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error describing the failure.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a successful result carrying the specified value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="value">The value produced by the successful operation.</param>
    /// <returns>A successful <see cref="Result{TValue}"/> containing <paramref name="value"/>.</returns>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, null);

    /// <summary>
    /// Creates a failed result of the specified value type with the given error.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="error">The error describing the failure.</param>
    /// <returns>A failed <see cref="Result{TValue}"/>.</returns>
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

/// <summary>
/// Represents the outcome of an operation that can succeed with a value or fail with an error.
/// </summary>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    /// <summary>
    /// Initializes a new result with an optional value, enforcing consistency between success state and error.
    /// </summary>
    /// <param name="value">The value produced by the operation, or <c>default</c> on failure.</param>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="error">The error describing the failure, or <c>null</c> for a success.</param>
    protected internal Result(TValue? value, bool isSuccess, Error? error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// The value produced by a successful operation. Throws if the result is a failure.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessed on a failed result.</exception>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

    /// <summary>
    /// Implicitly wraps a value in a successful result, enabling concise return statements.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A successful <see cref="Result{TValue}"/> containing <paramref name="value"/>.</returns>
    public static implicit operator Result<TValue>(TValue value) => Success(value);
}

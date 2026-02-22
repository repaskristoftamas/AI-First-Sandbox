namespace Bookstore.SharedKernel.Results;

public abstract record Error(string Description);

public sealed record NotFoundError(string Description) : Error(Description);

public sealed record ConflictError(string Description) : Error(Description);

public sealed record ValidationError(string Description) : Error(Description);

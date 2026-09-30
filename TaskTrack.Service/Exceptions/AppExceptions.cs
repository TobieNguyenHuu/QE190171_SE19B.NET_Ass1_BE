namespace TaskTrack.Service.Exceptions;

/// <summary>Resource does not exist -> HTTP 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>A business rule blocks the operation (e.g. delete with linked rows) -> HTTP 400.</summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}

/// <summary>Field-level validation failed in the service layer -> HTTP 400 with an "errors" object.</summary>
public class ValidationFailedException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationFailedException(string field, string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]> { [field] = new[] { message } };
    }
}

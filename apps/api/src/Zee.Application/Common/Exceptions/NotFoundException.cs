namespace Zee.Application.Common.Exceptions;

/// <summary>Thrown when a requested entity does not exist. Mapped to HTTP 404.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException()
        : base("The requested resource was not found.")
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Builds a message of the form "Post 3f2a... was not found."</summary>
    public static NotFoundException For(string entityName, object key) =>
        new($"{entityName} '{key}' was not found.");
}

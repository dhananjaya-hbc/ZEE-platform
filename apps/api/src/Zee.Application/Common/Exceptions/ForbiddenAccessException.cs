namespace Zee.Application.Common.Exceptions;

/// <summary>
/// Thrown when the caller is authenticated but not permitted to do this. Mapped to HTTP 403.
/// </summary>
/// <remarks>
/// Distinct from the 401 that missing or invalid credentials produce: 401 means "who are
/// you", 403 means "I know who you are and the answer is no". Editing someone else's post,
/// or posting into a group you have not joined, land here.
/// </remarks>
public sealed class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException()
        : base("You do not have permission to perform this action.")
    {
    }

    public ForbiddenAccessException(string message)
        : base(message)
    {
    }

    public ForbiddenAccessException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

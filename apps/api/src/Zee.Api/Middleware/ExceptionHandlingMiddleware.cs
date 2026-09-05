using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Zee.Application.Common.Exceptions;
using Zee.Domain.Common;
using ValidationException = Zee.Application.Common.Exceptions.ValidationException;

namespace Zee.Api.Middleware;

/// <summary>
/// Converts exceptions thrown anywhere below into RFC 9457 problem responses.
/// </summary>
/// <remarks>
/// This exists so handlers and controllers contain no try/catch. A handler expresses the
/// happy path and throws a meaningful exception when it cannot proceed; deciding what
/// status code that becomes is a transport concern and lives here, once.
///
/// <para><b>Unmapped exceptions never reach the client.</b> The catch-all returns a generic
/// message and logs the detail server-side. Exception text routinely contains connection
/// strings, file paths and SQL, and an error response is exactly where an attacker looks
/// for them.</para>
/// </remarks>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await WriteProblemAsync(context, ex).ConfigureAwait(false);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            // Too late to change the status code; the body is already going out.
            _logger.LogError(exception, "Exception thrown after the response had started.");
            return;
        }

        var problem = BuildProblem(exception);

        if (problem.Status >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception processing {Path}.", context.Request.Path);
        }
        else if (_logger.IsEnabled(LogLevel.Information))
        {
            // Expected, client-caused outcomes. Logged at a level that will not drown out
            // real faults, but still visible when investigating a specific request.
            //
            // The IsEnabled guard is not ceremony: this runs on every rejected request, and
            // without it the arguments are boxed and evaluated even when Information logging
            // is switched off in production.
            _logger.LogInformation(
                "Request to {Path} rejected with {Status}: {Message}",
                context.Request.Path,
                problem.Status,
                exception.Message);
        }

        problem.Instance = context.Request.Path;
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = MediaTypeNames.Application.ProblemJson;

        await context.Response.WriteAsJsonAsync(problem, problem.GetType()).ConfigureAwait(false);
    }

    private static ProblemDetails BuildProblem(Exception exception) => exception switch
    {
        // Every field error at once, keyed by property, so the web client can attach each
        // message to the input it belongs to.
        ValidationException validation => new ValidationProblemDetails(
            validation.Errors.ToDictionary(e => e.Key, e => e.Value, StringComparer.Ordinal))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
        },

        // A business rule was broken. Safe to surface: these messages are written for
        // students and contain no internals.
        DomainException domain => new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "The request could not be completed.",
            Detail = domain.Message,
        },

        NotFoundException notFound => new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Not found.",
            Detail = notFound.Message,
        },

        ForbiddenAccessException => new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden.",
            Detail = "You do not have permission to perform this action.",
        },

        TooManyRequestsException tooMany => new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Too many requests.",
            Detail = tooMany.Message,
        },

                InvalidOtpException invalidOtp => new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Invalid code.",
            Detail = invalidOtp.Message,
        },



        // Phase 1 scaffolding: most handlers are still stubs. 501 says "this endpoint is
        // routed and reachable but not built yet", which is a far more useful signal to a
        // contributor - and to the frontend - than a generic 500.
        NotImplementedException => new ProblemDetails
        {
            Status = StatusCodes.Status501NotImplemented,
            Title = "Not implemented.",
            Detail = "This endpoint is scaffolded but not yet implemented. "
                   + "See the TODO in the corresponding handler.",
        },

        OperationCanceledException => new ProblemDetails
        {
            Status = StatusCodesExtra.ClientClosedRequest,
            Title = "Request cancelled.",
        },

        _ => new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            // Deliberately no Detail. See the class remarks.
        },
    };
}

/// <summary>Extra status codes not present in <see cref="StatusCodes"/>.</summary>
internal static class StatusCodesExtra
{
    /// <summary>499 Client Closed Request (nginx convention).</summary>
    public const int ClientClosedRequest = 499;
}

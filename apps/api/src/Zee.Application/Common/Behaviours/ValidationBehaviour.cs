using FluentValidation;
using MediatR;
using ValidationException = Zee.Application.Common.Exceptions.ValidationException;

namespace Zee.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline step that runs every registered validator for a request before its
/// handler executes.
/// </summary>
/// <typeparam name="TRequest">The command or query being sent.</typeparam>
/// <typeparam name="TResponse">What the handler returns.</typeparam>
/// <remarks>
/// This is the reason handlers contain no validation code. Writing a
/// <c>CreatePostCommandValidator</c> is enough to have it enforced - registration is by
/// assembly scan, and this behaviour runs whatever it finds.
///
/// <para>The alternative, calling validators by hand at the top of each handler, fails in
/// the way that matters: it works until someone adds a handler and forgets, and nothing
/// tells them. Here, forgetting is not expressible.</para>
///
/// <para>A request with no validators skips straight through, so the cost on unvalidated
/// queries is a single empty-collection check.</para>
/// </remarks>
public sealed class ValidationBehaviour<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        if (!_validators.Any())
        {
            return await next().ConfigureAwait(false);
        }

        var context = new ValidationContext<TRequest>(request);

        // All validators run, and every failure is collected, rather than stopping at the
        // first. A student filling in a form should see everything that is wrong in one go.
        var results = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)))
            .ConfigureAwait(false);

        var failures = results
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await next().ConfigureAwait(false);
    }
}

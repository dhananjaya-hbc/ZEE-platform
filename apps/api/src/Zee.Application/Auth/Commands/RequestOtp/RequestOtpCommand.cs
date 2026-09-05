using MediatR;

namespace Zee.Application.Auth.Commands.RequestOtp;

/// <summary>
/// Requests a one-time sign-in code be emailed to an institutional address.
/// </summary>
/// <param name="Email">The address to verify. Must match a verified domain of an onboarded university.</param>
/// <remarks>
/// Returns nothing on success - the response is deliberately silent about whether an
/// account already exists for this address, only about whether the domain itself is
/// onboarded (which is already public information; see
/// <c>IUniversityRepository.GetActiveAsync</c>).
/// </remarks>
public sealed record RequestOtpCommand(string Email) : IRequest;

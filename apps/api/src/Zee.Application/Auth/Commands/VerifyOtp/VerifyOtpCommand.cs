
using MediatR;
using Zee.Application.Auth.Dtos;

namespace Zee.Application.Auth.Commands.VerifyOtp;

/// <summary>
/// Redeems a one-time code, signing the student in.
/// </summary>
/// <param name="Email">The address the code was requested for.</param>
/// <param name="Code">The 6-digit code from the student's inbox.</param>
/// <remarks>
/// On success, creates the <c>User</c> if this is their first sign-in - see the handler
/// for how a display name is derived when none has been collected yet.
/// </remarks>
public sealed record VerifyOtpCommand(string Email, string Code) : IRequest<AuthResultDto>;

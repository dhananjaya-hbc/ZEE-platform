using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zee.Application.Auth.Commands.RequestOtp;
using Zee.Application.Auth.Commands.VerifyOtp;
using Zee.Application.Auth.Dtos;

namespace Zee.Api.Controllers;

/// <summary>Institutional email sign-in: request a one-time code, then redeem it.</summary>
/// <remarks>
/// The only controller in the API marked <see cref="AllowAnonymousAttribute"/> at the
/// class level. Every other controller inherits [Authorize] from
/// <see cref="ApiControllerBase"/> by default - these two endpoints are the sole
/// exception, because a student has no token yet when calling them.
/// </remarks>
[AllowAnonymous]
public sealed class AuthController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>Requests a one-time code be emailed to an institutional address.</summary>
    /// <response code="204">A code was generated and emailed (or logged, in Development).</response>
    /// <response code="400">The email address is malformed.</response>
    /// <response code="404">No university is onboarded for that email's domain.</response>
    /// <response code="429">Too many codes have been requested for this address recently.</response>
    [HttpPost("request-otp")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RequestOtp(
        [FromBody] RequestOtpCommand command,
        CancellationToken cancellationToken)
    {
        await Sender.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>Redeems a one-time code, signing the student in.</summary>
    /// <response code="200">Signed in. Returns an access token.</response>
    /// <response code="400">The code is invalid, expired, already used, or malformed.</response>
    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResultDto>> VerifyOtp(
        [FromBody] VerifyOtpCommand command,
        CancellationToken cancellationToken)
        => Ok(await Sender.Send(command, cancellationToken));
}

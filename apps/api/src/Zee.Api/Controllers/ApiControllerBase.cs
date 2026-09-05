using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Zee.Api.Controllers;

/// <summary>
/// Shared base for ZEE's controllers: authenticated by default, with a MediatR sender.
/// </summary>
/// <remarks>
/// <b>[Authorize] is applied here, not per-controller.</b> That makes authentication the
/// default and requires an explicit [AllowAnonymous] to opt out - so a new endpoint is
/// private unless someone deliberately publishes it. The reverse arrangement fails silently
/// the first time a contributor forgets an attribute, and nothing in review reliably
/// catches a missing one.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public abstract class ApiControllerBase(ISender sender) : ControllerBase
{
    /// <summary>Dispatches commands and queries to their handlers.</summary>
    protected ISender Sender { get; } = sender;
}

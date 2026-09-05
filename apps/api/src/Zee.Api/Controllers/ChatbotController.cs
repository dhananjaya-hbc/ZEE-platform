using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zee.Application.Chatbot.Queries.AskChatbot;
using Zee.Application.Common.Ai;

namespace Zee.Api.Controllers;

/// <summary>The campus assistant.</summary>
/// <remarks>
/// <b>Phase 1 returns mock answers.</b> The full path is real - this controller dispatches
/// to a handler, which calls the AI service over HTTP with the internal key - but the
/// FastAPI service replies with canned text until Phase 2.
///
/// <para>Browsers never reach the AI service directly. It has no public route, and the
/// X-Internal-Key it requires is held server-side by this API. Proxying through here is
/// what lets the AI service trust its caller at all.</para>
/// </remarks>
public sealed class ChatbotController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>Asks the assistant a question.</summary>
    /// <response code="200">An answer with its sources. Sources may be empty.</response>
    /// <response code="400">The question was empty or too long.</response>
    [HttpPost("ask")]
    [ProducesResponseType(typeof(ChatbotAnswer), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatbotAnswer>> Ask(
        [FromBody] AskChatbotQuery query,
        CancellationToken cancellationToken)
        => Ok(await Sender.Send(query, cancellationToken));
}

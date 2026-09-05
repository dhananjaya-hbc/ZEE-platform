using MediatR;
using Zee.Application.Common.Ai;
using Zee.Application.Common.Interfaces;

namespace Zee.Application.Chatbot.Queries.AskChatbot;

/// <summary>
/// Handles <see cref="AskChatbotQuery"/> by delegating to the AI service.
/// </summary>
/// <remarks>
/// This handler is a pass-through, and should stay one. It exists so the controller depends
/// on MediatR like every other endpoint rather than on <c>IAiServiceClient</c> directly -
/// which is what keeps the Phase 2 swap invisible to the Api layer.
///
/// <para><b>In Phase 1 the answer is mock text from the FastAPI service.</b> The whole path
/// is real - HTTP call, internal key header, timeout, deserialisation - so when the real
/// model is wired up in Phase 2, nothing on the .NET side changes.</para>
/// </remarks>
///
/// TODO: Implement Handle.
/// Acceptance criteria:
///   1. Read userId from _currentUser; throw ForbiddenAccessException if null.
///   2. return await _ai.AskAsync(request.Question, userId.Value, cancellationToken).
///   3. Do NOT wrap this in try/catch. IAiServiceClient already contracts to return
///      ChatbotAnswer.Unavailable rather than throwing when the service is unreachable;
///      catching here would just hide real bugs behind the same generic message.
public sealed class AskChatbotQueryHandler(
    IAiServiceClient ai,
    ICurrentUser currentUser)
    : IRequestHandler<AskChatbotQuery, ChatbotAnswer>
{
    private readonly IAiServiceClient _ai = ai;
    private readonly ICurrentUser _currentUser = currentUser;

    public Task<ChatbotAnswer> Handle(AskChatbotQuery request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}

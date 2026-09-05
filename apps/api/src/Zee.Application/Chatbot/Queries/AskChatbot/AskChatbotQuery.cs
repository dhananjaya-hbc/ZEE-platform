using MediatR;
using Zee.Application.Common.Ai;

namespace Zee.Application.Chatbot.Queries.AskChatbot;

/// <summary>
/// Asks the campus assistant a question on behalf of the authenticated student.
/// </summary>
/// <param name="Question">The student's question in natural language.</param>
/// <remarks>
/// Modelled as a query rather than a command because it changes no ZEE state.
///
/// <para>The <c>user_id</c> the AI service receives comes from <c>ICurrentUser</c>, not from
/// the request. In Phase 2 that id will scope retrieval to what the student is allowed to
/// see, so accepting it from the client would let anyone read another student's context by
/// changing one field - the kind of hole that is far cheaper to close now than after the
/// RAG pipeline is built on top of it.</para>
/// </remarks>
public sealed record AskChatbotQuery(string Question) : IRequest<ChatbotAnswer>;

"""Campus assistant logic.

Phase 1: returns mock data.
Phase 2: retrieval-augmented generation over campus content. OWNER-MAINTAINED -
see docs/AI_DESIGN.md and CONTRIBUTING.md before opening a PR here.
"""

from app.schemas.chatbot import ChatbotAnswerResponse


async def answer_question(question: str, user_id: str) -> ChatbotAnswerResponse:
    """Answer a student's question.

    Args:
        question: The student's natural-language question.
        user_id: The asking student, for scoping retrieval in Phase 2.

    Returns:
        An answer with its supporting sources.

    TODO (Phase 1): Return a MOCK response. Do not build real logic here.

    Acceptance criteria:
      - Return a ChatbotAnswerResponse with placeholder text that clearly reads as
        a mock, so nobody mistakes it for a working assistant.
      - Include at least one ChatbotSource, so the frontend's citation rendering is
        exercised from day one rather than discovered to be missing in Phase 2.
      - Echo nothing from `question` back into `answer` verbatim - a mock that
        reflects user input is an XSS foot-gun in whatever renders it.
    """
    raise NotImplementedError

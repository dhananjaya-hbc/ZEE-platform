"""Chatbot routes."""

from fastapi import APIRouter

from app.schemas.chatbot import ChatbotAnswerResponse, ChatbotAskRequest
from app.services import chatbot_service

router = APIRouter(prefix="/api/chatbot", tags=["chatbot"])


@router.post("/ask", response_model=ChatbotAnswerResponse)
async def ask(request: ChatbotAskRequest) -> ChatbotAnswerResponse:
    """Answer a student's question.

    Phase 1 returns mock data. Authentication is applied at the router level in
    main.py, so this function never handles the internal key itself.
    """
    return await chatbot_service.answer_question(request.question, request.user_id)

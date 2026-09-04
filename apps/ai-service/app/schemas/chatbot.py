"""Request and response models for the chatbot endpoint.

These mirror the C# records in
``apps/api/src/Zee.Application/Common/Ai/AiContracts.cs``. The two halves are one
contract: changing a field name here without changing it there breaks the boundary
silently, because the .NET side deserialises by name.
"""

from pydantic import BaseModel, Field


class ChatbotAskRequest(BaseModel):
    """POST /api/chatbot/ask body."""

    question: str = Field(..., min_length=1, max_length=1000)
    user_id: str = Field(
        ...,
        description=(
            "The asking student. Supplied by the .NET API from the caller's JWT, never "
            "by a browser. In Phase 2 this scopes retrieval to what the student may see."
        ),
    )


class ChatbotSource(BaseModel):
    """One document the answer drew on."""

    id: str
    title: str
    url: str | None = None


class ChatbotAnswerResponse(BaseModel):
    """POST /api/chatbot/ask response."""

    answer: str
    sources: list[ChatbotSource] = Field(default_factory=list)

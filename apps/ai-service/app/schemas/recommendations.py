"""Response models for the student matching endpoint.

Mirrors ``StudentMatch`` in the .NET AiContracts.
"""

from pydantic import BaseModel, Field


class StudentMatch(BaseModel):
    """A suggested connection."""

    user_id: str
    score: float = Field(..., ge=0.0, le=1.0)
    reason: str = Field(
        ...,
        description=(
            "Short human-readable justification. Required, not optional: an "
            "unexplained 'you should meet this person' is unactionable, and forcing "
            "an explanation keeps Phase 2 honest about whether its matches hold up."
        ),
    )


class RecommendationsResponse(BaseModel):
    """GET /api/recommendations/{user_id} response."""

    matches: list[StudentMatch] = Field(default_factory=list)

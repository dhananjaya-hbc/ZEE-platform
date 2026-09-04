"""Response models for the ranked feed endpoint.

Mirrors ``RankedPost`` in the .NET AiContracts.
"""

from pydantic import BaseModel, Field


class RankedPost(BaseModel):
    """A post id with its relevance score.

    Note this carries no post content. The AI service does not own posts and must
    not become a second source of truth for them - the .NET API hydrates these ids
    from PostgreSQL, which is also where visibility rules are applied.
    """

    post_id: str
    score: float


class FeedResponse(BaseModel):
    """GET /api/feed/{user_id} response."""

    posts: list[RankedPost] = Field(default_factory=list)

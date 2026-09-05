"""Student matching logic.

Phase 1: returns mock data.
Phase 2: embedding-based similarity over courses, interests and activity.
OWNER-MAINTAINED - see docs/AI_DESIGN.md.
"""

from app.schemas.recommendations import RecommendationsResponse


async def recommend_students(user_id: str) -> RecommendationsResponse:
    """Suggest students the given student might want to connect with.

    TODO (Phase 1): Return a MOCK response with a small number of matches.

    Acceptance criteria:
      - Scores in [0.0, 1.0], returned in DESCENDING score order. The .NET side
        documents that ordering as part of the contract.
      - Every match carries a non-empty `reason`.
      - Never include `user_id` itself in the results.
    """
    raise NotImplementedError

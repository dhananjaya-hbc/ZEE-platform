"""Feed ranking logic.

Phase 1: returns mock data. THE PHASE 1 FEED DOES NOT CALL THIS - the .NET API
serves posts in reverse-chronological order and never consults this endpoint.
Phase 2: real personalised ranking. OWNER-MAINTAINED - see docs/AI_DESIGN.md.
"""

from app.schemas.feed import FeedResponse


async def rank_feed(user_id: str) -> FeedResponse:
    """Return a ranked ordering of posts for a student.

    TODO (Phase 1): Return a MOCK response - an empty list is acceptable and
    honest, since no real post ids are known to this service yet.

    Acceptance criteria:
      - Return post ids and scores only, never post content.
      - An empty list is a valid response. The .NET client reads empty as
        "fall back to chronological", so it must not be an error.
    """
    raise NotImplementedError

"""Ranked feed routes."""

from fastapi import APIRouter

from app.schemas.feed import FeedResponse
from app.services import ranking_service

router = APIRouter(prefix="/api/feed", tags=["feed"])


@router.get("/{user_id}", response_model=FeedResponse)
async def get_ranked_feed(user_id: str) -> FeedResponse:
    """Rank a student's feed.

    NOT called by the Phase 1 .NET feed, which is purely chronological. This exists
    so the Phase 2 ranked path has a route already agreed and documented.
    """
    return await ranking_service.rank_feed(user_id)

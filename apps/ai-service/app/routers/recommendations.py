"""Student matching routes."""

from fastapi import APIRouter

from app.schemas.recommendations import RecommendationsResponse
from app.services import matching_service

router = APIRouter(prefix="/api/recommendations", tags=["recommendations"])


@router.get("/{user_id}", response_model=RecommendationsResponse)
async def get_recommendations(user_id: str) -> RecommendationsResponse:
    """Suggest students to connect with. Phase 1 returns mock data."""
    return await matching_service.recommend_students(user_id)

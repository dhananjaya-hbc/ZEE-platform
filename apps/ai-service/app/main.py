"""ZEE AI service — FastAPI application entry point.

Phase 1 scaffolding. Every endpoint is routed, authenticated and typed, but the
service functions behind them return mock data. Phase 2 replaces those functions
without changing any route or schema, so the .NET side needs no changes.

Run locally:
    uvicorn app.main:app --reload --port 8000
"""

from fastapi import Depends, FastAPI

from app.core.config import get_settings
from app.core.security import require_internal_key
from app.routers import chatbot, feed, health, recommendations

settings = get_settings()

app = FastAPI(
    title="ZEE AI Service",
    version="0.1.0",
    description=(
        "Internal service for ZEE's chatbot, student matching and feed ranking. "
        "Phase 1 returns mock responses. Not internet-facing: every /api route "
        "requires the X-Internal-Key header."
    ),
    # Interactive docs only in development. In production this service has no
    # public route, and publishing its schema would just describe the internal
    # attack surface to anyone who reached it.
    docs_url="/docs" if settings.is_development else None,
    redoc_url=None,
    openapi_url="/openapi.json" if settings.is_development else None,
)

# Health is unauthenticated so orchestrators can probe it.
app.include_router(health.router)

# Everything else requires the internal key. Applying it here, at inclusion, rather
# than per-endpoint means a newly added route is protected by default - a route that
# forgets its own dependency would otherwise be silently public.
_protected = Depends(require_internal_key)

app.include_router(chatbot.router, dependencies=[_protected])
app.include_router(recommendations.router, dependencies=[_protected])
app.include_router(feed.router, dependencies=[_protected])

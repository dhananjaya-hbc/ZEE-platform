"""Liveness endpoint.

Deliberately OUTSIDE the internal-key dependency: container orchestrators and the
Compose healthcheck need to probe it without holding a secret.
"""

from fastapi import APIRouter

router = APIRouter(tags=["health"])


@router.get("/health")
async def health() -> dict[str, str]:
    """Report that the process is up."""
    return {"status": "ok", "phase": "1-mock"}

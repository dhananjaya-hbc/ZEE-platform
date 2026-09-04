"""Internal API key authentication.

Every endpoint under ``/api`` requires a valid ``X-Internal-Key`` header. The AI
service is not internet-facing: only the .NET API is meant to call it, and the .NET
API holds the key server-side so it never reaches a browser.
"""

# HTTPException and status are unused while the body below is a stub, but they are
# exactly what the implementation needs — keeping them saves the next person a lookup.
from fastapi import Depends, Header, HTTPException, status  # noqa: F401

from app.core.config import Settings, get_settings

INTERNAL_KEY_HEADER = "X-Internal-Key"


async def require_internal_key(
    x_internal_key: str | None = Header(default=None, alias=INTERNAL_KEY_HEADER),
    settings: Settings = Depends(get_settings),
) -> None:
    """Reject any request without the correct internal key.

    Raises:
        HTTPException: 401 when the header is missing or does not match.

    TODO: Implement. SECURITY-SENSITIVE.

    Acceptance criteria:
      1. Compare with ``secrets.compare_digest``, NOT ``==``. Ordinary string
         comparison short-circuits on the first differing byte, and that timing
         difference is measurable across many requests - enough to recover the key
         one byte at a time.
      2. A missing header and a wrong header must produce the SAME 401 response.
         Distinguishing them tells a prober whether they are on the right track.
      3. Never log the supplied or expected key.
      4. Tests in tests/test_security.py cover: valid key passes, wrong key 401s,
         missing header 401s.
    """
    raise NotImplementedError

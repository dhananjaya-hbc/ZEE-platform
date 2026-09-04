"""Tests for the internal API key gate.

To pick these up: implement ``require_internal_key`` in app/core/security.py, then
remove the ``@pytest.mark.skip`` decorators.
"""

import pytest
from fastapi.testclient import TestClient

TODO = "TODO: implement require_internal_key, then remove this skip."


@pytest.mark.skip(reason=TODO)
def test_valid_key_is_accepted(client: TestClient, internal_key: str) -> None:
    response = client.post(
        "/api/chatbot/ask",
        json={"question": "When is the library open?", "user_id": "test-user"},
        headers={"X-Internal-Key": internal_key},
    )

    assert response.status_code == 200


@pytest.mark.skip(reason=TODO)
def test_wrong_key_is_rejected(client: TestClient) -> None:
    response = client.post(
        "/api/chatbot/ask",
        json={"question": "hello", "user_id": "test-user"},
        headers={"X-Internal-Key": "wrong-key-entirely"},
    )

    assert response.status_code == 401


@pytest.mark.skip(reason=TODO)
def test_missing_header_is_rejected(client: TestClient) -> None:
    response = client.post(
        "/api/chatbot/ask",
        json={"question": "hello", "user_id": "test-user"},
    )

    assert response.status_code == 401


@pytest.mark.skip(reason=TODO)
def test_missing_and_wrong_key_are_indistinguishable(client: TestClient) -> None:
    """A prober must not be able to tell "no header" from "bad header"."""
    missing = client.post("/api/chatbot/ask", json={"question": "x", "user_id": "u"})
    wrong = client.post(
        "/api/chatbot/ask",
        json={"question": "x", "user_id": "u"},
        headers={"X-Internal-Key": "nope"},
    )

    assert missing.status_code == wrong.status_code
    assert missing.json() == wrong.json()


def test_health_needs_no_key(client: TestClient) -> None:
    """Not skipped: health is unauthenticated by design and works today."""
    response = client.get("/health")

    assert response.status_code == 200
    assert response.json()["status"] == "ok"

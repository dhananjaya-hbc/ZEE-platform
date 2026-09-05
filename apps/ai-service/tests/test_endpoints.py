"""Contract tests for the three AI endpoints.

These assert the SHAPE of each response, which is what the .NET client
deserialises into. The values are mock data and will change in Phase 2; the shapes
must not.

To pick these up: implement the service functions, then remove the skips.
"""

import pytest
from fastapi.testclient import TestClient

TODO = "TODO: implement the service function, then remove this skip."


@pytest.mark.skip(reason=TODO)
def test_chatbot_returns_answer_and_sources(client: TestClient, internal_key: str) -> None:
    response = client.post(
        "/api/chatbot/ask",
        json={"question": "Where is the careers fair?", "user_id": "u1"},
        headers={"X-Internal-Key": internal_key},
    )

    body = response.json()
    assert isinstance(body["answer"], str) and body["answer"]
    assert isinstance(body["sources"], list)


@pytest.mark.skip(reason=TODO)
def test_recommendations_are_sorted_by_descending_score(
    client: TestClient, internal_key: str
) -> None:
    response = client.get("/api/recommendations/u1", headers={"X-Internal-Key": internal_key})

    scores = [m["score"] for m in response.json()["matches"]]
    assert scores == sorted(scores, reverse=True)
    assert all(0.0 <= s <= 1.0 for s in scores)


@pytest.mark.skip(reason=TODO)
def test_recommendations_never_include_the_requesting_student(
    client: TestClient, internal_key: str
) -> None:
    response = client.get("/api/recommendations/u1", headers={"X-Internal-Key": internal_key})

    assert all(m["user_id"] != "u1" for m in response.json()["matches"])


@pytest.mark.skip(reason=TODO)
def test_ranked_feed_returns_ids_and_scores_only(client: TestClient, internal_key: str) -> None:
    response = client.get("/api/feed/u1", headers={"X-Internal-Key": internal_key})

    for post in response.json()["posts"]:
        assert set(post.keys()) == {"post_id", "score"}

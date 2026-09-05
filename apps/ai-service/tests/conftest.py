"""Shared pytest fixtures."""

import os
from collections.abc import Iterator

import pytest
from fastapi.testclient import TestClient

TEST_INTERNAL_KEY = "test-internal-key-not-a-real-secret"


@pytest.fixture(scope="session", autouse=True)
def _configure_environment() -> None:
    """Set the environment before the app module is imported.

    Settings are read at import time, so this has to happen first — hence
    autouse and session scope.
    """
    os.environ.setdefault("INTERNAL_API_KEY", TEST_INTERNAL_KEY)
    os.environ.setdefault("ENVIRONMENT", "development")


@pytest.fixture
def internal_key() -> str:
    return TEST_INTERNAL_KEY


@pytest.fixture
def client() -> Iterator[TestClient]:
    from app.main import app

    with TestClient(app) as test_client:
        yield test_client

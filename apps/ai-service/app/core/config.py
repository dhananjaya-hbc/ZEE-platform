"""Application settings, loaded from environment variables."""

from functools import lru_cache

from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    """Configuration for the ZEE AI service.

    Values come from environment variables, falling back to a local ``.env`` file
    during development. See ``.env.example``.
    """

    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        extra="ignore",
    )

    internal_api_key: str = Field(
        ...,
        min_length=16,
        alias="INTERNAL_API_KEY",
        description=(
            "Shared secret the .NET API sends as X-Internal-Key. Must match "
            "AISERVICE__INTERNALKEY on the .NET side exactly. This is the only thing "
            "protecting the service from anyone who can reach its port, so it is "
            "required with no default - the service refuses to start without it."
        ),
    )

    database_url: str | None = Field(
        default=None,
        alias="DATABASE_URL",
        description=(
            "PostgreSQL connection string. Unused in Phase 1 - the mocks touch no "
            "database - but present so Phase 2 retrieval has somewhere to read from."
        ),
    )

    environment: str = Field(default="development", alias="ENVIRONMENT")

    log_level: str = Field(default="INFO", alias="LOG_LEVEL")

    @property
    def is_development(self) -> bool:
        return self.environment.lower() in {"development", "dev", "local"}


@lru_cache
def get_settings() -> Settings:
    """Return the settings singleton.

    Cached so the environment is parsed once per process rather than per request,
    and so FastAPI's dependency system hands every caller the same instance.
    """
    return Settings()  # type: ignore[call-arg]

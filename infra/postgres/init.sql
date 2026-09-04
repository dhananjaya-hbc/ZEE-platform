-- Runs once, when the Postgres data directory is first initialised.
--
-- Re-running it means removing the volume:  docker compose down -v

-- pgvector is enabled now so Phase 2 can add embedding columns without a
-- migration that needs superuser rights at an awkward moment. Nothing in
-- Phase 1 uses it; an enabled-but-unused extension costs nothing.
CREATE EXTENSION IF NOT EXISTS vector;

-- Case-insensitive text, available for future use. Emails are normalised to
-- lowercase in the domain layer instead, so ordinary indexes still apply.
CREATE EXTENSION IF NOT EXISTS citext;

-- Trigram indexes, for search over post and group names later on.
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- NOTE: tables are created by EF Core migrations, not here. Keeping schema in
-- one place (the migrations) avoids the situation where the container's schema
-- and the migration history disagree about what exists.

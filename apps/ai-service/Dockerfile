# ZEE AI service.
#
# Python 3.12 rather than the newest release: pydantic-core ships prebuilt wheels
# for it, so the build takes seconds instead of compiling Rust from source.

FROM python:3.12-slim AS runtime

# Never write .pyc files; never buffer stdout, so logs actually appear in
# `docker compose logs` rather than sitting in a buffer.
ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PIP_NO_CACHE_DIR=1

WORKDIR /app

# Requirements first, so the dependency layer is cached independently of source.
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY app/ app/

# Non-root. Created after the pip install so site-packages stays root-owned and
# the application cannot modify its own dependencies at runtime.
RUN useradd --create-home --shell /usr/sbin/nologin zee \
    && chown -R zee:zee /app
USER zee

EXPOSE 8000

# No --reload: that is a development flag and it disables the worker model.
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]

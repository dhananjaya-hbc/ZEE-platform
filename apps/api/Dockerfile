# ZEE API — multi-stage build.
#
# The SDK image is ~800MB; the runtime image is ~200MB. Building in one and
# shipping from the other keeps the deployed image small and, more importantly,
# keeps the compiler and source out of production.

# --- Build -----------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project and props files FIRST and restore before copying source.
# Docker caches layers by content, so this restore layer is reused on every build
# where dependencies have not changed — which is nearly all of them.
COPY Directory.Build.props Directory.Packages.props ./
COPY src/Zee.Domain/Zee.Domain.csproj                 src/Zee.Domain/
COPY src/Zee.Application/Zee.Application.csproj       src/Zee.Application/
COPY src/Zee.Infrastructure/Zee.Infrastructure.csproj src/Zee.Infrastructure/
COPY src/Zee.Api/Zee.Api.csproj                       src/Zee.Api/
RUN dotnet restore src/Zee.Api/Zee.Api.csproj

COPY src/ src/
RUN dotnet publish src/Zee.Api/Zee.Api.csproj \
      --configuration Release \
      --output /app/publish \
      --no-restore \
      /p:UseAppHost=false

# --- Runtime ---------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# curl is needed by the Compose healthcheck and is not in the base image.
RUN apt-get update \
    && apt-get install --no-install-recommends -y curl \
    && rm -rf /var/lib/apt/lists/*

# Run as a non-root user. The aspnet image ships an "app" user for this; a
# container process that does not need root should not have it.
USER app

COPY --from=build --chown=app:app /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "Zee.Api.dll"]

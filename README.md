# daline-sys

Monorepo for the sport championship management platform.

## Layout

- `apps/api/` — .NET 10 backend (Sport.Core + 6 discipline modules + Sport.Api host).
- `apps/web/`, `apps/mobile/` — coming.
- `docs/` — design specs, implementation plans, ODF references.

## Local development

Prerequisites: Docker (or Docker Desktop), .NET 10 SDK.

1. Copy the env template and adjust the password:
   ```
   cp .env.example .env
   ```
2. Start Postgres in the background:
   ```
   docker compose up -d postgres
   ```
3. Run the API from your IDE or:
   ```
   dotnet run --project apps/api/src/Sport.Api/Sport.Api.csproj
   ```
4. API at `http://localhost:5080`. Explore the API in Scalar UI at `http://localhost:5080/scalar/v1`.

## Run the API in Docker (production-like)

```
docker compose --profile api up --build
```

API at `http://localhost:8080`. Health endpoint at `/health`.

## Tests

```
dotnet test apps/api/Sport.slnx
```

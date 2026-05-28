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

## Auditing queries

EF Core logs slow queries (default threshold 200 ms) to the API logs:

```
docker compose --profile api logs api | grep "Slow query"
```

Postgres-level stats via pg_stat_statements:

```
docker compose exec postgres psql -U sport -d sport -c \
  "SELECT calls, round(total_exec_time::numeric, 2) AS total_ms, \
          round(mean_exec_time::numeric, 2) AS mean_ms, query \
     FROM pg_stat_statements \
    ORDER BY total_exec_time DESC \
    LIMIT 20;"
```

Reset stats after optimizing:

```
docker compose exec postgres psql -U sport -d sport -c "SELECT pg_stat_statements_reset();"
```

Ad-hoc EXPLAIN ANALYZE on a query:

```
docker compose exec postgres psql -U sport -d sport -c \
  "EXPLAIN (ANALYZE, BUFFERS) <your sql>;"
```

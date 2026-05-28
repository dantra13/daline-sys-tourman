---
title: API Dockerization Design
date: 2026-05-28
status: draft
scope: Sport.Api host + Dockerfile + docker-compose with Postgres
---

# API Dockerization Design

Spec para crear el host ASP.NET Core `Sport.Api`, su `Dockerfile` multi-stage y el `docker-compose.yml` raíz del monorepo con PostgreSQL como dependencia. Habilita el siguiente paso (persistencia con EF Core) al dejar un ambiente local reproducible.

## 1. Contexto

El monorepo está organizado bajo `apps/api/` para el backend .NET (ver memoria `monorepo-layout.md` y commit del refactor). El core deportivo (`Sport.Core` + 6 módulos de disciplina) ya está implementado y testeado (121 tests verde). No existe todavía un proyecto host que sea entry point ejecutable: todo son class libraries.

Esta spec materializa:

- Un proyecto host `Sport.Api` (ASP.NET Core 10) con DI completo del core + endpoints mínimos (health, OpenAPI, Scalar).
- Un `Dockerfile` multi-stage que produce una imagen runtime sobre `aspnet:10.0-alpine`.
- Un `docker-compose.yml` raíz con `postgres:18-alpine` siempre activo y la API bajo un perfil opt-in.
- `.env`/`.env.example` para credenciales.
- Tests de integración del host con `Microsoft.AspNetCore.Mvc.Testing`.
- README raíz con instrucciones de dev.

## 2. Alcance

### En scope

- Proyecto `Sport.Api` con `Program.cs`, `appsettings*.json`, `launchSettings.json`.
- Endpoints: `GET /health`, `GET /health/ready`, OpenAPI doc en `/openapi/v1.json`, Scalar UI en `/scalar/v1`.
- DI: `AddSportCore()` + `AddDisciplineModule<>()` × 6, `BuildSportRegistry()` al arrancar.
- `Dockerfile` multi-stage (restore → publish → runtime alpine).
- `apps/api/.dockerignore`.
- `docker-compose.yml` raíz con `postgres` (default) y `api` (profile `api`).
- `.env.example` (committed) y entrada `.env` en `.gitignore`.
- `apps/api/tests/Sport.Api.Tests/` con 3 integration tests.
- `README.md` raíz con sección "Local development".

### Fuera de scope (specs futuras)

- EF Core / Npgsql wiring y conexión real a Postgres (la connection string queda registrada pero sin uso).
- Migraciones de base de datos.
- Endpoints de dominio (Competition, Event, Entry, etc.).
- MinIO (object storage), Mailpit (email).
- GitHub Actions CI.
- Multi-arch build, image registry, deploy pipeline.
- TLS / HTTPS dentro del container (HTTP local; producción usa reverse proxy).
- Hot reload dentro de Docker (`docker-compose.dev.yml`).
- Métricas, tracing, logging estructurado (Serilog se agregará en otra spec).

## 3. Decisiones tomadas

| # | Decisión | Resumen |
|---|---|---|
| D1 | Servicios en compose | Solo `postgres` (default) + `api` (profile `api`). MinIO/Mailpit en specs futuras. |
| D2 | Endpoints iniciales | Health + DI wiring + OpenAPI + Scalar. Sin endpoints de dominio. |
| D3 | Config | Env vars vía `.env` raíz; `.env.example` committed; falla rápido si `POSTGRES_PASSWORD` falta. |
| D4 | Runtime image | `mcr.microsoft.com/dotnet/aspnet:10.0-alpine`. |
| D5 | Postgres image | `postgres:18-alpine`. |
| D6 | Dev workflow | API desde IDE normalmente; Docker para Postgres y para correr API "production-like". |
| D7 | Profile | API tiene `profiles: ["api"]` para que `docker compose up` solo levante deps. |
| D8 | Build context | `./apps/api` (no repo root); el Dockerfile no necesita ver `docs/`, `.env`, etc. |
| D9 | Healthcheck | Dockerfile usa `curl` (vendrá pre-instalado en alpine) + compose con `pg_isready` y HTTP `/health`. |

## 4. Proyecto host `Sport.Api`

### 4.1 Estructura

```
apps/api/src/Sport.Api/
├── Sport.Api.csproj
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
└── Properties/launchSettings.json
```

### 4.2 `Sport.Api.csproj`

- SDK: `Microsoft.NET.Sdk.Web`.
- TargetFramework: `net10.0` (heredado de `Directory.Build.props`).
- Package references:
  - `Microsoft.AspNetCore.OpenApi` (10.0).
  - `Scalar.AspNetCore` (última estable).
- Project references:
  - `Sport.Core`.
  - Los 6 `Sport.Disciplines.<X>` (FBL, BKB, BDM, VBV, BOX, ATH).

### 4.3 `Program.cs` (esqueleto conceptual)

```csharp
using Scalar.AspNetCore;
using Sport.Core.DisciplineRegistry;
using Sport.Disciplines.ATH;
using Sport.Disciplines.BDM;
using Sport.Disciplines.BKB;
using Sport.Disciplines.BOX;
using Sport.Disciplines.FBL;
using Sport.Disciplines.VBV;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSportCore()
    .AddDisciplineModule<FblModule>()
    .AddDisciplineModule<BkbModule>()
    .AddDisciplineModule<BdmModule>()
    .AddDisciplineModule<VbvModule>()
    .AddDisciplineModule<BoxModule>()
    .AddDisciplineModule<AthModule>();

builder.Services.AddOpenApi();

var app = builder.Build();

// Force registry construction at startup so any module registration error is fail-fast.
app.Services.BuildSportRegistry();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/health",       () => Results.Ok(new { status = "alive" }));
app.MapGet("/health/ready", () => Results.Ok(new { status = "ready" }));

app.Run();
```

### 4.4 `appsettings.json`

```json
{
  "ConnectionStrings": {
    "Postgres": ""
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

`ConnectionStrings__Postgres` se sobreescribe por env var en runtime (Docker o local). Sin uso todavía en esta spec; la spec de persistencia lo consume.

### 4.5 `launchSettings.json` (dev local)

```json
{
  "profiles": {
    "Sport.Api": {
      "commandName": "Project",
      "applicationUrl": "http://localhost:5080",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "ConnectionStrings__Postgres": "Host=localhost;Port=5432;Database=sport;Username=sport;Password=change-me-locally"
      }
    }
  }
}
```

Puerto `5080` para no chocar con la API en Docker (que expone `8080` por default).

## 5. Dockerfile

Path: `apps/api/Dockerfile`. Multi-stage con cache de NuGet.

```dockerfile
# syntax=docker/dockerfile:1.7
ARG DOTNET_VERSION=10.0

# --- restore stage -----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-alpine AS restore
WORKDIR /src
COPY Directory.Build.props ./
COPY Sport.slnx ./
COPY src/Sport.Core/Sport.Core.csproj                       src/Sport.Core/
COPY src/Sport.Disciplines.FBL/Sport.Disciplines.FBL.csproj src/Sport.Disciplines.FBL/
COPY src/Sport.Disciplines.BKB/Sport.Disciplines.BKB.csproj src/Sport.Disciplines.BKB/
COPY src/Sport.Disciplines.BDM/Sport.Disciplines.BDM.csproj src/Sport.Disciplines.BDM/
COPY src/Sport.Disciplines.VBV/Sport.Disciplines.VBV.csproj src/Sport.Disciplines.VBV/
COPY src/Sport.Disciplines.BOX/Sport.Disciplines.BOX.csproj src/Sport.Disciplines.BOX/
COPY src/Sport.Disciplines.ATH/Sport.Disciplines.ATH.csproj src/Sport.Disciplines.ATH/
COPY src/Sport.Api/Sport.Api.csproj                         src/Sport.Api/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore src/Sport.Api/Sport.Api.csproj

# --- build + publish stage ---------------------------------------------------
FROM restore AS publish
COPY src/ ./src/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish src/Sport.Api/Sport.Api.csproj \
        -c Release \
        -o /app \
        --no-restore \
        /p:UseAppHost=false

# --- runtime stage -----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine AS runtime
RUN apk add --no-cache curl
WORKDIR /app
COPY --from=publish /app ./
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 8080
USER 1000:1000
HEALTHCHECK --interval=10s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "Sport.Api.dll"]
```

### 5.1 `apps/api/.dockerignore`

```
**/bin/
**/obj/
**/.vs/
**/TestResults/
tests/
*.user
*.suo
```

## 6. `docker-compose.yml` y `.env`

### 6.1 `docker-compose.yml` (repo root)

```yaml
name: daline-sys

services:
  postgres:
    image: postgres:18-alpine
    container_name: daline-postgres
    environment:
      POSTGRES_USER:     ${POSTGRES_USER:-sport}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:?password is required}
      POSTGRES_DB:       ${POSTGRES_DB:-sport}
    ports:
      - "${POSTGRES_PORT:-5432}:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER:-sport} -d ${POSTGRES_DB:-sport}"]
      interval: 5s
      timeout: 3s
      retries: 10
      start_period: 5s
    restart: unless-stopped

  api:
    profiles: ["api"]
    build:
      context: ./apps/api
      dockerfile: Dockerfile
    container_name: daline-api
    environment:
      ASPNETCORE_URLS:                "http://+:8080"
      ASPNETCORE_ENVIRONMENT:         "Production"
      ConnectionStrings__Postgres:    "Host=postgres;Port=5432;Database=${POSTGRES_DB:-sport};Username=${POSTGRES_USER:-sport};Password=${POSTGRES_PASSWORD}"
    ports:
      - "${API_PORT:-8080}:8080"
    depends_on:
      postgres:
        condition: service_healthy
    healthcheck:
      test: ["CMD", "curl", "--fail", "http://localhost:8080/health"]
      interval: 10s
      timeout: 3s
      retries: 3
      start_period: 10s
    restart: unless-stopped

volumes:
  postgres-data:
```

### 6.2 `.env.example` (committed)

```
POSTGRES_USER=sport
POSTGRES_PASSWORD=change-me-locally
POSTGRES_DB=sport
POSTGRES_PORT=5432
API_PORT=8080
```

### 6.3 `.gitignore` (delta)

Añadir `.env` (asegurarse de no ignorar `.env.example`).

### 6.4 UX resultante

| Comando | Levanta |
|---|---|
| `docker compose up` | postgres |
| `docker compose up -d postgres` | postgres en background |
| `docker compose --profile api up` | postgres + api |
| `docker compose --profile api up --build` | rebuild + run |
| `docker compose down` | apaga |
| `docker compose down -v` | apaga + borra volúmenes |

## 7. Tests de integración del host

Nuevo proyecto: `apps/api/tests/Sport.Api.Tests/`.

- SDK: `Microsoft.NET.Sdk` (xUnit).
- Packages:
  - `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `coverlet.collector` (versiones del resto del repo).
  - `FluentAssertions` 7.2.0 (pinned igual que el resto).
  - `Microsoft.AspNetCore.Mvc.Testing` 10.0.x.
- Project reference: `Sport.Api`.

### 7.1 Tests mínimos

```csharp
public class HostSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public HostSmokeTests(WebApplicationFactory<Program> f) => _client = f.CreateClient();

    [Fact]
    public async Task Health_returns_alive()
    {
        var res = await _client.GetAsync("/health");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Ready_returns_ready()
    {
        var res = await _client.GetAsync("/health/ready");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public void Registry_has_all_six_disciplines_registered()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var scope   = factory.Services.CreateScope();
        var registry      = scope.ServiceProvider.GetRequiredService<IDisciplineRegistry>();
        // BuildSportRegistry is called at startup so codes are populated.
        registry.RegisteredCodes.Should().HaveCount(6);
    }
}
```

`Program` debe ser accesible para `WebApplicationFactory<Program>`. Decisión: añadir `public partial class Program { }` al final de `Program.cs`. No usamos `InternalsVisibleTo`.

El test project se agrega al `Sport.slnx` y participa en `dotnet test` global.

## 8. README raíz

Nuevo archivo `README.md` en repo root, mínimo y útil. Contenido sugerido:

```markdown
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
   cd apps/api
   dotnet run --project src/Sport.Api
   ```
4. API at `http://localhost:5080`. Explore the API in Scalar UI: `http://localhost:5080/scalar/v1`.

## Run the API in Docker (production-like)

```
docker compose --profile api up --build
```

API at `http://localhost:8080`. Health at `/health`.

## Tests

```
dotnet test apps/api/Sport.slnx
```
```

## 9. Cross-cutting

- **Versionado de paquetes:** `Microsoft.AspNetCore.OpenApi` y `Scalar.AspNetCore` a la última estable al momento de implementar.
- **Tests:** mantener el patrón del repo — proyecto xUnit por componente, FluentAssertions 7.2.0 pinned.
- **Naming:** containers con prefijo `daline-` (`daline-postgres`, `daline-api`).
- **Puertos default:** API 8080 (Docker), API 5080 (IDE), Postgres 5432. Todos parametrizables vía `.env`.
- **Volumen named:** `postgres-data` (managed by Docker). Recuperable con `docker compose down` y mantenido entre restarts.

## 10. Riesgos abiertos

1. **Healthcheck depende de `curl`.** Si se cambia a imagen chiseled en el futuro, el HEALTHCHECK debe migrar a `wget` u otro binario.
2. **Hot reload no soportado** dentro de Docker en esta spec. Si en el futuro otros contributors necesitan `docker compose up` puro para todo, se agrega `docker-compose.dev.yml` override.
3. **`/health/ready` retorna 200 sin verificar Postgres.** En la spec de persistencia (próxima) debe incluir `DbContext.Database.CanConnectAsync()`.
4. **Connection string en compose se inyecta como env var** con el password en plain text. Para producción se debe migrar a Docker secrets o secret manager del orquestador.
5. **`WebApplicationFactory<Program>` requiere `Program` parcial público.** Esto modifica ligeramente el `Program.cs` minimal hosting. Documentado en sección 7.

## 11. Dependencias hacia adelante

| Spec siguiente | Depende de este spec en |
|---|---|
| Persistencia + EF Core mapping | `Sport.Api` host, `ConnectionStrings__Postgres`, `postgres` service en compose. |
| Endpoints de dominio | `Sport.Api` host (DI, OpenAPI). |
| MinIO + Mailpit en compose | Patrón de `.env` + healthchecks ya establecido. |
| CI workflow | Dockerfile self-contained; `docker compose --profile api up` smoke. |
| Logging/observability (Serilog) | `Sport.Api` host. |

## 12. Verificación post-implementación

Estos checks confirman que la spec se cumplió. Forman la última tarea del plan:

1. `cp .env.example .env && nano .env` (cambia password).
2. `docker compose up -d postgres` → en 5–10 s el container está healthy.
3. `dotnet test apps/api/Sport.slnx` → 100% verde (incluyendo `Sport.Api.Tests`).
4. `cd apps/api && dotnet run --project src/Sport.Api` desde host.
5. `curl http://localhost:5080/health` → 200.
6. `curl http://localhost:5080/openapi/v1.json` → JSON con OpenAPI doc.
7. Browser `http://localhost:5080/scalar/v1` → Scalar UI carga.
8. `docker compose --profile api up --build` → ambos containers healthy.
9. `curl http://localhost:8080/health` → 200.
10. `docker images | grep daline-api` → tamaño < 200 MB.
11. `docker compose down -v` → limpio.

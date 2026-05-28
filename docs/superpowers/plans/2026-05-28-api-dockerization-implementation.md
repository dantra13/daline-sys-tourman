# API Dockerization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create `Sport.Api` host (ASP.NET Core 10), a multi-stage Dockerfile, and a `docker-compose.yml` with PostgreSQL — producing a reproducible local environment per `docs/superpowers/specs/2026-05-28-api-dockerization-design.md`.

**Architecture:** Sport.Api is a thin ASP.NET Core host that wires `AddSportCore()` + the six discipline modules, exposes `/health`, `/health/ready`, OpenAPI doc, and Scalar UI. The Dockerfile is multi-stage (restore → publish → runtime) on `aspnet:10.0-alpine`. `docker-compose.yml` lives at repo root, runs `postgres:18-alpine` by default and the API under profile `api`.

**Tech Stack:** .NET 10 · ASP.NET Core (Microsoft.NET.Sdk.Web) · Microsoft.AspNetCore.OpenApi · Scalar.AspNetCore · Microsoft.AspNetCore.Mvc.Testing · xUnit · FluentAssertions 7.2.0 · Docker · postgres:18-alpine

**Reference:** Spec at `docs/superpowers/specs/2026-05-28-api-dockerization-design.md`. All decisions D1..D9 listed there are implemented by tasks below.

---

## Conventions

- All paths are relative to repo root unless stated otherwise.
- The .NET solution lives at `apps/api/Sport.slnx`. New projects MUST be added to it.
- Tests follow the existing TDD pattern: failing test → minimal impl → green → commit.
- Commit message prefixes: `feat(api):`, `chore:`, `docs:`, `test(api):`.
- When a step says "run from `apps/api/`", use `dotnet` commands with the project path explicitly (the harness Bash CWD persists across calls).

---

## File map

```
apps/api/src/Sport.Api/
  Sport.Api.csproj                   (Task 1)
  Program.cs                          (Task 1, evolved through Tasks 2-7)
  appsettings.json                    (Task 8)
  appsettings.Development.json        (Task 8)
  Properties/launchSettings.json      (Task 8)

apps/api/tests/Sport.Api.Tests/
  Sport.Api.Tests.csproj              (Task 1)
  HostSmokeTests.cs                   (Tasks 2-5)

apps/api/Dockerfile                   (Task 9)
apps/api/.dockerignore                (Task 9)

docker-compose.yml                    (Task 11)
.env.example                          (Task 10)
.gitignore                            (Task 10 - modify)

README.md                             (Task 12)

apps/api/Sport.slnx                   (Task 1 - modify)
```

---

## Phase A — Sport.Api host with TDD

### Task 1: Scaffold `Sport.Api` and `Sport.Api.Tests` projects

**Files:**
- Create: `apps/api/src/Sport.Api/Sport.Api.csproj`
- Create: `apps/api/src/Sport.Api/Program.cs`
- Create: `apps/api/tests/Sport.Api.Tests/Sport.Api.Tests.csproj`
- Modify: `apps/api/Sport.slnx`

- [ ] **Step 1: Scaffold `Sport.Api` as a webapi project**

Run from repo root:
```
dotnet new webapi -n Sport.Api -o apps/api/src/Sport.Api -f net10.0 --no-openapi --use-program-main false
```

Expected: project created under `apps/api/src/Sport.Api/`. Delete any sample files except `Program.cs` and `Sport.Api.csproj`:
```
rm apps/api/src/Sport.Api/WeatherForecast.cs 2>/dev/null
rm apps/api/src/Sport.Api/appsettings*.json 2>/dev/null
rm -rf apps/api/src/Sport.Api/Properties 2>/dev/null
```

- [ ] **Step 2: Add references from `Sport.Api` to core and all discipline projects**

```
dotnet add apps/api/src/Sport.Api/Sport.Api.csproj reference apps/api/src/Sport.Core/Sport.Core.csproj
dotnet add apps/api/src/Sport.Api/Sport.Api.csproj reference apps/api/src/Sport.Disciplines.FBL/Sport.Disciplines.FBL.csproj
dotnet add apps/api/src/Sport.Api/Sport.Api.csproj reference apps/api/src/Sport.Disciplines.BKB/Sport.Disciplines.BKB.csproj
dotnet add apps/api/src/Sport.Api/Sport.Api.csproj reference apps/api/src/Sport.Disciplines.BDM/Sport.Disciplines.BDM.csproj
dotnet add apps/api/src/Sport.Api/Sport.Api.csproj reference apps/api/src/Sport.Disciplines.VBV/Sport.Disciplines.VBV.csproj
dotnet add apps/api/src/Sport.Api/Sport.Api.csproj reference apps/api/src/Sport.Disciplines.BOX/Sport.Disciplines.BOX.csproj
dotnet add apps/api/src/Sport.Api/Sport.Api.csproj reference apps/api/src/Sport.Disciplines.ATH/Sport.Disciplines.ATH.csproj
```

- [ ] **Step 3: Add `Microsoft.AspNetCore.OpenApi` and `Scalar.AspNetCore` to `Sport.Api`**

```
dotnet add apps/api/src/Sport.Api/Sport.Api.csproj package Microsoft.AspNetCore.OpenApi
dotnet add apps/api/src/Sport.Api/Sport.Api.csproj package Scalar.AspNetCore
```

`dotnet add package` resolves the latest stable for the target framework automatically. Verify the versions added are the latest stable on NuGet — if a prerelease was picked, repeat with `--version <stable>`.

- [ ] **Step 4: Replace `Program.cs` with a minimal entrypoint and partial Program class**

Path: `apps/api/src/Sport.Api/Program.cs`
```csharp
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { name = "Sport.Api" }));

app.Run();

// Required so WebApplicationFactory<Program> can find Program in tests.
public partial class Program { }
```

- [ ] **Step 5: Build to verify the project compiles**

Run: `dotnet build apps/api/src/Sport.Api/Sport.Api.csproj`
Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`.

- [ ] **Step 6: Scaffold the test project**

```
dotnet new xunit -n Sport.Api.Tests -o apps/api/tests/Sport.Api.Tests -f net10.0
rm apps/api/tests/Sport.Api.Tests/UnitTest1.cs
```

- [ ] **Step 7: Add test packages and project reference**

```
dotnet add apps/api/tests/Sport.Api.Tests/Sport.Api.Tests.csproj package FluentAssertions --version 7.2.0
dotnet add apps/api/tests/Sport.Api.Tests/Sport.Api.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing
dotnet add apps/api/tests/Sport.Api.Tests/Sport.Api.Tests.csproj reference apps/api/src/Sport.Api/Sport.Api.csproj
```

- [ ] **Step 8: Add both projects to the solution**

```
dotnet sln apps/api/Sport.slnx add apps/api/src/Sport.Api/Sport.Api.csproj apps/api/tests/Sport.Api.Tests/Sport.Api.Tests.csproj
```

- [ ] **Step 9: Build the whole solution and run existing tests to confirm nothing broke**

```
dotnet build apps/api/Sport.slnx
```
Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`.

```
dotnet test "C:/Users/mella/WebstormProjects/daline-sys/apps/api/Sport.slnx"
```
Expected: 121 passed (existing) + 0 from `Sport.Api.Tests` (no tests yet).

- [ ] **Step 10: Commit**

```
git add apps/api/src/Sport.Api/ apps/api/tests/Sport.Api.Tests/ apps/api/Sport.slnx
git commit -m "chore(api): scaffold Sport.Api host and test project"
```

---

### Task 2: Failing test for `GET /health` then implement

**Files:**
- Create: `apps/api/tests/Sport.Api.Tests/HostSmokeTests.cs`
- Modify: `apps/api/src/Sport.Api/Program.cs`

- [ ] **Step 1: Write the failing test**

Path: `apps/api/tests/Sport.Api.Tests/HostSmokeTests.cs`
```csharp
using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Sport.Api.Tests;

public class HostSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HostSmokeTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_returns_200_with_status_alive()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("alive");
    }
}
```

- [ ] **Step 2: Run the test to confirm it fails**

Run: `dotnet test apps/api/tests/Sport.Api.Tests/Sport.Api.Tests.csproj --filter "FullyQualifiedName~Health_returns_200_with_status_alive"`
Expected: 1 failure — 404 returned (no `/health` route).

- [ ] **Step 3: Add the `/health` route to `Program.cs`**

Modify `apps/api/src/Sport.Api/Program.cs` — replace the body so it reads:
```csharp
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/",       () => Results.Ok(new { name = "Sport.Api" }));
app.MapGet("/health", () => Results.Ok(new { status = "alive" }));

app.Run();

public partial class Program { }
```

- [ ] **Step 4: Run the test to confirm it passes**

Run: `dotnet test apps/api/tests/Sport.Api.Tests/Sport.Api.Tests.csproj --filter "FullyQualifiedName~Health_returns_200_with_status_alive"`
Expected: 1 passed.

- [ ] **Step 5: Commit**

```
git add apps/api/src/Sport.Api/Program.cs apps/api/tests/Sport.Api.Tests/HostSmokeTests.cs
git commit -m "feat(api): add GET /health endpoint"
```

---

### Task 3: Failing test for `GET /health/ready` then implement

**Files:**
- Modify: `apps/api/tests/Sport.Api.Tests/HostSmokeTests.cs`
- Modify: `apps/api/src/Sport.Api/Program.cs`

- [ ] **Step 1: Append the failing test to `HostSmokeTests.cs`**

Add to the class (inside the existing `HostSmokeTests` class):
```csharp
    [Fact]
    public async Task Ready_returns_200_with_status_ready()
    {
        var response = await _client.GetAsync("/health/ready");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("ready");
    }
```

- [ ] **Step 2: Run the test to confirm it fails**

Run: `dotnet test apps/api/tests/Sport.Api.Tests/Sport.Api.Tests.csproj --filter "FullyQualifiedName~Ready_returns_200_with_status_ready"`
Expected: 1 failure — 404.

- [ ] **Step 3: Add the `/health/ready` route**

Modify `apps/api/src/Sport.Api/Program.cs` — add the new MapGet line so the body reads:
```csharp
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/",             () => Results.Ok(new { name = "Sport.Api" }));
app.MapGet("/health",       () => Results.Ok(new { status = "alive" }));
app.MapGet("/health/ready", () => Results.Ok(new { status = "ready" }));

app.Run();

public partial class Program { }
```

- [ ] **Step 4: Run the test to confirm it passes**

Run: `dotnet test apps/api/tests/Sport.Api.Tests/Sport.Api.Tests.csproj --filter "FullyQualifiedName~Ready_returns_200_with_status_ready"`
Expected: 1 passed.

- [ ] **Step 5: Commit**

```
git add apps/api/src/Sport.Api/Program.cs apps/api/tests/Sport.Api.Tests/HostSmokeTests.cs
git commit -m "feat(api): add GET /health/ready endpoint"
```

---

### Task 4: Failing test for discipline registry wiring, then add DI

**Files:**
- Modify: `apps/api/tests/Sport.Api.Tests/HostSmokeTests.cs`
- Modify: `apps/api/src/Sport.Api/Program.cs`

- [ ] **Step 1: Add the failing test**

Append the test class with imports first. Replace the `using` block at the top of `HostSmokeTests.cs`:
```csharp
using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Sport.Core.DisciplineRegistry;
```

Then append this `[Fact]` to the `HostSmokeTests` class:
```csharp
    [Fact]
    public void Registry_has_all_six_disciplines_registered()
    {
        using var factory  = new WebApplicationFactory<Program>();
        using var scope    = factory.Services.CreateScope();
        var registry       = scope.ServiceProvider.GetRequiredService<IDisciplineRegistry>();

        registry.RegisteredCodes.Should().HaveCount(6);
        var codes = registry.RegisteredCodes.Select(c => c.Value).ToHashSet();
        codes.Should().BeEquivalentTo(new[] { "FBL", "BKB", "BDM", "VBV", "BOX", "ATH" });
    }
```

- [ ] **Step 2: Run the test to confirm it fails**

Run: `dotnet test apps/api/tests/Sport.Api.Tests/Sport.Api.Tests.csproj --filter "FullyQualifiedName~Registry_has_all_six_disciplines_registered"`
Expected: 1 failure — `IDisciplineRegistry` not registered (service resolution throws).

- [ ] **Step 3: Wire `AddSportCore` + 6 module registrations + `BuildSportRegistry` at startup**

Modify `apps/api/src/Sport.Api/Program.cs` so the body reads:
```csharp
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

var app = builder.Build();

// Force registry construction at startup so any module registration error is fail-fast.
app.Services.BuildSportRegistry();

app.MapGet("/",             () => Results.Ok(new { name = "Sport.Api" }));
app.MapGet("/health",       () => Results.Ok(new { status = "alive" }));
app.MapGet("/health/ready", () => Results.Ok(new { status = "ready" }));

app.Run();

public partial class Program { }
```

- [ ] **Step 4: Run all `Sport.Api.Tests`**

Run: `dotnet test apps/api/tests/Sport.Api.Tests/Sport.Api.Tests.csproj`
Expected: 3 passed.

- [ ] **Step 5: Commit**

```
git add apps/api/src/Sport.Api/Program.cs apps/api/tests/Sport.Api.Tests/HostSmokeTests.cs
git commit -m "feat(api): wire SportCore DI and register six discipline modules"
```

---

### Task 5: Add OpenAPI document and Scalar UI

**Files:**
- Modify: `apps/api/tests/Sport.Api.Tests/HostSmokeTests.cs`
- Modify: `apps/api/src/Sport.Api/Program.cs`

- [ ] **Step 1: Add a failing test for the OpenAPI document endpoint**

Append to `HostSmokeTests.cs` class:
```csharp
    [Fact]
    public async Task OpenApi_document_is_served()
    {
        var response = await _client.GetAsync("/openapi/v1.json");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("openapi");
    }
```

- [ ] **Step 2: Run the test to confirm it fails**

Run: `dotnet test apps/api/tests/Sport.Api.Tests/Sport.Api.Tests.csproj --filter "FullyQualifiedName~OpenApi_document_is_served"`
Expected: 1 failure — 404.

- [ ] **Step 3: Wire OpenAPI and Scalar in `Program.cs`**

Modify `apps/api/src/Sport.Api/Program.cs` — add `AddOpenApi()` to services and `MapOpenApi()` + `MapScalarApiReference()` to the app. Final body:
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

app.Services.BuildSportRegistry();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/",             () => Results.Ok(new { name = "Sport.Api" }));
app.MapGet("/health",       () => Results.Ok(new { status = "alive" }));
app.MapGet("/health/ready", () => Results.Ok(new { status = "ready" }));

app.Run();

public partial class Program { }
```

- [ ] **Step 4: Run all tests**

Run: `dotnet test apps/api/tests/Sport.Api.Tests/Sport.Api.Tests.csproj`
Expected: 4 passed.

- [ ] **Step 5: Commit**

```
git add apps/api/src/Sport.Api/Program.cs apps/api/tests/Sport.Api.Tests/HostSmokeTests.cs
git commit -m "feat(api): expose OpenAPI document and Scalar UI"
```

---

### Task 6: Verify Scalar UI is reachable

Scalar's `MapScalarApiReference()` registers a route at `/scalar/v1` by default. Add a passing smoke test that confirms it returns 200 and an HTML response.

**Files:**
- Modify: `apps/api/tests/Sport.Api.Tests/HostSmokeTests.cs`

- [ ] **Step 1: Append the test**

Add to the class:
```csharp
    [Fact]
    public async Task Scalar_ui_is_reachable()
    {
        var response = await _client.GetAsync("/scalar/v1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var contentType = response.Content.Headers.ContentType?.MediaType;
        contentType.Should().Be("text/html");
    }
```

- [ ] **Step 2: Run all tests**

Run: `dotnet test apps/api/tests/Sport.Api.Tests/Sport.Api.Tests.csproj`
Expected: 5 passed. (If Scalar's default URL is different in the installed version, adjust the route in the test and document the change.)

- [ ] **Step 3: Commit**

```
git add apps/api/tests/Sport.Api.Tests/HostSmokeTests.cs
git commit -m "test(api): assert Scalar UI is served at /scalar/v1"
```

---

### Task 7: Run full solution test to confirm nothing regressed

- [ ] **Step 1: Run all tests across the solution**

Run: `dotnet test "C:/Users/mella/WebstormProjects/daline-sys/apps/api/Sport.slnx"`
Expected: 121 (existing) + 5 (new `Sport.Api.Tests`) = 126 passed, 0 failed.

- [ ] **Step 2: No commit required if previous tasks were committed individually**

If any uncommitted changes remain, stop and investigate.

---

## Phase B — Host configuration files

### Task 8: `appsettings.json`, `appsettings.Development.json`, `launchSettings.json`

**Files:**
- Create: `apps/api/src/Sport.Api/appsettings.json`
- Create: `apps/api/src/Sport.Api/appsettings.Development.json`
- Create: `apps/api/src/Sport.Api/Properties/launchSettings.json`

- [ ] **Step 1: Create `appsettings.json`**

Path: `apps/api/src/Sport.Api/appsettings.json`
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
  },
  "AllowedHosts": "*"
}
```

- [ ] **Step 2: Create `appsettings.Development.json`**

Path: `apps/api/src/Sport.Api/appsettings.Development.json`
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

- [ ] **Step 3: Create `launchSettings.json`**

Path: `apps/api/src/Sport.Api/Properties/launchSettings.json`
```json
{
  "profiles": {
    "Sport.Api": {
      "commandName": "Project",
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5080",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "ConnectionStrings__Postgres": "Host=localhost;Port=5432;Database=sport;Username=sport;Password=change-me-locally"
      }
    }
  }
}
```

- [ ] **Step 4: Verify the host runs locally**

Run: `dotnet run --project apps/api/src/Sport.Api/Sport.Api.csproj`
Expected: console prints "Now listening on: http://localhost:5080" within a few seconds. Hit `Ctrl+C` to stop.

- [ ] **Step 5: Run all tests once to confirm nothing changed**

Run: `dotnet test apps/api/tests/Sport.Api.Tests/Sport.Api.Tests.csproj`
Expected: 5 passed.

- [ ] **Step 6: Commit**

```
git add apps/api/src/Sport.Api/appsettings.json apps/api/src/Sport.Api/appsettings.Development.json apps/api/src/Sport.Api/Properties/launchSettings.json
git commit -m "chore(api): add appsettings and launch profile for Sport.Api"
```

---

## Phase C — Docker

### Task 9: Dockerfile and `.dockerignore`

**Files:**
- Create: `apps/api/Dockerfile`
- Create: `apps/api/.dockerignore`

- [ ] **Step 1: Create `.dockerignore`**

Path: `apps/api/.dockerignore`
```
**/bin/
**/obj/
**/.vs/
**/TestResults/
tests/
*.user
*.suo
```

- [ ] **Step 2: Create the multi-stage Dockerfile**

Path: `apps/api/Dockerfile`
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

- [ ] **Step 3: Build the image to confirm it works**

Run: `docker build -t daline-api:dev apps/api`
Expected: build succeeds; final stage tagged `daline-api:dev`.

- [ ] **Step 4: Run the image standalone to verify `/health` responds**

Run:
```
docker run --rm -d --name daline-api-smoke -p 18080:8080 daline-api:dev
```
Then poll for readiness (wait ~5 seconds first):
```
sleep 5 && curl -fsS http://localhost:18080/health
```
Expected: JSON body containing `"alive"`.

Stop the container:
```
docker rm -f daline-api-smoke
```

- [ ] **Step 5: Confirm image size is under 200 MB**

Run: `docker image inspect daline-api:dev --format '{{.Size}}'`
Expected: value less than `209715200` (200 MB in bytes). Convert with `numfmt --to=iec` if needed.

- [ ] **Step 6: Commit**

```
git add apps/api/Dockerfile apps/api/.dockerignore
git commit -m "feat(api): add multi-stage Dockerfile and .dockerignore"
```

---

### Task 10: `.env.example` and `.gitignore` update

**Files:**
- Create: `.env.example`
- Modify: `.gitignore`

- [ ] **Step 1: Create `.env.example`**

Path: `.env.example`
```
POSTGRES_USER=sport
POSTGRES_PASSWORD=change-me-locally
POSTGRES_DB=sport
POSTGRES_PORT=5432
API_PORT=8080
```

- [ ] **Step 2: Update `.gitignore` to ignore `.env`**

Append the following lines at the end of `.gitignore`:
```
# Local env vars (use .env.example as template)
.env
```

- [ ] **Step 3: Verify the example is committable and `.env` would be ignored**

Run:
```
git check-ignore .env.example
```
Expected: no output (file is NOT ignored — committable).

Then create a temporary `.env` and check it's ignored:
```
echo "POSTGRES_PASSWORD=test" > .env
git check-ignore .env
```
Expected: prints `.env` (file IS ignored).

Remove the temp file:
```
rm .env
```

- [ ] **Step 4: Commit**

```
git add .env.example .gitignore
git commit -m "chore: add .env.example and ignore .env"
```

---

### Task 11: `docker-compose.yml` and end-to-end smoke

**Files:**
- Create: `docker-compose.yml`

- [ ] **Step 1: Create `docker-compose.yml` at repo root**

Path: `docker-compose.yml`
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

- [ ] **Step 2: Prepare a local `.env` for the smoke**

Run:
```
cp .env.example .env
```
The default password `change-me-locally` is fine for the smoke. Reminder: `.env` is gitignored.

- [ ] **Step 3: Smoke 1 — postgres alone**

Run:
```
docker compose up -d postgres
```
Wait 10 seconds then check the container is healthy:
```
sleep 10 && docker inspect daline-postgres --format '{{.State.Health.Status}}'
```
Expected: `healthy`.

Bring it down:
```
docker compose down
```

- [ ] **Step 4: Smoke 2 — full stack with `--profile api`**

Run:
```
docker compose --profile api up -d --build
```
Wait ~20 seconds (the API healthcheck has a 10 s start_period plus retry intervals), then:
```
sleep 20
docker inspect daline-postgres --format '{{.State.Health.Status}}'
docker inspect daline-api --format '{{.State.Health.Status}}'
```
Expected: both `healthy`.

Hit the HTTP endpoints:
```
curl -fsS http://localhost:8080/health
curl -fsS http://localhost:8080/health/ready
curl -fsS http://localhost:8080/openapi/v1.json | head -c 200
```
Expected: 200 OK for the first two with JSON bodies containing `alive` and `ready`; the third returns JSON that starts with an OpenAPI document (`{"openapi"...`).

- [ ] **Step 5: Smoke 3 — clean shutdown**

```
docker compose --profile api down -v
```
Expected: containers stop and `postgres-data` volume is removed.

- [ ] **Step 6: Commit**

```
git add docker-compose.yml
git commit -m "feat: add docker-compose with postgres and api profile"
```

---

## Phase D — Documentation

### Task 12: README

**Files:**
- Create: `README.md`

- [ ] **Step 1: Create `README.md` at repo root**

Path: `README.md`
````markdown
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
````

- [ ] **Step 2: Commit**

```
git add README.md
git commit -m "docs: add README with local development instructions"
```

---

## Phase E — Final verification

### Task 13: Spec section 12 verification checklist

Spec section 12 lists eleven end-to-end checks. Run them in order and stop if any fails.

- [ ] **Step 1: `.env` setup**

```
cp .env.example .env
```
Edit `.env` to set a real password if you want, otherwise the default is fine.

- [ ] **Step 2: Postgres up and healthy**

```
docker compose up -d postgres
sleep 10
docker inspect daline-postgres --format '{{.State.Health.Status}}'
```
Expected: `healthy`.

- [ ] **Step 3: Full solution test**

```
dotnet test "C:/Users/mella/WebstormProjects/daline-sys/apps/api/Sport.slnx"
```
Expected: 126 passed, 0 failed.

- [ ] **Step 4: API from host**

```
dotnet run --project apps/api/src/Sport.Api/Sport.Api.csproj
```
In a second shell:
```
curl -fsS http://localhost:5080/health
curl -fsS http://localhost:5080/openapi/v1.json | head -c 200
curl -fsS -o /dev/null -w "%{http_code}\n" http://localhost:5080/scalar/v1
```
Expected: 200 with `alive`; JSON starting with `{"openapi"`; `200` status code.

Stop the host with `Ctrl+C`.

- [ ] **Step 5: Dockerized stack**

```
docker compose --profile api up -d --build
sleep 20
docker inspect daline-api --format '{{.State.Health.Status}}'
curl -fsS http://localhost:8080/health
```
Expected: `healthy`; JSON containing `alive`.

- [ ] **Step 6: Image size**

Compose tags the API image as `daline-sys-api` (project name + service name). Get the size in bytes:
```
docker image inspect daline-sys-api --format '{{.Size}}'
```
Expected: value less than `209715200` (200 MB).

For human-readable output:
```
docker images daline-sys-api --format '{{.Repository}}:{{.Tag}}  {{.Size}}'
```

- [ ] **Step 7: Clean shutdown**

```
docker compose --profile api down -v
```
Expected: containers gone, volume removed. No error output.

- [ ] **Step 8: Final commit if anything pending**

```
git status
```
Expected: `nothing to commit, working tree clean`. If there's anything left from previous tasks, commit it now with an appropriate message.

---

## Self-review checklist

After completing all tasks verify against the spec:

1. **Spec coverage** — each decision and section of `docs/superpowers/specs/2026-05-28-api-dockerization-design.md` maps to at least one task above (Section 4 → Task 1, 4, 8; Section 5 → Task 9; Section 6 → Tasks 10, 11; Section 7 → Tasks 1–6; Section 8 → Task 12; Section 12 → Task 13).
2. **No placeholders** — no `TBD`/`TODO` and every step shows full code or a concrete command.
3. **Type consistency** — `Program.cs` evolves additively through Tasks 2–5 with the final state shown verbatim in Task 5 step 3.
4. **All tests green** — `dotnet test apps/api/Sport.slnx` ends with `126 passed`.
5. **Container size** — under 200 MB (alpine + framework, no extras).
6. **Compose UX** — `docker compose up` brings postgres only; `--profile api` adds the API.

If any item fails, fix inline before declaring the plan complete.

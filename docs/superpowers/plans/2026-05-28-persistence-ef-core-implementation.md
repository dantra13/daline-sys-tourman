# Persistence (EF Core + Postgres) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the persistence layer for the core sport domain per `docs/superpowers/specs/2026-05-28-persistence-ef-core-design.md` — `Sport.Infrastructure` with EF Core 10 + Npgsql + Postgres 18, mappings for all 11 root aggregates, migration runner, `/health/ready` real check, slow-query interceptor, integration tests with Testcontainers + Respawn, and `pg_stat_statements` enabled in compose.

**Architecture:** New `Sport.Infrastructure` project (DDD/Onion) holds `SportDbContext`, configurations, interceptors and migrations. `Sport.Core` stays domain-pure (only adds `[EfCoreConverter]` attributes on its Vogen VOs — Vogen generates EF converters in the same namespace, no EF Core reference added to Core). `Sport.Api` references Infrastructure and wires `AddSportInfrastructure()`. Auto-migrate on Development, manual elsewhere.

**Tech Stack:** .NET 10 · EF Core 10 · Npgsql.EntityFrameworkCore.PostgreSQL · EFCore.NamingConventions (snake_case) · Vogen 8 with `[EfCoreConverter]` · Microsoft.AspNetCore.Diagnostics.HealthChecks · Testcontainers.PostgreSql · Respawn · xUnit · FluentAssertions 7.2.0

**Reference:** Spec at `docs/superpowers/specs/2026-05-28-persistence-ef-core-design.md`. All decisions D1..D14 are implemented by tasks below.

---

## Conventions used in this plan

- All paths are relative to repo root unless stated otherwise.
- The .NET solution lives at `apps/api/Sport.slnx`. New projects MUST be added to it.
- Tests follow TDD: failing test → minimal impl → green → commit, except for mechanical scaffolding tasks.
- Commit message prefixes: `chore:`, `feat(core):`, `feat(infra):`, `feat(api):`, `test(infra):`, `docs:`.
- Bash CWD persists across calls in the harness. Use absolute paths or be careful with `cd`.
- All Vogen attributes used in `Sport.Core` come from `Vogen` (already a dependency since Phase B of the core spec).

---

## File map

```
.config/dotnet-tools.json                                 (Task 1)

apps/api/src/Sport.Core/                                  (Task 2: add [EfCoreConverter] to 23 VOs)
  Competitions/CompetitionId.cs                           (modify)
  Competitions/CompetitionDisciplineId.cs                 (modify)
  Competitions/CompetitionCode.cs                         (modify)
  Structure/EventId.cs                                    (modify)
  Structure/PhaseId.cs                                    (modify)
  Structure/UnitId.cs                                     (modify)
  Structure/SubunitId.cs                                  (modify)
  Structure/Rsc.cs                                        (modify)
  Structure/EventTypeCode.cs                              (modify)
  Structure/EventModifierCode.cs                          (modify)
  Structure/PhaseCode.cs                                  (modify)
  Structure/UnitCode.cs                                   (modify)
  Structure/SubunitCode.cs                                (modify)
  Participants/PersonId.cs                                (modify)
  Participants/OrganisationId.cs                          (modify)
  Participants/TeamId.cs                                  (modify)
  Participants/EntryId.cs                                 (modify)
  Participants/OrganisationCode.cs                        (modify)
  Participants/TeamCode.cs                                (modify)
  Participants/Bib.cs                                     (modify)
  Officials/OfficialAssignmentId.cs                       (modify)
  Officials/FunctionCode.cs                               (modify)
  DisciplineRegistry/DisciplineCode.cs                    (modify)

apps/api/src/Sport.Infrastructure/                        (Tasks 3-12)
  Sport.Infrastructure.csproj                             (Task 3)
  SportDbContext.cs                                       (Task 6)
  DependencyInjection.cs                                  (Task 7)
  SportMigrationRunner.cs                                 (Task 8)
  Interceptors/SlowQueryInterceptor.cs                    (Task 5)
  Interceptors/SlowQueryOptions.cs                        (Task 5)
  Configurations/Competitions/
    CompetitionConfiguration.cs                           (Task 10)
    CompetitionDisciplineConfiguration.cs                 (Task 10)
  Configurations/Structure/
    EventConfiguration.cs                                 (Task 11)
    PhaseConfiguration.cs                                 (Task 11)
    UnitConfiguration.cs                                  (Task 11)
    SubunitConfiguration.cs                               (Task 11)
  Configurations/Participants/
    PersonConfiguration.cs                                (Task 12)
    OrganisationConfiguration.cs                          (Task 12)
    TeamConfiguration.cs                                  (Task 12)
    EntryConfiguration.cs                                 (Task 12)
    CompositionMemberConfiguration.cs                     (Task 12)
  Configurations/Officials/
    OfficialAssignmentConfiguration.cs                    (Task 13)
  Migrations/                                             (Task 14: generated)
    *_InitialCreate.cs
    *_InitialCreate.Designer.cs
    SportDbContextModelSnapshot.cs

apps/api/tests/Sport.Infrastructure.Tests/                (Tasks 4 + 9-16)
  Sport.Infrastructure.Tests.csproj                       (Task 4)
  Fixtures/PostgresFixture.cs                             (Task 9)
  Fixtures/SportDbContextFixture.cs                       (Task 9)
  Fixtures/PostgresCollection.cs                          (Task 9)
  TestHelpers/SqlCommandCounterInterceptor.cs             (Task 9)
  TestHelpers/FakeRegistry.cs                             (Task 9)
  Interceptors/SlowQueryInterceptorTests.cs               (Task 5, lives in Tests project)
  Persistence/CompetitionPersistenceTests.cs              (Task 10)
  Persistence/EventPersistenceTests.cs                    (Task 11)
  Persistence/PhaseUnitSubunitPersistenceTests.cs         (Task 11)
  Persistence/PersonOrganisationTeamPersistenceTests.cs   (Task 12)
  Persistence/EntryPersistenceTests.cs                    (Task 12)
  Persistence/OfficialAssignmentPersistenceTests.cs       (Task 13)
  Schema/ForeignKeyIndexTests.cs                          (Task 15)
  HealthChecks/ReadyEndpointTests.cs                      (Task 17)

apps/api/src/Sport.Api/
  Sport.Api.csproj                                        (Task 16: add reference)
  Program.cs                                              (Task 16: AddSportInfrastructure + runner + /health/ready)
  appsettings.json                                        (Task 7: add SlowQuery section)

apps/api/tests/Sport.Architecture.Tests/
  ArchitectureRules.cs                                    (Task 18: add 5 new rules)

apps/api/docker/postgres/init/
  01-create-extension.sql                                 (Task 19)

docker-compose.yml                                        (Task 19: add pg_stat_statements command + mount)

README.md                                                 (Task 20: append Auditing queries section)

apps/api/Sport.slnx                                       (Tasks 3, 4: add new projects)
```

---

## Phase A — Tooling and Sport.Core changes

### Task 1: Install `dotnet-ef` as a tool manifest

**Files:**
- Create: `.config/dotnet-tools.json`

- [ ] **Step 1: Create the tool manifest at repo root**

Run from repo root:
```
dotnet new tool-manifest
```

This creates `.config/dotnet-tools.json` with an empty `tools` map.

- [ ] **Step 2: Install dotnet-ef into the manifest**

```
dotnet tool install dotnet-ef
```

`dotnet add package` resolves the latest stable. Verify it picked an EF Core 10.x version.

- [ ] **Step 3: Verify the tool can be invoked**

```
dotnet ef --version
```
Expected: prints the version (e.g. `Entity Framework Core .NET Command-line Tools 10.0.x`).

- [ ] **Step 4: Commit**

```
git add .config/dotnet-tools.json
git commit -m "chore: install dotnet-ef as local tool"
```

---

### Task 2: Add `[EfCoreConverter]` to all 23 Vogen value objects in `Sport.Core`

**Files:** modify 23 files in `apps/api/src/Sport.Core/`. Listed in the File map.

The change per file is identical in shape: add `using Vogen;` (already present), add the attribute `[EfCoreConverter]` above the type, no other change. Vogen 8 generates a partial class `{TypeName}EfCoreValueConverter` in the same namespace.

- [ ] **Step 1: Modify `Competitions/CompetitionId.cs`**

Path: `apps/api/src/Sport.Core/Competitions/CompetitionId.cs` — replace the existing type declaration so it reads:
```csharp
using Vogen;

namespace Sport.Core.Competitions;

[ValueObject<Guid>]
[EfCoreConverter]
public readonly partial struct CompetitionId
{
    public static CompetitionId New() => From(Guid.CreateVersion7());
}
```

- [ ] **Step 2: Modify the other 22 VOs**

Apply the same change (add `[EfCoreConverter]` line right after `[ValueObject<...>]`) to each of:
- `apps/api/src/Sport.Core/Competitions/CompetitionDisciplineId.cs`
- `apps/api/src/Sport.Core/Competitions/CompetitionCode.cs`
- `apps/api/src/Sport.Core/Structure/EventId.cs`
- `apps/api/src/Sport.Core/Structure/PhaseId.cs`
- `apps/api/src/Sport.Core/Structure/UnitId.cs`
- `apps/api/src/Sport.Core/Structure/SubunitId.cs`
- `apps/api/src/Sport.Core/Structure/Rsc.cs`
- `apps/api/src/Sport.Core/Structure/EventTypeCode.cs`
- `apps/api/src/Sport.Core/Structure/EventModifierCode.cs`
- `apps/api/src/Sport.Core/Structure/PhaseCode.cs`
- `apps/api/src/Sport.Core/Structure/UnitCode.cs`
- `apps/api/src/Sport.Core/Structure/SubunitCode.cs`
- `apps/api/src/Sport.Core/Participants/PersonId.cs`
- `apps/api/src/Sport.Core/Participants/OrganisationId.cs`
- `apps/api/src/Sport.Core/Participants/TeamId.cs`
- `apps/api/src/Sport.Core/Participants/EntryId.cs`
- `apps/api/src/Sport.Core/Participants/OrganisationCode.cs`
- `apps/api/src/Sport.Core/Participants/TeamCode.cs`
- `apps/api/src/Sport.Core/Participants/Bib.cs`
- `apps/api/src/Sport.Core/Officials/OfficialAssignmentId.cs`
- `apps/api/src/Sport.Core/Officials/FunctionCode.cs`
- `apps/api/src/Sport.Core/DisciplineRegistry/DisciplineCode.cs`

Each gets the single attribute line. Body unchanged.

- [ ] **Step 3: Build and verify generators run**

```
dotnet build apps/api/Sport.slnx
```
Expected: 0 warnings, 0 errors. Each modified VO now has a generated `{Name}EfCoreValueConverter` accessible at compile time (no source visible — Vogen emits it as a roslyn analyzer).

If the build fails with `EfCoreConverter` not found, the Vogen package may not have it bundled — install `Vogen.EfCoreSupport` as a hint, otherwise rerun with `--verbosity normal` to see the analyzer error.

- [ ] **Step 4: Run the existing test suite to confirm no regression**

```
dotnet test "C:/Users/mella/WebstormProjects/daline-sys/apps/api/Sport.slnx"
```
Expected: 126 passed (existing). The attribute is purely additive.

- [ ] **Step 5: Commit**

```
git add apps/api/src/Sport.Core/
git commit -m "feat(core): add [EfCoreConverter] to all Vogen value objects"
```

---

## Phase B — Scaffold `Sport.Infrastructure` and tests project

### Task 3: Create `Sport.Infrastructure` class library

**Files:**
- Create: `apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj`
- Create: `apps/api/src/Sport.Infrastructure/Class1.cs` (will be deleted)
- Modify: `apps/api/Sport.slnx`

- [ ] **Step 1: Scaffold the project**

```
dotnet new classlib -n Sport.Infrastructure -o apps/api/src/Sport.Infrastructure -f net10.0
rm apps/api/src/Sport.Infrastructure/Class1.cs
```

- [ ] **Step 2: Add project reference to Sport.Core**

```
dotnet add apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj reference apps/api/src/Sport.Core/Sport.Core.csproj
```

- [ ] **Step 3: Add EF Core, Npgsql, naming conventions, health checks, hosting abstractions and Vogen packages**

```
dotnet add apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj package Microsoft.EntityFrameworkCore
dotnet add apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Relational
dotnet add apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj package EFCore.NamingConventions
dotnet add apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj package Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore
dotnet add apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj package Microsoft.Extensions.Hosting.Abstractions
dotnet add apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj package Vogen
```

`dotnet add package` resolves the latest stable compatible with `net10.0`. Verify all picks are 10.x (or latest stable for non-MS packages like EFCore.NamingConventions, Vogen).

- [ ] **Step 4: Mark `Microsoft.EntityFrameworkCore.Design` as `PrivateAssets="all"`**

Edit `apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj` — replace the existing `<PackageReference Include="Microsoft.EntityFrameworkCore.Design" ... />` line with:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0" PrivateAssets="all" />
```
(Replace `10.0.0` with the actual version that `dotnet add` picked.)

This prevents the Design package from being a transitive dependency.

- [ ] **Step 5: Add the project to the solution**

```
dotnet sln apps/api/Sport.slnx add apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj
```

- [ ] **Step 6: Build to confirm everything resolves**

```
dotnet build apps/api/Sport.slnx
```
Expected: 0 warnings, 0 errors.

- [ ] **Step 7: Commit**

```
git add apps/api/src/Sport.Infrastructure/ apps/api/Sport.slnx
git commit -m "chore(infra): scaffold Sport.Infrastructure project"
```

---

### Task 4: Create `Sport.Infrastructure.Tests` project

**Files:**
- Create: `apps/api/tests/Sport.Infrastructure.Tests/Sport.Infrastructure.Tests.csproj`
- Modify: `apps/api/Sport.slnx`

- [ ] **Step 1: Scaffold the xunit project**

```
dotnet new xunit -n Sport.Infrastructure.Tests -o apps/api/tests/Sport.Infrastructure.Tests -f net10.0
rm apps/api/tests/Sport.Infrastructure.Tests/UnitTest1.cs
```

- [ ] **Step 2: Pin `coverlet.collector` to 10.0.1 in the csproj**

Edit `apps/api/tests/Sport.Infrastructure.Tests/Sport.Infrastructure.Tests.csproj`. Replace the line `<PackageReference Include="coverlet.collector" Version="6.0.4" />` with:
```xml
<PackageReference Include="coverlet.collector" Version="10.0.1" />
```
(Align with `Sport.Core.Tests`.)

- [ ] **Step 3: Add test packages**

```
dotnet add apps/api/tests/Sport.Infrastructure.Tests/Sport.Infrastructure.Tests.csproj package FluentAssertions --version 7.2.0
dotnet add apps/api/tests/Sport.Infrastructure.Tests/Sport.Infrastructure.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing
dotnet add apps/api/tests/Sport.Infrastructure.Tests/Sport.Infrastructure.Tests.csproj package Testcontainers.PostgreSql
dotnet add apps/api/tests/Sport.Infrastructure.Tests/Sport.Infrastructure.Tests.csproj package Respawn
dotnet add apps/api/tests/Sport.Infrastructure.Tests/Sport.Infrastructure.Tests.csproj package Npgsql
dotnet add apps/api/tests/Sport.Infrastructure.Tests/Sport.Infrastructure.Tests.csproj package Microsoft.EntityFrameworkCore.Design
```

- [ ] **Step 4: Add project references**

```
dotnet add apps/api/tests/Sport.Infrastructure.Tests/Sport.Infrastructure.Tests.csproj reference apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj
dotnet add apps/api/tests/Sport.Infrastructure.Tests/Sport.Infrastructure.Tests.csproj reference apps/api/src/Sport.Api/Sport.Api.csproj
```

(The reference to `Sport.Api` is needed by `ReadyEndpointTests` for `WebApplicationFactory<Program>`. It also gives access to Sport.Core through transitive refs.)

- [ ] **Step 5: Add to solution and build**

```
dotnet sln apps/api/Sport.slnx add apps/api/tests/Sport.Infrastructure.Tests/Sport.Infrastructure.Tests.csproj
dotnet build apps/api/Sport.slnx
```
Expected: 0 warnings, 0 errors.

- [ ] **Step 6: Commit**

```
git add apps/api/tests/Sport.Infrastructure.Tests/ apps/api/Sport.slnx
git commit -m "chore(infra): scaffold Sport.Infrastructure.Tests project"
```

---

## Phase C — Slow-query interceptor (smallest unit we can TDD)

### Task 5: `SlowQueryInterceptor` and `SlowQueryOptions`

**Files:**
- Create: `apps/api/src/Sport.Infrastructure/Interceptors/SlowQueryOptions.cs`
- Create: `apps/api/src/Sport.Infrastructure/Interceptors/SlowQueryInterceptor.cs`
- Create: `apps/api/tests/Sport.Infrastructure.Tests/Interceptors/SlowQueryInterceptorTests.cs`

- [ ] **Step 1: Write the failing tests**

Path: `apps/api/tests/Sport.Infrastructure.Tests/Interceptors/SlowQueryInterceptorTests.cs`
```csharp
using System.Data.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Sport.Infrastructure.Interceptors;

namespace Sport.Infrastructure.Tests.Interceptors;

public class SlowQueryInterceptorTests
{
    [Fact]
    public void LogsWarning_When_DurationExceedsThreshold()
    {
        var logger = new ListLogger<SlowQueryInterceptor>();
        var options = Options.Create(new SlowQueryOptions { ThresholdMs = 100 });
        var interceptor = new SlowQueryInterceptor(logger, options);

        var command = Mock.Of<DbCommand>(c => c.CommandText == "SELECT 1");
        var eventData = MakeEventData(command, TimeSpan.FromMilliseconds(150));

        interceptor.ReaderExecuted(command, eventData, result: null!);

        logger.Entries.Should().ContainSingle(e =>
            e.Level == LogLevel.Warning &&
            e.Message.Contains("Slow query") &&
            e.Message.Contains("150"));
    }

    [Fact]
    public void DoesNotLog_When_DurationBelowThreshold()
    {
        var logger = new ListLogger<SlowQueryInterceptor>();
        var options = Options.Create(new SlowQueryOptions { ThresholdMs = 100 });
        var interceptor = new SlowQueryInterceptor(logger, options);

        var command = Mock.Of<DbCommand>(c => c.CommandText == "SELECT 1");
        var eventData = MakeEventData(command, TimeSpan.FromMilliseconds(50));

        interceptor.ReaderExecuted(command, eventData, result: null!);

        logger.Entries.Should().BeEmpty();
    }

    private static CommandExecutedEventData MakeEventData(DbCommand command, TimeSpan duration)
    {
        return new CommandExecutedEventData(
            eventDefinition: null!,
            messageGenerator: null!,
            connection: null!,
            command: command,
            context: null,
            executeMethod: DbCommandMethod.ExecuteReader,
            commandId: Guid.NewGuid(),
            connectionId: Guid.NewGuid(),
            async: false,
            startTime: DateTimeOffset.UtcNow,
            duration: duration,
            commandSource: CommandSource.Unknown);
    }

    private sealed class ListLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = new();
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(
            LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
                => Entries.Add((logLevel, formatter(state, exception)));

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
```

Add Moq package to the test project:
```
dotnet add apps/api/tests/Sport.Infrastructure.Tests/Sport.Infrastructure.Tests.csproj package Moq
```

- [ ] **Step 2: Run the tests to confirm they fail**

```
dotnet test apps/api/tests/Sport.Infrastructure.Tests/Sport.Infrastructure.Tests.csproj --filter "FullyQualifiedName~SlowQueryInterceptorTests"
```
Expected: build fails — `SlowQueryInterceptor` and `SlowQueryOptions` not defined.

- [ ] **Step 3: Implement `SlowQueryOptions`**

Path: `apps/api/src/Sport.Infrastructure/Interceptors/SlowQueryOptions.cs`
```csharp
namespace Sport.Infrastructure.Interceptors;

public sealed class SlowQueryOptions
{
    public int ThresholdMs { get; set; } = 200;
}
```

- [ ] **Step 4: Implement `SlowQueryInterceptor`**

Path: `apps/api/src/Sport.Infrastructure/Interceptors/SlowQueryInterceptor.cs`
```csharp
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sport.Infrastructure.Interceptors;

public sealed class SlowQueryInterceptor(
    ILogger<SlowQueryInterceptor> logger,
    IOptions<SlowQueryOptions> options) : DbCommandInterceptor
{
    public override DbDataReader ReaderExecuted(
        DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        LogIfSlow(command, eventData);
        return result;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData,
        DbDataReader result, CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData);
        return ValueTask.FromResult(result);
    }

    public override int NonQueryExecuted(
        DbCommand command, CommandExecutedEventData eventData, int result)
    {
        LogIfSlow(command, eventData);
        return result;
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData,
        int result, CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData);
        return ValueTask.FromResult(result);
    }

    public override object? ScalarExecuted(
        DbCommand command, CommandExecutedEventData eventData, object? result)
    {
        LogIfSlow(command, eventData);
        return result;
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData,
        object? result, CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData);
        return ValueTask.FromResult(result);
    }

    private void LogIfSlow(DbCommand command, CommandExecutedEventData eventData)
    {
        var thresholdMs = options.Value.ThresholdMs;
        var durationMs = eventData.Duration.TotalMilliseconds;
        if (durationMs < thresholdMs) return;

        logger.LogWarning(
            "Slow query: {DurationMs:F0} ms (threshold {ThresholdMs} ms). SQL: {CommandText}",
            durationMs, thresholdMs, command.CommandText);
    }
}
```

- [ ] **Step 5: Run the tests**

```
dotnet test apps/api/tests/Sport.Infrastructure.Tests/Sport.Infrastructure.Tests.csproj --filter "FullyQualifiedName~SlowQueryInterceptorTests"
```
Expected: 2 passed.

- [ ] **Step 6: Commit**

```
git add apps/api/src/Sport.Infrastructure/Interceptors/ apps/api/tests/Sport.Infrastructure.Tests/ apps/api/tests/Sport.Infrastructure.Tests/Sport.Infrastructure.Tests.csproj
git commit -m "feat(infra): add SlowQueryInterceptor with configurable threshold"
```

---

## Phase D — DbContext, DI, Migration Runner

### Task 6: `SportDbContext` (without configurations yet)

**Files:**
- Create: `apps/api/src/Sport.Infrastructure/SportDbContext.cs`

- [ ] **Step 1: Implement the DbContext**

Path: `apps/api/src/Sport.Infrastructure/SportDbContext.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using Sport.Core.Competitions;
using Sport.Core.Officials;
using Sport.Core.Participants;
using Sport.Core.Structure;

namespace Sport.Infrastructure;

public sealed class SportDbContext(DbContextOptions<SportDbContext> options) : DbContext(options)
{
    public DbSet<Competition>        Competitions        => Set<Competition>();
    public DbSet<Event>              Events              => Set<Event>();
    public DbSet<Phase>              Phases              => Set<Phase>();
    public DbSet<Unit>               Units               => Set<Unit>();
    public DbSet<Subunit>            Subunits            => Set<Subunit>();
    public DbSet<Person>             Persons             => Set<Person>();
    public DbSet<Organisation>       Organisations       => Set<Organisation>();
    public DbSet<Team>               Teams               => Set<Team>();
    public DbSet<Entry>              Entries             => Set<Entry>();
    public DbSet<OfficialAssignment> OfficialAssignments => Set<OfficialAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SportDbContext).Assembly);
    }
}
```

- [ ] **Step 2: Build to verify compilation**

```
dotnet build apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj
```
Expected: 0 warnings, 0 errors. (No configurations registered yet, but `ApplyConfigurationsFromAssembly` won't fail with zero matches.)

- [ ] **Step 3: Commit**

```
git add apps/api/src/Sport.Infrastructure/SportDbContext.cs
git commit -m "feat(infra): add SportDbContext with DbSets for the 10 root aggregates"
```

---

### Task 7: `AddSportInfrastructure` DI extension + appsettings SlowQuery section

**Files:**
- Create: `apps/api/src/Sport.Infrastructure/DependencyInjection.cs`
- Modify: `apps/api/src/Sport.Api/appsettings.json`

- [ ] **Step 1: Implement `DependencyInjection.cs`**

Path: `apps/api/src/Sport.Infrastructure/DependencyInjection.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sport.Infrastructure.Interceptors;

namespace Sport.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSportInfrastructure(this IServiceCollection services)
    {
        services.AddOptions<SlowQueryOptions>().BindConfiguration("SlowQuery");
        services.AddSingleton<SlowQueryInterceptor>();

        services.AddDbContext<SportDbContext>((sp, options) =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var env = sp.GetRequiredService<IHostEnvironment>();

            options
                .UseNpgsql(
                    cfg.GetConnectionString("Postgres"),
                    npgsql => npgsql.MigrationsAssembly(typeof(SportDbContext).Assembly.FullName))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<SlowQueryInterceptor>())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution)
                .ConfigureWarnings(w => w
                    .Throw(RelationalEventId.MultipleCollectionIncludeWarning)
                    .Throw(CoreEventId.RowLimitingOperationWithoutOrderByWarning)
                    .Throw(RelationalEventId.PendingModelChangesWarning));

            if (env.IsDevelopment())
            {
                options
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
                    .LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name },
                           LogLevel.Information);
            }
        });

        services.AddHealthChecks()
            .AddDbContextCheck<SportDbContext>(name: "postgres", tags: new[] { "ready" });

        services.AddScoped<SportMigrationRunner>();
        return services;
    }
}
```

- [ ] **Step 2: Add SlowQuery section to appsettings.json**

Modify `apps/api/src/Sport.Api/appsettings.json` — replace its content with:
```json
{
  "ConnectionStrings": {
    "Postgres": ""
  },
  "SlowQuery": {
    "ThresholdMs": 200
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

- [ ] **Step 3: Build to verify (note: SportMigrationRunner does not yet exist — expect a compile error)**

```
dotnet build apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj
```
Expected: 1 error — `SportMigrationRunner` not defined. This is OK; Task 8 creates it.

- [ ] **Step 4: Skip commit until Task 8 succeeds**

Wait — combined commit at end of Task 8.

---

### Task 8: `SportMigrationRunner`

**Files:**
- Create: `apps/api/src/Sport.Infrastructure/SportMigrationRunner.cs`

- [ ] **Step 1: Implement the runner**

Path: `apps/api/src/Sport.Infrastructure/SportMigrationRunner.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Sport.Infrastructure;

public sealed class SportMigrationRunner(
    SportDbContext db,
    IHostEnvironment env,
    ILogger<SportMigrationRunner> logger)
{
    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        if (!env.IsDevelopment())
        {
            logger.LogInformation(
                "Auto-migration skipped (env={Env}). Apply migrations manually via `dotnet ef database update`.",
                env.EnvironmentName);
            return;
        }

        var pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count == 0)
        {
            logger.LogInformation("No pending migrations.");
            return;
        }

        logger.LogInformation(
            "Applying {N} pending migrations: {Names}",
            pending.Count, string.Join(", ", pending));
        await db.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Migrations applied.");
    }
}
```

- [ ] **Step 2: Build the solution**

```
dotnet build apps/api/Sport.slnx
```
Expected: 0 warnings, 0 errors.

- [ ] **Step 3: Run the full test suite to confirm nothing regressed**

```
dotnet test "C:/Users/mella/WebstormProjects/daline-sys/apps/api/Sport.slnx"
```
Expected: 128 passed (126 existing + 2 new from `SlowQueryInterceptorTests`).

- [ ] **Step 4: Commit Tasks 7 and 8 together**

```
git add apps/api/src/Sport.Infrastructure/DependencyInjection.cs apps/api/src/Sport.Infrastructure/SportMigrationRunner.cs apps/api/src/Sport.Api/appsettings.json
git commit -m "feat(infra): add AddSportInfrastructure DI extension and SportMigrationRunner"
```

---

## Phase E — Test fixtures and helpers

### Task 9: `PostgresFixture`, `SportDbContextFixture`, `PostgresCollection`, `SqlCommandCounterInterceptor`, `FakeRegistry`

**Files:**
- Create: `apps/api/tests/Sport.Infrastructure.Tests/Fixtures/PostgresFixture.cs`
- Create: `apps/api/tests/Sport.Infrastructure.Tests/Fixtures/PostgresCollection.cs`
- Create: `apps/api/tests/Sport.Infrastructure.Tests/Fixtures/SportDbContextFixture.cs`
- Create: `apps/api/tests/Sport.Infrastructure.Tests/TestHelpers/SqlCommandCounterInterceptor.cs`
- Create: `apps/api/tests/Sport.Infrastructure.Tests/TestHelpers/FakeRegistry.cs`

- [ ] **Step 1: Implement `PostgresFixture`**

Path: `apps/api/tests/Sport.Infrastructure.Tests/Fixtures/PostgresFixture.cs`
```csharp
using Testcontainers.PostgreSql;

namespace Sport.Infrastructure.Tests.Fixtures;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:18-alpine")
        .WithDatabase("sport_test")
        .WithUsername("sport")
        .WithPassword("test-password")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync() => await _container.StartAsync();
    public async Task DisposeAsync()    => await _container.DisposeAsync().AsTask();
}
```

- [ ] **Step 2: Implement `PostgresCollection`**

Path: `apps/api/tests/Sport.Infrastructure.Tests/Fixtures/PostgresCollection.cs`
```csharp
namespace Sport.Infrastructure.Tests.Fixtures;

[CollectionDefinition("Postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture> { }
```

- [ ] **Step 3: Implement `SportDbContextFixture`**

Path: `apps/api/tests/Sport.Infrastructure.Tests/Fixtures/SportDbContextFixture.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using Respawn;

namespace Sport.Infrastructure.Tests.Fixtures;

public sealed class SportDbContextFixture(PostgresFixture pg) : IAsyncLifetime
{
    private Respawner? _respawner;

    public SportDbContext CreateContext(params IInterceptor[] extra)
    {
        var options = new DbContextOptionsBuilder<SportDbContext>()
            .UseNpgsql(pg.ConnectionString)
            .UseSnakeCaseNamingConvention()
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution)
            .AddInterceptors(extra)
            .Options;
        return new SportDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await using var ctx = CreateContext();
        await ctx.Database.MigrateAsync();

        await using var conn = new NpgsqlConnection(pg.ConnectionString);
        await conn.OpenAsync();
        _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" },
            TablesToIgnore = new[] { new Respawn.Graph.Table("__EFMigrationsHistory") },
        });
    }

    public async Task ResetAsync()
    {
        await using var conn = new NpgsqlConnection(pg.ConnectionString);
        await conn.OpenAsync();
        await _respawner!.ResetAsync(conn);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
```

- [ ] **Step 4: Implement `SqlCommandCounterInterceptor`**

Path: `apps/api/tests/Sport.Infrastructure.Tests/TestHelpers/SqlCommandCounterInterceptor.cs`
```csharp
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Sport.Infrastructure.Tests.TestHelpers;

public sealed class SqlCommandCounterInterceptor : DbCommandInterceptor
{
    public List<string> Commands { get; } = new();

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        Commands.Add(command.CommandText);
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData,
        InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        Commands.Add(command.CommandText);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command, CommandEventData eventData,
        InterceptionResult<int> result)
    {
        Commands.Add(command.CommandText);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command, CommandEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Commands.Add(command.CommandText);
        return ValueTask.FromResult(result);
    }
}
```

- [ ] **Step 5: Implement `FakeRegistry` (reusable across persistence tests)**

Path: `apps/api/tests/Sport.Infrastructure.Tests/TestHelpers/FakeRegistry.cs`
```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Participants;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Infrastructure.Tests.TestHelpers;

public sealed class FakeRegistry : IDisciplineRegistry
{
    public HashSet<DisciplineCode> SupportedCodes { get; } = new();
    public Dictionary<DisciplineCode, IReadOnlySet<GenderCode>> GendersByCode { get; } = new();

    public IDisciplineModule Get(DisciplineCode code) =>
        new FakeModule(code, GendersByCode[code]);
    public bool IsRegistered(DisciplineCode code) => SupportedCodes.Contains(code);
    public IReadOnlyCollection<DisciplineCode> RegisteredCodes => SupportedCodes.ToArray();
    public IReadOnlyCollection<FunctionDescriptor> CommonFunctions => Array.Empty<FunctionDescriptor>();

    private sealed class FakeModule(DisciplineCode code, IReadOnlySet<GenderCode> genders) : IDisciplineModule
    {
        public DisciplineCode Code { get; } = code;
        public string DisplayName => "fake";
        public IReadOnlySet<GenderCode> SupportedGenders { get; } = genders;
        public IReadOnlyCollection<EventTypeDescriptor> EventTypes => Array.Empty<EventTypeDescriptor>();
        public IPhaseCatalog PhaseCatalog => throw new NotImplementedException();
        public IUnitCodeStrategy UnitCodeStrategy => throw new NotImplementedException();
        public IReadOnlyCollection<FunctionDescriptor> Functions => Array.Empty<FunctionDescriptor>();
        public IEntryRules EntryRules => throw new NotImplementedException();
        public Result ValidateEventType(EventTypeCode type, EventModifierCode? modifier) => Result.Ok();
        public Result ValidatePhaseForEventType(EventTypeCode type, PhaseCode phase) => Result.Ok();
        public Result ValidateUnitCode(EventTypeCode type, PhaseCode phase, UnitCode code) => Result.Ok();
        public Result ValidateEntry(EntryCandidate candidate) => Result.Ok();
        public Result ValidateOfficialFunctionInScope(FunctionCode function, ScopeLevel level) => Result.Ok();
    }
}
```

- [ ] **Step 6: Build to confirm compiles**

```
dotnet build apps/api/tests/Sport.Infrastructure.Tests/Sport.Infrastructure.Tests.csproj
```
Expected: 0 warnings, 0 errors.

(Don't run tests yet — no persistence tests use the fixture; migrations would fail because no configurations exist.)

- [ ] **Step 7: Commit**

```
git add apps/api/tests/Sport.Infrastructure.Tests/Fixtures/ apps/api/tests/Sport.Infrastructure.Tests/TestHelpers/
git commit -m "test(infra): add Postgres fixture, DbContext fixture, command counter and fake registry"
```

---

## Phase F — Configurations, one aggregate at a time, TDD

Each configuration task follows the same shape: write a failing roundtrip test against the fixture; add the configuration; the test goes green when a migration is generated (which we do once at the end). To unblock the early tests, we generate an interim migration after the first aggregate is mapped (Task 10), then update it after each subsequent task.

Alternative: each task generates its migration. This produces lots of small migrations. To keep history clean, we instead generate ONE migration at the end (Task 14), and tests in Tasks 10–13 are written but skipped until Task 14 lands. We follow the second approach.

> **Important:** Tasks 10–13 implement configurations and write tests that target the future migrated schema. The test code is committed but marked `[Fact(Skip = "Run after InitialCreate migration is generated in Task 14")]`. After Task 14, we unskip in a single sweep and run the suite.

This keeps each task bite-sized and avoids regenerating migrations 5 times.

---

### Task 10: Competitions configurations + skipped roundtrip test

**Files:**
- Create: `apps/api/src/Sport.Infrastructure/Configurations/Competitions/CompetitionConfiguration.cs`
- Create: `apps/api/src/Sport.Infrastructure/Configurations/Competitions/CompetitionDisciplineConfiguration.cs`
- Create: `apps/api/tests/Sport.Infrastructure.Tests/Persistence/CompetitionPersistenceTests.cs`

- [ ] **Step 1: Implement `CompetitionConfiguration.cs`**

Path: `apps/api/src/Sport.Infrastructure/Configurations/Competitions/CompetitionConfiguration.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.Competitions;

namespace Sport.Infrastructure.Configurations.Competitions;

internal sealed class CompetitionConfiguration : IEntityTypeConfiguration<Competition>
{
    public void Configure(EntityTypeBuilder<Competition> b)
    {
        b.ToTable("competitions");

        b.HasKey(c => c.Id);
        b.Property(c => c.Id)
            .HasConversion<CompetitionIdEfCoreValueConverter>();

        b.Property(c => c.Code)
            .HasConversion<CompetitionCodeEfCoreValueConverter>()
            .HasMaxLength(64)
            .IsRequired();
        b.HasIndex(c => c.Code).IsUnique();

        b.Property(c => c.Name).HasMaxLength(200).IsRequired();

        b.OwnsOne(c => c.Dates, dr =>
        {
            dr.Property(d => d.Start).HasColumnName("dates_start").IsRequired();
            dr.Property(d => d.End)  .HasColumnName("dates_end")  .IsRequired();
        });

        b.HasMany<CompetitionDiscipline>("_disciplines")
            .WithOne()
            .HasForeignKey(d => d.CompetitionId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Metadata.FindNavigation("_disciplines")!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
```

- [ ] **Step 2: Implement `CompetitionDisciplineConfiguration.cs`**

Path: `apps/api/src/Sport.Infrastructure/Configurations/Competitions/CompetitionDisciplineConfiguration.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;

namespace Sport.Infrastructure.Configurations.Competitions;

internal sealed class CompetitionDisciplineConfiguration : IEntityTypeConfiguration<CompetitionDiscipline>
{
    public void Configure(EntityTypeBuilder<CompetitionDiscipline> b)
    {
        b.ToTable("competition_disciplines");

        b.HasKey(d => d.Id);
        b.Property(d => d.Id)
            .HasConversion<CompetitionDisciplineIdEfCoreValueConverter>();

        b.Property(d => d.CompetitionId)
            .HasConversion<CompetitionIdEfCoreValueConverter>()
            .IsRequired();

        b.Property(d => d.Code)
            .HasConversion<DisciplineCodeEfCoreValueConverter>()
            .HasMaxLength(3)
            .IsRequired();

        b.Property(d => d.EnabledGenders)
            .HasConversion(
                v => v.Select(g => g.ToString()).ToArray(),
                v => v.Select(s => Enum.Parse<GenderCode>(s)).ToHashSet())
            .HasColumnType("text[]")
            .HasColumnName("enabled_genders")
            .IsRequired();

        b.HasIndex(d => new { d.CompetitionId, d.Code }).IsUnique();
    }
}
```

- [ ] **Step 3: Write the skipped roundtrip test**

Path: `apps/api/tests/Sport.Infrastructure.Tests/Persistence/CompetitionPersistenceTests.cs`
```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Infrastructure.Tests.Fixtures;
using Sport.Infrastructure.Tests.TestHelpers;

namespace Sport.Infrastructure.Tests.Persistence;

[Collection("Postgres")]
public sealed class CompetitionPersistenceTests : IClassFixture<SportDbContextFixture>
{
    private static readonly DisciplineCode Fbl = DisciplineCode.From("FBL");
    private readonly SportDbContextFixture _fixture;

    public CompetitionPersistenceTests(SportDbContextFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync().GetAwaiter().GetResult();
    }

    private static FakeRegistry MakeRegistry()
    {
        var reg = new FakeRegistry();
        reg.SupportedCodes.Add(Fbl);
        reg.GendersByCode[Fbl] = new HashSet<GenderCode> { GenderCode.M, GenderCode.W };
        return reg;
    }

    [Fact(Skip = "Run after InitialCreate migration is generated in Task 14")]
    public async Task Round_trips_a_competition_with_disciplines_and_genders_array()
    {
        var comp = Competition.Create(
            CompetitionId.New(),
            CompetitionCode.From("copa-2026"),
            "Copa 2026",
            DateRange.Create(new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30)),
            new[] { (Fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.M }) },
            MakeRegistry());

        await using (var write = _fixture.CreateContext())
        {
            write.Add(comp);
            await write.SaveChangesAsync();
        }

        await using var read = _fixture.CreateContext();
        var loaded = await read.Competitions
            .Include(c => c.Disciplines)
            .SingleAsync(c => c.Id == comp.Id);

        loaded.Code.Should().Be(comp.Code);
        loaded.Name.Should().Be("Copa 2026");
        loaded.Dates.Start.Should().Be(new DateOnly(2026, 6, 1));
        loaded.Dates.End.Should().Be(new DateOnly(2026, 6, 30));
        loaded.Disciplines.Should().HaveCount(1);
        loaded.Disciplines.Single().Code.Should().Be(Fbl);
        loaded.Disciplines.Single().EnabledGenders.Should().BeEquivalentTo(new[] { GenderCode.M });
    }

    [Fact(Skip = "Run after InitialCreate migration is generated in Task 14")]
    public async Task Code_unique_constraint_is_enforced_at_db_level()
    {
        var registry = MakeRegistry();
        var first = MakeCompetition("copa-2026", registry);
        var second = MakeCompetition("copa-2026", registry);

        await using (var ctx = _fixture.CreateContext())
        {
            ctx.Add(first);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = _fixture.CreateContext())
        {
            ctx.Add(second);
            var act = () => ctx.SaveChangesAsync();
            await act.Should().ThrowAsync<DbUpdateException>();
        }
    }

    private static Competition MakeCompetition(string code, FakeRegistry registry) =>
        Competition.Create(
            CompetitionId.New(),
            CompetitionCode.From(code),
            "Copa",
            DateRange.Create(new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30)),
            new[] { (Fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.M }) },
            registry);
}
```

- [ ] **Step 4: Build**

```
dotnet build apps/api/Sport.slnx
```
Expected: 0 warnings, 0 errors.

- [ ] **Step 5: Commit**

```
git add apps/api/src/Sport.Infrastructure/Configurations/Competitions/ apps/api/tests/Sport.Infrastructure.Tests/Persistence/CompetitionPersistenceTests.cs
git commit -m "feat(infra): map Competition + CompetitionDiscipline aggregate"
```

---

### Task 11: Structure configurations + skipped roundtrip tests

**Files:**
- Create: `apps/api/src/Sport.Infrastructure/Configurations/Structure/EventConfiguration.cs`
- Create: `apps/api/src/Sport.Infrastructure/Configurations/Structure/PhaseConfiguration.cs`
- Create: `apps/api/src/Sport.Infrastructure/Configurations/Structure/UnitConfiguration.cs`
- Create: `apps/api/src/Sport.Infrastructure/Configurations/Structure/SubunitConfiguration.cs`
- Create: `apps/api/tests/Sport.Infrastructure.Tests/Persistence/EventPersistenceTests.cs`
- Create: `apps/api/tests/Sport.Infrastructure.Tests/Persistence/PhaseUnitSubunitPersistenceTests.cs`

- [ ] **Step 1: Implement `EventConfiguration.cs`**

Path: `apps/api/src/Sport.Infrastructure/Configurations/Structure/EventConfiguration.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Structure;

namespace Sport.Infrastructure.Configurations.Structure;

internal sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> b)
    {
        b.ToTable("events");

        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion<EventIdEfCoreValueConverter>();

        b.Property(e => e.CompetitionDisciplineId)
            .HasConversion<CompetitionDisciplineIdEfCoreValueConverter>()
            .IsRequired();

        b.Property(e => e.DisciplineCode)
            .HasConversion<DisciplineCodeEfCoreValueConverter>()
            .HasMaxLength(3).IsRequired();

        b.Property(e => e.Gender).HasConversion<string>().HasMaxLength(1).IsRequired();

        b.Property(e => e.EventType)
            .HasConversion<EventTypeCodeEfCoreValueConverter>()
            .HasMaxLength(8).IsRequired();

        b.Property(e => e.EventModifier)
            .HasConversion<EventModifierCodeEfCoreValueConverter>()
            .HasMaxLength(10);

        b.Property(e => e.Name).HasMaxLength(200).IsRequired();

        b.Property(e => e.Rsc)
            .HasConversion<RscEfCoreValueConverter>()
            .HasMaxLength(34).IsRequired();
        b.HasIndex(e => new { e.CompetitionDisciplineId, e.Rsc }).IsUnique();

        b.HasMany<Phase>("_phases")
            .WithOne()
            .HasForeignKey(p => p.EventId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Metadata.FindNavigation("_phases")!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
```

- [ ] **Step 2: Implement `PhaseConfiguration.cs`**

Path: `apps/api/src/Sport.Infrastructure/Configurations/Structure/PhaseConfiguration.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.Structure;

namespace Sport.Infrastructure.Configurations.Structure;

internal sealed class PhaseConfiguration : IEntityTypeConfiguration<Phase>
{
    public void Configure(EntityTypeBuilder<Phase> b)
    {
        b.ToTable("phases");

        b.HasKey(p => p.Id);
        b.Property(p => p.Id).HasConversion<PhaseIdEfCoreValueConverter>();

        b.Property(p => p.EventId).HasConversion<EventIdEfCoreValueConverter>().IsRequired();

        b.Property(p => p.Code)
            .HasConversion<PhaseCodeEfCoreValueConverter>()
            .HasMaxLength(4).IsRequired();

        b.Property(p => p.Order).IsRequired();

        b.Property(p => p.Rsc)
            .HasConversion<RscEfCoreValueConverter>()
            .HasMaxLength(34).IsRequired();

        b.HasIndex(p => new { p.EventId, p.Code }).IsUnique();
        b.HasIndex(p => new { p.EventId, p.Order }).IsUnique();

        b.HasMany<Unit>("_units")
            .WithOne()
            .HasForeignKey(u => u.PhaseId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Metadata.FindNavigation("_units")!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
```

- [ ] **Step 3: Implement `UnitConfiguration.cs`**

Path: `apps/api/src/Sport.Infrastructure/Configurations/Structure/UnitConfiguration.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.Structure;

namespace Sport.Infrastructure.Configurations.Structure;

internal sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> b)
    {
        b.ToTable("units");

        b.HasKey(u => u.Id);
        b.Property(u => u.Id).HasConversion<UnitIdEfCoreValueConverter>();

        b.Property(u => u.PhaseId).HasConversion<PhaseIdEfCoreValueConverter>().IsRequired();

        b.Property(u => u.Code)
            .HasConversion<UnitCodeEfCoreValueConverter>()
            .HasMaxLength(8).IsRequired();

        b.Property(u => u.ScheduledStart);

        b.Property(u => u.Rsc)
            .HasConversion<RscEfCoreValueConverter>()
            .HasMaxLength(34).IsRequired();

        b.Property(u => u.DisciplineUnitRef);

        b.HasIndex(u => new { u.PhaseId, u.Code }).IsUnique();

        b.HasMany<Subunit>("_subunits")
            .WithOne()
            .HasForeignKey(s => s.UnitId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Metadata.FindNavigation("_subunits")!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
```

- [ ] **Step 4: Implement `SubunitConfiguration.cs`**

Path: `apps/api/src/Sport.Infrastructure/Configurations/Structure/SubunitConfiguration.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.Structure;

namespace Sport.Infrastructure.Configurations.Structure;

internal sealed class SubunitConfiguration : IEntityTypeConfiguration<Subunit>
{
    public void Configure(EntityTypeBuilder<Subunit> b)
    {
        b.ToTable("subunits");

        b.HasKey(s => s.Id);
        b.Property(s => s.Id).HasConversion<SubunitIdEfCoreValueConverter>();

        b.Property(s => s.UnitId).HasConversion<UnitIdEfCoreValueConverter>().IsRequired();

        b.Property(s => s.Code)
            .HasConversion<SubunitCodeEfCoreValueConverter>()
            .HasMaxLength(2).IsRequired();

        b.Property(s => s.Rsc)
            .HasConversion<RscEfCoreValueConverter>()
            .HasMaxLength(34).IsRequired();

        b.HasIndex(s => new { s.UnitId, s.Code }).IsUnique();
    }
}
```

- [ ] **Step 5: Write skipped tests for Event roundtrip and the Phase→Unit→Subunit hierarchy**

Path: `apps/api/tests/Sport.Infrastructure.Tests/Persistence/EventPersistenceTests.cs`
```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Structure;
using Sport.Infrastructure.Tests.Fixtures;
using Sport.Infrastructure.Tests.TestHelpers;

namespace Sport.Infrastructure.Tests.Persistence;

[Collection("Postgres")]
public sealed class EventPersistenceTests : IClassFixture<SportDbContextFixture>
{
    private static readonly DisciplineCode Fbl = DisciplineCode.From("FBL");
    private static readonly EventTypeCode Team11 = EventTypeCode.From("TEAM11");
    private readonly SportDbContextFixture _fixture;

    public EventPersistenceTests(SportDbContextFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync().GetAwaiter().GetResult();
    }

    [Fact(Skip = "Run after InitialCreate migration is generated in Task 14")]
    public async Task Round_trips_an_event_with_its_rsc()
    {
        var ev = Event.Create(
            EventId.New(), CompetitionDisciplineId.New(), Fbl, GenderCode.M,
            Team11, modifier: null, name: "Men's Football",
            disciplineModule: BuildModule());

        await using (var write = _fixture.CreateContext())
        {
            write.Add(ev);
            await write.SaveChangesAsync();
        }

        await using var read = _fixture.CreateContext();
        var loaded = await read.Events.SingleAsync(e => e.Id == ev.Id);
        loaded.Rsc.Value.Should().Be("FBLMTEAM11------------------------");
    }

    private static IDisciplineModule BuildModule()
    {
        var reg = new FakeRegistry();
        reg.SupportedCodes.Add(Fbl);
        reg.GendersByCode[Fbl] = new HashSet<GenderCode> { GenderCode.M };
        return reg.Get(Fbl);
    }
}
```

Path: `apps/api/tests/Sport.Infrastructure.Tests/Persistence/PhaseUnitSubunitPersistenceTests.cs`
```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Sport.Core.Structure;
using Sport.Infrastructure.Tests.Fixtures;

namespace Sport.Infrastructure.Tests.Persistence;

[Collection("Postgres")]
public sealed class PhaseUnitSubunitPersistenceTests : IClassFixture<SportDbContextFixture>
{
    private readonly SportDbContextFixture _fixture;

    public PhaseUnitSubunitPersistenceTests(SportDbContextFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync().GetAwaiter().GetResult();
    }

    [Fact(Skip = "Run after InitialCreate migration is generated in Task 14")]
    public async Task Round_trips_phase_unit_subunit_hierarchy()
    {
        var phaseRsc = Rsc.From("FBLMTEAM11------------QFNL--------");
        var phase = Phase.Create(PhaseId.New(), EventId.New(), PhaseCode.From("QFNL"), 1,
                                  Rsc.From("FBLMTEAM11------------------------"));
        var parentUnit = Unit.CreateParentForSubunits(
            UnitId.New(), phase.Id, UnitCode.From("00010000"), phase.Rsc, null);
        var sub = Subunit.Create(SubunitId.New(), parentUnit.Id, SubunitCode.From("01"), parentUnit.Rsc);
        parentUnit.AddSubunit(sub);
        phase.AddUnit(parentUnit);

        await using (var write = _fixture.CreateContext())
        {
            write.Add(phase);
            await write.SaveChangesAsync();
        }

        await using var read = _fixture.CreateContext();
        var loaded = await read.Phases
            .Include(p => p.Units).ThenInclude(u => u.Subunits)
            .AsSplitQuery()
            .SingleAsync(p => p.Id == phase.Id);

        loaded.Units.Should().HaveCount(1);
        loaded.Units.Single().Subunits.Should().HaveCount(1);
        loaded.Units.Single().Subunits.Single().Rsc.Value.Should().EndWith("01");
    }
}
```

Note `AsSplitQuery()` — required because we throw `MultipleCollectionIncludeWarning` for multi-Include of collections.

- [ ] **Step 6: Build**

```
dotnet build apps/api/Sport.slnx
```
Expected: 0 warnings, 0 errors.

- [ ] **Step 7: Commit**

```
git add apps/api/src/Sport.Infrastructure/Configurations/Structure/ apps/api/tests/Sport.Infrastructure.Tests/Persistence/EventPersistenceTests.cs apps/api/tests/Sport.Infrastructure.Tests/Persistence/PhaseUnitSubunitPersistenceTests.cs
git commit -m "feat(infra): map Event, Phase, Unit, Subunit aggregate hierarchy"
```

---

### Task 12: Participants configurations + skipped roundtrip tests

**Files:**
- Create: `apps/api/src/Sport.Infrastructure/Configurations/Participants/PersonConfiguration.cs`
- Create: `apps/api/src/Sport.Infrastructure/Configurations/Participants/OrganisationConfiguration.cs`
- Create: `apps/api/src/Sport.Infrastructure/Configurations/Participants/TeamConfiguration.cs`
- Create: `apps/api/src/Sport.Infrastructure/Configurations/Participants/EntryConfiguration.cs`
- Create: `apps/api/src/Sport.Infrastructure/Configurations/Participants/CompositionMemberConfiguration.cs`
- Create: `apps/api/tests/Sport.Infrastructure.Tests/Persistence/PersonOrganisationTeamPersistenceTests.cs`
- Create: `apps/api/tests/Sport.Infrastructure.Tests/Persistence/EntryPersistenceTests.cs`

- [ ] **Step 1: Implement `PersonConfiguration.cs`**

Path: `apps/api/src/Sport.Infrastructure/Configurations/Participants/PersonConfiguration.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;

namespace Sport.Infrastructure.Configurations.Participants;

internal sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> b)
    {
        b.ToTable("persons");

        b.HasKey(p => p.Id);
        b.Property(p => p.Id).HasConversion<PersonIdEfCoreValueConverter>();

        b.Property(p => p.FamilyName).HasMaxLength(50).IsRequired();
        b.Property(p => p.GivenName).HasMaxLength(50);
        b.Property(p => p.Gender).HasConversion<string>().HasMaxLength(1).IsRequired();
        b.Property(p => p.BirthDate);
        b.Property(p => p.IFId).HasMaxLength(20);

        b.HasIndex(p => new { p.FamilyName, p.GivenName });
    }
}
```

- [ ] **Step 2: Implement `OrganisationConfiguration.cs`**

Path: `apps/api/src/Sport.Infrastructure/Configurations/Participants/OrganisationConfiguration.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.Participants;

namespace Sport.Infrastructure.Configurations.Participants;

internal sealed class OrganisationConfiguration : IEntityTypeConfiguration<Organisation>
{
    public void Configure(EntityTypeBuilder<Organisation> b)
    {
        b.ToTable("organisations");

        b.HasKey(o => o.Id);
        b.Property(o => o.Id).HasConversion<OrganisationIdEfCoreValueConverter>();

        b.Property(o => o.Code)
            .HasConversion<OrganisationCodeEfCoreValueConverter>()
            .HasMaxLength(10).IsRequired();
        b.HasIndex(o => o.Code).IsUnique();

        b.Property(o => o.Name).HasMaxLength(200).IsRequired();
        b.Property(o => o.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
    }
}
```

- [ ] **Step 3: Implement `TeamConfiguration.cs`**

Path: `apps/api/src/Sport.Infrastructure/Configurations/Participants/TeamConfiguration.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;

namespace Sport.Infrastructure.Configurations.Participants;

internal sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> b)
    {
        b.ToTable("teams");

        b.HasKey(t => t.Id);
        b.Property(t => t.Id).HasConversion<TeamIdEfCoreValueConverter>();

        b.Property(t => t.Code)
            .HasConversion<TeamCodeEfCoreValueConverter>()
            .HasMaxLength(20).IsRequired();
        b.HasIndex(t => t.Code).IsUnique();

        b.Property(t => t.Name).HasMaxLength(200).IsRequired();

        b.Property(t => t.OrganisationId)
            .HasConversion<OrganisationIdEfCoreValueConverter>().IsRequired();

        b.Property(t => t.DisciplineCode)
            .HasConversion<DisciplineCodeEfCoreValueConverter>()
            .HasMaxLength(3).IsRequired();
    }
}
```

- [ ] **Step 4: Implement `EntryConfiguration.cs`**

Path: `apps/api/src/Sport.Infrastructure/Configurations/Participants/EntryConfiguration.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.Participants;
using Sport.Core.Structure;

namespace Sport.Infrastructure.Configurations.Participants;

internal sealed class EntryConfiguration : IEntityTypeConfiguration<Entry>
{
    public void Configure(EntityTypeBuilder<Entry> b)
    {
        b.ToTable("entries");

        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion<EntryIdEfCoreValueConverter>();

        b.Property(e => e.EventId).HasConversion<EventIdEfCoreValueConverter>().IsRequired();
        b.Property(e => e.Type).HasConversion<string>().HasMaxLength(10).IsRequired();
        b.Property(e => e.OrganisationId)
            .HasConversion<OrganisationIdEfCoreValueConverter>().IsRequired();
        b.Property(e => e.TeamId).HasConversion<TeamIdEfCoreValueConverter>();
        b.Property(e => e.Bib).HasConversion<BibEfCoreValueConverter>().HasMaxLength(20);
        b.Property(e => e.Seed);
        b.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        b.HasIndex(e => new { e.EventId, e.Status });

        b.HasMany<CompositionMember>("_composition")
            .WithOne()
            .HasForeignKey(m => m.EntryId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Metadata.FindNavigation("_composition")!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
```

- [ ] **Step 5: Implement `CompositionMemberConfiguration.cs`**

Path: `apps/api/src/Sport.Infrastructure/Configurations/Participants/CompositionMemberConfiguration.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.Participants;

namespace Sport.Infrastructure.Configurations.Participants;

internal sealed class CompositionMemberConfiguration : IEntityTypeConfiguration<CompositionMember>
{
    public void Configure(EntityTypeBuilder<CompositionMember> b)
    {
        b.ToTable("composition_members");

        // Composite key avoids needing an artificial id; CompositionMember is a record without Id.
        b.HasKey(m => new { m.EntryId, m.PersonId });

        b.Property(m => m.EntryId).HasConversion<EntryIdEfCoreValueConverter>();
        b.Property(m => m.PersonId).HasConversion<PersonIdEfCoreValueConverter>();
        b.Property(m => m.Order).IsRequired();
        b.Property(m => m.Bib).HasConversion<BibEfCoreValueConverter>().HasMaxLength(20);

        b.HasIndex(m => new { m.PersonId, m.EntryId }).IsUnique();
    }
}
```

- [ ] **Step 6: Write the skipped tests**

Path: `apps/api/tests/Sport.Infrastructure.Tests/Persistence/PersonOrganisationTeamPersistenceTests.cs`
```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Infrastructure.Tests.Fixtures;

namespace Sport.Infrastructure.Tests.Persistence;

[Collection("Postgres")]
public sealed class PersonOrganisationTeamPersistenceTests : IClassFixture<SportDbContextFixture>
{
    private readonly SportDbContextFixture _fixture;

    public PersonOrganisationTeamPersistenceTests(SportDbContextFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync().GetAwaiter().GetResult();
    }

    [Fact(Skip = "Run after InitialCreate migration is generated in Task 14")]
    public async Task Round_trips_person_organisation_team()
    {
        var org = Organisation.Create(OrganisationId.New(), OrganisationCode.From("ESP"), "Spain", OrganisationType.Noc);
        var person = Person.Create(PersonId.New(), "Pérez", "Juan", GenderCode.M, new DateOnly(1990, 1, 1), null);
        var team = Team.Create(TeamId.New(), TeamCode.From("ESP-FBL-M"), "Spain", org.Id, DisciplineCode.From("FBL"));

        await using (var write = _fixture.CreateContext())
        {
            write.Add(org);
            write.Add(person);
            write.Add(team);
            await write.SaveChangesAsync();
        }

        await using var read = _fixture.CreateContext();
        (await read.Organisations.SingleAsync(o => o.Id == org.Id)).Code.Value.Should().Be("ESP");
        (await read.Persons.SingleAsync(p => p.Id == person.Id)).FamilyName.Should().Be("Pérez");
        (await read.Teams.SingleAsync(t => t.Id == team.Id)).Code.Value.Should().Be("ESP-FBL-M");
    }
}
```

Path: `apps/api/tests/Sport.Infrastructure.Tests/Persistence/EntryPersistenceTests.cs`
```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Sport.Core.Participants;
using Sport.Core.Structure;
using Sport.Infrastructure.Tests.Fixtures;

namespace Sport.Infrastructure.Tests.Persistence;

[Collection("Postgres")]
public sealed class EntryPersistenceTests : IClassFixture<SportDbContextFixture>
{
    private readonly SportDbContextFixture _fixture;

    public EntryPersistenceTests(SportDbContextFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync().GetAwaiter().GetResult();
    }

    [Fact(Skip = "Run after InitialCreate migration is generated in Task 14")]
    public async Task Round_trips_entry_with_composition()
    {
        var members = Enumerable.Range(1, 3)
            .Select(i => (PersonId.New(), i, (Bib?)null))
            .ToArray();
        var entry = Entry.Create(
            EntryId.New(), EventId.New(),
            EntryType.Team, OrganisationId.New(),
            TeamId.New(), bib: null, seed: null,
            members);

        await using (var write = _fixture.CreateContext())
        {
            write.Add(entry);
            await write.SaveChangesAsync();
        }

        await using var read = _fixture.CreateContext();
        var loaded = await read.Entries
            .Include(e => e.Composition)
            .SingleAsync(e => e.Id == entry.Id);

        loaded.Status.Should().Be(EntryStatus.Registered);
        loaded.Composition.Should().HaveCount(3);
    }

    [Fact(Skip = "Run after InitialCreate migration is generated in Task 14")]
    public async Task Status_transitions_persist()
    {
        var entry = Entry.Create(
            EntryId.New(), EventId.New(), EntryType.Athlete, OrganisationId.New(),
            teamId: null, bib: null, seed: null,
            new[] { (PersonId.New(), 1, (Bib?)null) });
        entry.Disqualify();

        await using (var write = _fixture.CreateContext())
        {
            write.Add(entry);
            await write.SaveChangesAsync();
        }

        await using var read = _fixture.CreateContext();
        var loaded = await read.Entries.SingleAsync(e => e.Id == entry.Id);
        loaded.Status.Should().Be(EntryStatus.Disqualified);
    }
}
```

- [ ] **Step 7: Build**

```
dotnet build apps/api/Sport.slnx
```
Expected: 0 warnings, 0 errors.

- [ ] **Step 8: Commit**

```
git add apps/api/src/Sport.Infrastructure/Configurations/Participants/ apps/api/tests/Sport.Infrastructure.Tests/Persistence/PersonOrganisationTeamPersistenceTests.cs apps/api/tests/Sport.Infrastructure.Tests/Persistence/EntryPersistenceTests.cs
git commit -m "feat(infra): map Participants aggregates (Person, Organisation, Team, Entry, CompositionMember)"
```

---

### Task 10: Officials configuration + skipped roundtrip test

**Files:**
- Create: `apps/api/src/Sport.Infrastructure/Configurations/Officials/OfficialAssignmentConfiguration.cs`
- Create: `apps/api/tests/Sport.Infrastructure.Tests/Persistence/OfficialAssignmentPersistenceTests.cs`

- [ ] **Step 1: Implement `OfficialAssignmentConfiguration.cs`**

Path: `apps/api/src/Sport.Infrastructure/Configurations/Officials/OfficialAssignmentConfiguration.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.Officials;
using Sport.Core.Participants;

namespace Sport.Infrastructure.Configurations.Officials;

internal sealed class OfficialAssignmentConfiguration : IEntityTypeConfiguration<OfficialAssignment>
{
    public void Configure(EntityTypeBuilder<OfficialAssignment> b)
    {
        b.ToTable("official_assignments");

        b.HasKey(a => a.Id);
        b.Property(a => a.Id).HasConversion<OfficialAssignmentIdEfCoreValueConverter>();

        b.Property(a => a.PersonId).HasConversion<PersonIdEfCoreValueConverter>().IsRequired();

        b.Property(a => a.FunctionCode)
            .HasConversion<FunctionCodeEfCoreValueConverter>()
            .HasMaxLength(20).IsRequired();

        b.OwnsOne(a => a.Scope, sb =>
        {
            sb.Property(s => s.Level).HasConversion<string>().HasMaxLength(30).IsRequired().HasColumnName("scope_level");
            sb.Property(s => s.TargetId).IsRequired().HasColumnName("scope_target_id");
        });

        b.Property(a => a.OrganisationId).HasConversion<OrganisationIdEfCoreValueConverter>();

        b.Property(a => a.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        // Index spans owned-type columns via shadow access; declare via raw column names.
        b.HasIndex("scope_level", "scope_target_id", "function_code");
    }
}
```

- [ ] **Step 2: Write skipped test**

Path: `apps/api/tests/Sport.Infrastructure.Tests/Persistence/OfficialAssignmentPersistenceTests.cs`
```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Participants;
using Sport.Infrastructure.Tests.Fixtures;

namespace Sport.Infrastructure.Tests.Persistence;

[Collection("Postgres")]
public sealed class OfficialAssignmentPersistenceTests : IClassFixture<SportDbContextFixture>
{
    private readonly SportDbContextFixture _fixture;

    public OfficialAssignmentPersistenceTests(SportDbContextFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync().GetAwaiter().GetResult();
    }

    [Fact(Skip = "Run after InitialCreate migration is generated in Task 14")]
    public async Task Round_trips_official_assignment_with_owned_scope()
    {
        var descriptor = new FunctionDescriptor(
            FunctionCode.From("FBL.REF"), "Referee",
            new HashSet<ScopeLevel> { ScopeLevel.Unit },
            IsTeamOfficial: false, RequiresOrganisation: false);

        var assignment = OfficialAssignment.Create(
            OfficialAssignmentId.New(), PersonId.New(), descriptor,
            OfficialScope.Create(ScopeLevel.Unit, Guid.NewGuid()),
            organisationId: null);

        await using (var write = _fixture.CreateContext())
        {
            write.Add(assignment);
            await write.SaveChangesAsync();
        }

        await using var read = _fixture.CreateContext();
        var loaded = await read.OfficialAssignments.SingleAsync(a => a.Id == assignment.Id);
        loaded.FunctionCode.Value.Should().Be("FBL.REF");
        loaded.Scope.Level.Should().Be(ScopeLevel.Unit);
        loaded.Status.Should().Be(OfficialAssignmentStatus.Active);
    }
}
```

- [ ] **Step 3: Build**

```
dotnet build apps/api/Sport.slnx
```
Expected: 0 warnings, 0 errors.

- [ ] **Step 4: Commit**

```
git add apps/api/src/Sport.Infrastructure/Configurations/Officials/ apps/api/tests/Sport.Infrastructure.Tests/Persistence/OfficialAssignmentPersistenceTests.cs
git commit -m "feat(infra): map OfficialAssignment with owned OfficialScope"
```

---

## Phase G — Generate migration and unskip tests

### Task 11: Generate `InitialCreate` migration and unskip all persistence tests

**Files:**
- Create: `apps/api/src/Sport.Infrastructure/Migrations/*_InitialCreate.cs` (autogenerated)
- Create: `apps/api/src/Sport.Infrastructure/Migrations/*_InitialCreate.Designer.cs` (autogenerated)
- Create: `apps/api/src/Sport.Infrastructure/Migrations/SportDbContextModelSnapshot.cs` (autogenerated)
- Modify: all skipped tests from Tasks 13-16 (remove `Skip` argument from `[Fact]`)

- [ ] **Step 1: Start a local Postgres so `dotnet ef migrations add` can verify the model**

```
cp .env.example .env  # if not already present; password default is OK
docker compose up -d postgres
sleep 10
```

- [ ] **Step 2: Generate the migration**

```
dotnet ef migrations add InitialCreate \
    --project apps/api/src/Sport.Infrastructure \
    --startup-project apps/api/src/Sport.Api \
    --output-dir Migrations
```

Expected: three files appear under `apps/api/src/Sport.Infrastructure/Migrations/`:
- `YYYYMMDDHHmmss_InitialCreate.cs`
- `YYYYMMDDHHmmss_InitialCreate.Designer.cs`
- `SportDbContextModelSnapshot.cs`

- [ ] **Step 3: Verify the generated SQL has all expected tables**

Inspect the `Up` method of the migration. Confirm `CreateTable` calls for: `competitions`, `competition_disciplines`, `events`, `phases`, `units`, `subunits`, `persons`, `organisations`, `teams`, `entries`, `composition_members`, `official_assignments` — 12 tables.

- [ ] **Step 4: Unskip all `[Fact(Skip = "Run after InitialCreate migration is generated in Task 14")]` attributes**

For each persistence test file listed in Tasks 10–13, replace every `[Fact(Skip = "Run after InitialCreate migration is generated in Task 14")]` with `[Fact]`. Use a global search-and-replace across `apps/api/tests/Sport.Infrastructure.Tests/Persistence/`.

- [ ] **Step 5: Run all infrastructure tests**

```
dotnet test apps/api/tests/Sport.Infrastructure.Tests/Sport.Infrastructure.Tests.csproj
```
Expected: all tests (~10 persistence + 2 interceptor = ~12) pass. Testcontainers starts a Postgres, the fixture migrates, each test resets via Respawn, asserts roundtrip.

If a test fails because of a mapping issue (e.g. a column name mismatch), fix the configuration, regenerate the migration:
```
dotnet ef migrations remove --project apps/api/src/Sport.Infrastructure --startup-project apps/api/src/Sport.Api
dotnet ef migrations add InitialCreate --project apps/api/src/Sport.Infrastructure --startup-project apps/api/src/Sport.Api --output-dir Migrations
```
And rerun. Iterate until green.

- [ ] **Step 6: Commit the migration and unskipped tests together**

```
git add apps/api/src/Sport.Infrastructure/Migrations/ apps/api/tests/Sport.Infrastructure.Tests/Persistence/
git commit -m "feat(infra): generate InitialCreate migration and unskip persistence tests"
```

---

## Phase H — Schema verification and FK indexes

### Task 12: `ForeignKeyIndexTests`

**Files:**
- Create: `apps/api/tests/Sport.Infrastructure.Tests/Schema/ForeignKeyIndexTests.cs`

- [ ] **Step 1: Write the test**

Path: `apps/api/tests/Sport.Infrastructure.Tests/Schema/ForeignKeyIndexTests.cs`
```csharp
using FluentAssertions;
using Sport.Infrastructure.Tests.Fixtures;

namespace Sport.Infrastructure.Tests.Schema;

[Collection("Postgres")]
public sealed class ForeignKeyIndexTests : IClassFixture<SportDbContextFixture>
{
    private readonly SportDbContextFixture _fixture;

    public ForeignKeyIndexTests(SportDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Every_foreign_key_column_has_a_supporting_index()
    {
        await using var ctx = _fixture.CreateContext();
        var conn = ctx.Database.GetDbConnection();
        await conn.OpenAsync();

        const string sql = """
            SELECT conrelid::regclass::text AS table_name, a.attname AS column_name
              FROM pg_constraint c
              JOIN pg_attribute a ON a.attrelid = c.conrelid AND a.attnum = ANY(c.conkey)
             WHERE c.contype = 'f'
               AND NOT EXISTS (
                   SELECT 1 FROM pg_index i
                    WHERE i.indrelid = c.conrelid
                      AND a.attnum = ANY(i.indkey)
               );
            """;

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        await using var reader = await cmd.ExecuteReaderAsync();

        var missing = new List<string>();
        while (await reader.ReadAsync())
            missing.Add($"{reader.GetString(0)}.{reader.GetString(1)}");

        missing.Should().BeEmpty("every FK column should have a supporting index");
    }
}
```

- [ ] **Step 2: Run the test**

```
dotnet test apps/api/tests/Sport.Infrastructure.Tests/Sport.Infrastructure.Tests.csproj --filter "FullyQualifiedName~ForeignKeyIndexTests"
```
Expected: 1 passed.

- [ ] **Step 3: Commit**

```
git add apps/api/tests/Sport.Infrastructure.Tests/Schema/
git commit -m "test(infra): assert every foreign key column has a supporting index"
```

---

## Phase I — Wire into Sport.Api, /health/ready, NetArchTest

### Task 10: Wire `AddSportInfrastructure` and migration runner into `Sport.Api`

**Files:**
- Modify: `apps/api/src/Sport.Api/Sport.Api.csproj`
- Modify: `apps/api/src/Sport.Api/Program.cs`

- [ ] **Step 1: Add ProjectReference to Sport.Infrastructure**

```
dotnet add apps/api/src/Sport.Api/Sport.Api.csproj reference apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj
```

- [ ] **Step 2: Update `Program.cs` to call `AddSportInfrastructure` and the migration runner, and replace `/health/ready`**

Replace the entire content of `apps/api/src/Sport.Api/Program.cs` with:
```csharp
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Sport.Core.DisciplineRegistry;
using Sport.Disciplines.ATH;
using Sport.Disciplines.BDM;
using Sport.Disciplines.BKB;
using Sport.Disciplines.BOX;
using Sport.Disciplines.FBL;
using Sport.Disciplines.VBV;
using Sport.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSportCore()
    .AddDisciplineModule<FblModule>()
    .AddDisciplineModule<BkbModule>()
    .AddDisciplineModule<BdmModule>()
    .AddDisciplineModule<VbvModule>()
    .AddDisciplineModule<BoxModule>()
    .AddDisciplineModule<AthModule>();

builder.Services.AddSportInfrastructure();
builder.Services.AddOpenApi();

var app = builder.Build();

app.Services.BuildSportRegistry();

using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<SportMigrationRunner>();
    await runner.ApplyAsync();
}

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/",        () => Results.Ok(new { name = "Sport.Api" }));
app.MapGet("/health",  () => Results.Ok(new { status = "alive" }));
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
});

app.Run();

public partial class Program { }
```

- [ ] **Step 3: Update the existing `Ready_returns_200_with_status_ready` test in Sport.Api.Tests to assert on the status code only (it depends on DB now)**

Modify `apps/api/tests/Sport.Api.Tests/HostSmokeTests.cs` — replace the `Ready_returns_200_with_status_ready` method body with:
```csharp
    [Fact]
    public async Task Ready_endpoint_responds_with_health_check_status()
    {
        var response = await _client.GetAsync("/health/ready");

        // Without a DB available, expect 503 (Unhealthy). The body is implementation-defined
        // by the health check pipeline ("Healthy" or "Unhealthy" in text/plain).
        response.StatusCode.Should().BeOneOf(System.Net.HttpStatusCode.OK, System.Net.HttpStatusCode.ServiceUnavailable);
    }
```

- [ ] **Step 4: Build the solution**

```
dotnet build apps/api/Sport.slnx
```
Expected: 0 warnings, 0 errors.

- [ ] **Step 5: Run the Sport.Api.Tests**

```
dotnet test apps/api/tests/Sport.Api.Tests/Sport.Api.Tests.csproj
```
Expected: 5 passed. The renamed test asserts on a relaxed status code so it works without a live Postgres.

- [ ] **Step 6: Commit**

```
git add apps/api/src/Sport.Api/Sport.Api.csproj apps/api/src/Sport.Api/Program.cs apps/api/tests/Sport.Api.Tests/HostSmokeTests.cs
git commit -m "feat(api): wire Sport.Infrastructure and replace /health/ready with health-check pipeline"
```

---

### Task 11: `ReadyEndpointTests` in Sport.Infrastructure.Tests

**Files:**
- Create: `apps/api/tests/Sport.Infrastructure.Tests/HealthChecks/ReadyEndpointTests.cs`

- [ ] **Step 1: Write the tests**

Path: `apps/api/tests/Sport.Infrastructure.Tests/HealthChecks/ReadyEndpointTests.cs`
```csharp
using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Sport.Infrastructure.Tests.Fixtures;

namespace Sport.Infrastructure.Tests.HealthChecks;

[Collection("Postgres")]
public sealed class ReadyEndpointTests : IClassFixture<SportDbContextFixture>
{
    private readonly SportDbContextFixture _fixture;
    private readonly PostgresFixture _pg;

    public ReadyEndpointTests(SportDbContextFixture fixture, PostgresFixture pg)
    {
        _fixture = fixture;
        _pg = pg;
    }

    [Fact]
    public async Task Returns_200_when_postgres_is_reachable()
    {
        await using var factory = MakeFactory(_pg.ConnectionString);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/ready");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Returns_503_when_postgres_is_unreachable()
    {
        const string badConnString = "Host=127.0.0.1;Port=1;Database=missing;Username=x;Password=x;Timeout=2";
        await using var factory = MakeFactory(badConnString);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/ready");
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    private static WebApplicationFactory<Program> MakeFactory(string postgresConnectionString)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureAppConfiguration((_, cfg) =>
            {
                cfg.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Postgres"] = postgresConnectionString,
                });
            });
        });
    }
}
```

- [ ] **Step 2: Run the tests**

```
dotnet test apps/api/tests/Sport.Infrastructure.Tests/Sport.Infrastructure.Tests.csproj --filter "FullyQualifiedName~ReadyEndpointTests"
```
Expected: 2 passed. The 503 test might take ~2 s due to the connection timeout.

- [ ] **Step 3: Commit**

```
git add apps/api/tests/Sport.Infrastructure.Tests/HealthChecks/
git commit -m "test(infra): assert /health/ready returns 200 when DB up and 503 when DB down"
```

---

### Task 12: Add NetArchTest rules for the new layering

**Files:**
- Modify: `apps/api/tests/Sport.Architecture.Tests/Sport.Architecture.Tests.csproj`
- Modify: `apps/api/tests/Sport.Architecture.Tests/ArchitectureRules.cs`

- [ ] **Step 1: Add a project reference to Sport.Infrastructure so the arch tests can analyze it**

```
dotnet add apps/api/tests/Sport.Architecture.Tests/Sport.Architecture.Tests.csproj reference apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj
```

- [ ] **Step 2: Append the 5 new rules to `ArchitectureRules.cs`**

Modify `apps/api/tests/Sport.Architecture.Tests/ArchitectureRules.cs` — append the following methods inside the existing `ArchitectureRules` class:

```csharp
    private static readonly System.Reflection.Assembly InfrastructureAssembly =
        typeof(Sport.Infrastructure.SportDbContext).Assembly;

    [Fact]
    public void Sport_Core_does_not_reference_Sport_Infrastructure()
    {
        var result = Types.InAssembly(CoreAssembly)
            .Should().NotHaveDependencyOn("Sport.Infrastructure")
            .GetResult();
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Discipline_modules_do_not_reference_Sport_Infrastructure()
    {
        foreach (var asm in DisciplineAssemblies)
        {
            var result = Types.InAssembly(asm)
                .Should().NotHaveDependencyOn("Sport.Infrastructure")
                .GetResult();
            result.IsSuccessful.Should().BeTrue($"{asm.GetName().Name} must not depend on Sport.Infrastructure");
        }
    }

    [Fact]
    public void Sport_Infrastructure_references_Sport_Core()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .That().ResideInNamespace("Sport.Infrastructure")
            .Should().HaveDependencyOn("Sport.Core.Competitions")
            .GetResult();
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Sport_Core_does_not_reference_EntityFrameworkCore()
    {
        var result = Types.InAssembly(CoreAssembly)
            .Should().NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();
        result.IsSuccessful.Should().BeTrue("Sport.Core must remain domain-pure");
    }

    [Fact]
    public void All_EntityTypeConfigurations_are_internal_sealed()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .That().ImplementInterface(typeof(Microsoft.EntityFrameworkCore.IEntityTypeConfiguration<>))
            .Should().BeSealed()
            .And().NotBePublic()
            .GetResult();
        result.IsSuccessful.Should().BeTrue();
    }
```

- [ ] **Step 3: Run the architecture tests**

```
dotnet test apps/api/tests/Sport.Architecture.Tests/Sport.Architecture.Tests.csproj
```
Expected: 8 passed (3 existing + 5 new).

- [ ] **Step 4: Commit**

```
git add apps/api/tests/Sport.Architecture.Tests/
git commit -m "test(arch): enforce DDD layering with Sport.Infrastructure rules"
```

---

## Phase J — Compose changes and README

### Task 10: Enable `pg_stat_statements` in compose

**Files:**
- Create: `apps/api/docker/postgres/init/01-create-extension.sql`
- Modify: `docker-compose.yml`

- [ ] **Step 1: Create the init script**

Path: `apps/api/docker/postgres/init/01-create-extension.sql`
```sql
CREATE EXTENSION IF NOT EXISTS pg_stat_statements;
```

- [ ] **Step 2: Modify the `postgres` service in `docker-compose.yml` — add `command` and mount the init script**

Edit `docker-compose.yml` — replace the `postgres:` block so the full service definition reads:

```yaml
  postgres:
    image: postgres:18-alpine
    container_name: daline-postgres
    command:
      - "postgres"
      - "-c"
      - "shared_preload_libraries=pg_stat_statements"
      - "-c"
      - "pg_stat_statements.max=10000"
      - "-c"
      - "pg_stat_statements.track=all"
    environment:
      POSTGRES_USER:     ${POSTGRES_USER:-sport}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:?password is required}
      POSTGRES_DB:       ${POSTGRES_DB:-sport}
    ports:
      - "${POSTGRES_PORT:-5432}:5432"
    volumes:
      # Mount the parent dir (not /data) because postgres:18+ uses PGDATA=/var/lib/postgresql/<MAJOR>/docker.
      # Parent mount is upgrade-friendly across major versions.
      - postgres-data:/var/lib/postgresql
      - ./apps/api/docker/postgres/init:/docker-entrypoint-initdb.d:ro
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER:-sport} -d ${POSTGRES_DB:-sport}"]
      interval: 5s
      timeout: 3s
      retries: 10
      start_period: 5s
    restart: unless-stopped
```

- [ ] **Step 3: Recreate the postgres container with the new init script (volume must be empty for the script to run)**

```
docker compose down -v
docker compose up -d postgres
sleep 10
```

- [ ] **Step 4: Verify the extension is loaded**

```
docker compose exec postgres psql -U sport -d sport -c "SELECT extname FROM pg_extension WHERE extname = 'pg_stat_statements';"
```
Expected: 1 row returned.

- [ ] **Step 5: Commit**

```
git add docker-compose.yml apps/api/docker/postgres/init/
git commit -m "feat: enable pg_stat_statements in compose postgres service"
```

---

### Task 11: README "Auditing queries" section

**Files:**
- Modify: `README.md`

- [ ] **Step 1: Append the new section to the README**

Add the following at the bottom of `README.md`:

````markdown

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
````

- [ ] **Step 2: Commit**

```
git add README.md
git commit -m "docs: add auditing queries section to README"
```

---

## Phase K — Final verification

### Task 12: Run the full spec section 16 verification

Work from `C:\Users\mella\WebstormProjects\daline-sys`. Walk through every check.

- [ ] **Step 1: Full build**

```
dotnet build apps/api/Sport.slnx
```
Expected: 0 warnings, 0 errors.

- [ ] **Step 2: Full test run**

```
dotnet test "C:/Users/mella/WebstormProjects/daline-sys/apps/api/Sport.slnx"
```
Expected: ~155 tests pass (98 Core + 20 Disciplines + 8 Architecture + 5 Api + ~14 Infrastructure + 2 SlowQueryInterceptor). 0 failures.

- [ ] **Step 3: Clean rebuild of compose, init script applied**

```
docker compose down -v
docker compose up -d postgres
sleep 10
docker inspect daline-postgres --format '{{.State.Health.Status}}'
docker compose exec postgres psql -U sport -d sport -c "SELECT extname FROM pg_extension WHERE extname = 'pg_stat_statements';"
```
Expected: healthy; 1 row.

- [ ] **Step 4: Run the API from host and confirm migration applies**

```
dotnet run --project apps/api/src/Sport.Api/Sport.Api.csproj &
HOST_PID=$!
sleep 10
curl -fsS http://localhost:5080/health
echo "---"
curl -i http://localhost:5080/health/ready
kill $HOST_PID 2>/dev/null
wait $HOST_PID 2>/dev/null
```
Expected: `/health` returns `alive`; `/health/ready` returns 200. The host log shows `Applying 1 pending migrations: <timestamp>_InitialCreate`.

- [ ] **Step 5: Confirm tables in Postgres**

```
docker compose exec postgres psql -U sport -d sport -c "\dt"
```
Expected: 13 tables — 12 model + `__EFMigrationsHistory`.

- [ ] **Step 6: Dockerized full stack**

```
docker compose --profile api up -d --build
sleep 25
docker inspect daline-api --format '{{.State.Health.Status}}'
curl -fsS http://localhost:8080/health/ready
```
Expected: api healthy; `/health/ready` returns 200.

- [ ] **Step 7: Clean shutdown**

```
docker compose --profile api down -v
```

- [ ] **Step 8: Confirm git status is clean**

```
git status
```
Expected: `nothing to commit, working tree clean` (modulo unrelated untracked files).

If anything fails, stop and triage. If a step requires a code fix, make the fix, commit it with `fix(infra): ...`, and rerun.

---

## Self-review checklist

After completing all tasks:

1. **Spec coverage** — every decision in the spec (D1..D14) is implemented:
   - D1 (scope: only core) → Tasks 13-16 cover all 11 root aggregates.
   - D2 (layout: Sport.Infrastructure) → Task 3.
   - D3 (Vogen converters auto) → Task 2.
   - D4 (migrations: auto-dev/manual-other) → Task 8 + 19.
   - D5 (Testcontainers + Respawn) → Tasks 4 + 9.
   - D6 (/health/ready) → Task 16 + 20.
   - D7 (snake_case) → Task 7 (`.UseSnakeCaseNamingConvention()`).
   - D8 (layout mirror) → Tasks 13-16 directory structure.
   - D9 (query control: NoTracking, warnings, slow query) → Tasks 5 + 7.
   - D10 (pg_stat_statements) → Task 19.
   - D11 (EnabledGenders as text[]) → Task 10.
   - D12 (owned types: DateRange, OfficialScope) → Tasks 13 + 16.
   - D13 (backing-field collections) → Tasks 13-15.
   - D14 (no EnsureCreated, always Migrate) → Task 8.

2. **No placeholders** — every step shows full code/commands. The `Skip = "..."` placeholders in Tasks 13-16 are intentional and removed in Task 14.

3. **Type consistency** — `SportDbContext.cs`, `AddSportInfrastructure`, `SportMigrationRunner` and the configurations all use the same property/type names. Each `[EfCoreConverter]`-generated class is referenced as `{Type}EfCoreValueConverter` throughout.

4. **All tests green** — `dotnet test apps/api/Sport.slnx` after Task 21 reports ~155 passed.

5. **Architecture preserved** — Sport.Core has no EF Core dependency; NetArchTest enforces this in Task 18.

If any item fails, fix inline before declaring the plan complete.

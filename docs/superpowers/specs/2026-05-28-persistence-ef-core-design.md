---
title: Persistence (EF Core + PostgreSQL) Design
date: 2026-05-28
status: draft
scope: Core entities persistence with EF Core 10 + Npgsql + Postgres 18
---

# Persistence Design

Spec para la capa de persistencia del core deportivo. Crea el proyecto `Sport.Infrastructure` (DDD/Onion), mapea los 11 agregados raíz del core a Postgres 18 vía EF Core 10 + Npgsql, integra migraciones, habilita `/health/ready` con check real de DB, y baja palancas de control sobre las queries autogeneradas para evitar bottlenecks silenciosos.

## 1. Contexto

El core deportivo está implementado y testeado (126 tests verde en main). La API está dockerizada con `postgres:18-alpine` corriendo en compose. La connection string ya viaja por env var (`ConnectionStrings__Postgres`) pero ningún componente la consume aún. No existe DbContext, ni migrations, ni mappings.

Esta spec produce:

- Un proyecto `Sport.Infrastructure` con `SportDbContext`, configuraciones por agregado, conversores Vogen, e interceptores.
- Migration inicial con todas las tablas del core.
- Auto-aplicación de migrations en `Development`; manual en otros entornos.
- `/health/ready` chequea Postgres real.
- `pg_stat_statements` cargado en el servicio `postgres` del compose.
- Tests de integración contra Postgres real (Testcontainers + Respawn).
- Palancas para mantener control de queries (slow query interceptor, warnings-as-errors, sin tracking por default).
- Audit playbook en el README.

## 2. Alcance

### En scope

- Proyecto `Sport.Infrastructure` con DbContext, configurations, migrations, interceptor.
- Mapping de los 11 agregados raíz: `Competition`, `CompetitionDiscipline`, `Event`, `Phase`, `Unit`, `Subunit`, `Person`, `Organisation`, `Team`, `Entry` (+ `CompositionMember`), `OfficialAssignment`.
- Conversores EF Core para los 23 value objects Vogen (11 IDs + 12 codes/string VOs).
- Owned types: `DateRange` (en Competition) y `OfficialScope` (en OfficialAssignment).
- Backing-field collection mappings: `Competition._disciplines`, `Event._phases`, `Phase._units`, `Unit._subunits`, `Entry._composition`.
- `EnabledGenders` como array Postgres `text[]`.
- Migration inicial `00000000000000_InitialCreate`.
- `SportMigrationRunner` con política por entorno.
- `SlowQueryInterceptor` con threshold configurable.
- `SqlCommandCounterInterceptor` (test helper).
- `AddDbContextCheck<SportDbContext>` en `/health/ready` con tag `ready`.
- Nuevo proyecto `Sport.Infrastructure.Tests` con Testcontainers + Respawn.
- Cambios al `docker-compose.yml` para cargar `pg_stat_statements` + script `init/01-create-extension.sql`.
- Cambios al `Sport.Api`: reference a `Sport.Infrastructure`, llamada a `AddSportInfrastructure`, invocación del migration runner, nuevo `/health/ready`.
- Nuevas reglas de NetArchTest en `Sport.Architecture.Tests`.
- Sección "Auditing queries" agregada al `README.md`.

### Fuera de scope (specs futuras)

- Endpoints de dominio (Competition CRUD, etc.).
- Persistencia para entidades específicas de disciplina (FootballMatch, BoxingBout, HighJumpTrial). Mecanismo para que los módulos contribuyan DbSets.
- Mapping de live ops, lineups, replacements, oficial availability.
- Audit log / outbox pattern.
- Multi-tenancy.
- Concurrency tokens y control de concurrencia optimista.
- OpenTelemetry / tracing distribuido. Endpoint HTTP para exponer pg_stat_statements.
- PgBouncer / connection pooling externo. Tuning de `shared_buffers`, `work_mem`, etc.
- Métricas Prometheus (postgres_exporter).
- Seed data.
- Migration scripts review en CI / migration rollbacks automáticos.
- Múltiples schemas.
- Logging estructurado con Serilog (separado).

## 3. Decisiones tomadas

| # | Decisión | Resumen |
|---|---|---|
| D1 | Alcance | Solo los 11 agregados raíz del core. |
| D2 | Layout | Proyecto nuevo `Sport.Infrastructure`. `Sport.Core` permanece dominio puro. |
| D3 | Vogen converters | Generación automática con marker class en `Sport.Infrastructure` (atributos `[EfCoreConverter<T>]`) — Sport.Core no referencia EF Core. |
| D4 | Migraciones | Auto-apply en `Development`; manual en otros entornos. |
| D5 | Tests | Testcontainers + Respawn; proyecto nuevo `Sport.Infrastructure.Tests`. |
| D6 | `/health/ready` | Reemplazado por health-check pipeline con `AddDbContextCheck<SportDbContext>`. |
| D7 | Naming SQL | snake_case vía `EFCore.NamingConventions`. |
| D8 | Layout interno | Mirroring de submódulos del core dentro de `Configurations/`. |
| D9 | Query control | `NoTrackingWithIdentityResolution` por default; warnings críticos elevados a `Throw`; `SlowQueryInterceptor`; `EnableSensitiveDataLogging` + `LogTo` en Development. |
| D10 | Postgres extensions | `pg_stat_statements` cargada en el servicio postgres del compose. |
| D11 | `EnabledGenders` | Postgres array `text[]` (no tabla join). |
| D12 | `OfficialScope`, `DateRange` | Owned types con columnas en la tabla padre. |
| D13 | Backing-field collections | `PropertyAccessMode.Field` para preservar encapsulamiento. |
| D14 | `EnsureCreated` | No usar; siempre `Migrate`/`MigrateAsync`. |

## 4. Proyecto `Sport.Infrastructure`

### 4.1 Layout

```
apps/api/src/Sport.Infrastructure/
├── Sport.Infrastructure.csproj
├── SportDbContext.cs
├── DependencyInjection.cs                  (AddSportInfrastructure extension)
├── SportMigrationRunner.cs
├── Interceptors/
│   └── SlowQueryInterceptor.cs
├── Configurations/
│   ├── Competitions/
│   │   ├── CompetitionConfiguration.cs
│   │   └── CompetitionDisciplineConfiguration.cs
│   ├── Structure/
│   │   ├── EventConfiguration.cs
│   │   ├── PhaseConfiguration.cs
│   │   ├── UnitConfiguration.cs
│   │   └── SubunitConfiguration.cs
│   ├── Participants/
│   │   ├── PersonConfiguration.cs
│   │   ├── OrganisationConfiguration.cs
│   │   ├── TeamConfiguration.cs
│   │   ├── EntryConfiguration.cs
│   │   └── CompositionMemberConfiguration.cs
│   └── Officials/
│       └── OfficialAssignmentConfiguration.cs
└── Migrations/
    └── 00000000000000_InitialCreate.cs
```

### 4.2 Packages

Declarados explícitamente en `Sport.Infrastructure.csproj`:

- `Microsoft.EntityFrameworkCore` (10.x)
- `Microsoft.EntityFrameworkCore.Relational` (10.x)
- `Microsoft.EntityFrameworkCore.Design` (10.x, `PrivateAssets="all"`)
- `Npgsql.EntityFrameworkCore.PostgreSQL` (última estable compatible con EF 10)
- `EFCore.NamingConventions`
- `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore`
- `Microsoft.Extensions.Hosting.Abstractions`
- `Vogen` (para que `[EfCoreConverter<T>]` y `HasVogenConversion()` extension resuelvan)

`Sport.Api.csproj` agrega una `ProjectReference` a `Sport.Infrastructure`.

### 4.3 `SportDbContext`

```csharp
public sealed class SportDbContext(DbContextOptions<SportDbContext> options)
    : DbContext(options)
{
    public DbSet<Competition>         Competitions         => Set<Competition>();
    public DbSet<Event>               Events               => Set<Event>();
    public DbSet<Phase>               Phases               => Set<Phase>();
    public DbSet<Unit>                Units                => Set<Unit>();
    public DbSet<Subunit>             Subunits             => Set<Subunit>();
    public DbSet<Person>              Persons              => Set<Person>();
    public DbSet<Organisation>        Organisations        => Set<Organisation>();
    public DbSet<Team>                Teams                => Set<Team>();
    public DbSet<Entry>               Entries              => Set<Entry>();
    public DbSet<OfficialAssignment>  OfficialAssignments  => Set<OfficialAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SportDbContext).Assembly);
    }
}
```

`CompetitionDiscipline` y `CompositionMember` se acceden vía la entidad padre, no tienen `DbSet` propio.

### 4.4 `AddSportInfrastructure`

```csharp
public static IServiceCollection AddSportInfrastructure(this IServiceCollection services)
{
    services.AddOptions<SlowQueryOptions>().BindConfiguration("SlowQuery");
    services.AddSingleton<SlowQueryInterceptor>();

    services.AddDbContext<SportDbContext>((sp, options) =>
    {
        var cfg = sp.GetRequiredService<IConfiguration>();
        var env = sp.GetRequiredService<IHostEnvironment>();

        options
            .UseNpgsql(cfg.GetConnectionString("Postgres"),
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
```

## 5. `SlowQueryInterceptor`

Path: `Sport.Infrastructure/Interceptors/SlowQueryInterceptor.cs`.

```csharp
public sealed class SlowQueryInterceptor(
    ILogger<SlowQueryInterceptor> logger,
    IOptions<SlowQueryOptions> options) : DbCommandInterceptor
{
    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData,
        DbDataReader result, CancellationToken ct = default)
    {
        LogIfSlow(command, eventData);
        return ValueTask.FromResult(result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData,
        int result, CancellationToken ct = default)
    {
        LogIfSlow(command, eventData);
        return ValueTask.FromResult(result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData,
        object? result, CancellationToken ct = default)
    {
        LogIfSlow(command, eventData);
        return ValueTask.FromResult(result);
    }

    private void LogIfSlow(DbCommand command, CommandExecutedEventData eventData)
    {
        var thresholdMs = options.Value.ThresholdMs;
        var durationMs  = eventData.Duration.TotalMilliseconds;
        if (durationMs < thresholdMs) return;

        logger.LogWarning(
            "Slow query: {DurationMs:F1} ms (threshold {ThresholdMs} ms). SQL: {CommandText}",
            durationMs, thresholdMs, command.CommandText);
    }
}

public sealed class SlowQueryOptions
{
    public int ThresholdMs { get; set; } = 200;
}
```

Configuración en `appsettings.json`:

```json
{
  "SlowQuery": { "ThresholdMs": 200 }
}
```

El binding ya queda registrado en `AddSportInfrastructure` (Sección 4.4 línea 1).

## 6. Configurations por agregado

### 6.1 Convenciones aplicadas a todos

- PK del agregado raíz con `HasConversion<...EfCoreValueConverter>()` (generado por Vogen).
- Todas las VOs string usan `HasConversion<...EfCoreValueConverter>()` + `HasMaxLength(N)` cuando aplica.
- Enums (`GenderCode`, `EntryStatus`, etc.) → `HasConversion<string>()` (legibles en SQL).
- Backing fields: `b.Metadata.FindNavigation("_xxx")!.SetPropertyAccessMode(PropertyAccessMode.Field)`.
- Cascade delete del agregado a sus entidades hijas con `OnDelete(DeleteBehavior.Cascade)`.

### 6.2 Mapping específico por agregado

| Agregado | Tabla principal | Columnas notables | Hijos / Owned | Índices |
|---|---|---|---|---|
| `Competition` | `competitions` | `code` (S(64)), `name` (S(200)) | Owned `Dates` → `dates_start`/`dates_end`. `_disciplines` → tabla `competition_disciplines` (FK `competition_id`) | `UNIQUE(code)` |
| `CompetitionDiscipline` | `competition_disciplines` | `code` (DisciplineCode S(3)) | `enabled_genders` `text[]` | `UNIQUE(competition_id, code)` |
| `Event` | `events` | `gender` (text), `event_type` (S(8)), `event_modifier` (S(10) nullable), `name` (S(200)), `rsc` (S(34)) | `_phases` → tabla `phases` | `UNIQUE(competition_discipline_id, rsc)` |
| `Phase` | `phases` | `phase_code` (S(4)), `order` (int), `rsc` | `_units` | `UNIQUE(event_id, phase_code)`, `UNIQUE(event_id, order)` |
| `Unit` | `units` | `unit_code` (S(8)), `scheduled_start` (timestamptz nullable), `rsc`, `discipline_unit_ref` (uuid nullable, sin FK) | `_subunits` | `UNIQUE(phase_id, unit_code)` |
| `Subunit` | `subunits` | `subunit_code` (S(2)), `rsc` | — | `UNIQUE(unit_id, subunit_code)` |
| `Person` | `persons` | `family_name` (S(50)), `given_name` (S(50) nullable), `gender` (text), `birth_date` (date nullable), `if_id` (S(20) nullable) | — | `(family_name, given_name)` |
| `Organisation` | `organisations` | `code` (S(10)), `name` (S(200)), `type` (text) | — | `UNIQUE(code)` |
| `Team` | `teams` | `code` (S(20)), `name` (S(200)), `discipline_code` (S(3)) | — | `UNIQUE(code)` |
| `Entry` | `entries` | `type` (text), `bib` (S(20) nullable), `seed` (int nullable), `status` (text) | `_composition` → tabla `composition_members` | `(event_id, status)` |
| `CompositionMember` | `composition_members` | `order` (int), `bib` (S(20) nullable) | — | `UNIQUE(person_id, entry_id)` |
| `OfficialAssignment` | `official_assignments` | `function_code` (S(20)), `status` (text) | Owned `Scope` → `scope_level` (text), `scope_target_id` (uuid) | `(scope_level, scope_target_id, function_code)` |

### 6.3 Ejemplo — `CompetitionConfiguration.cs`

```csharp
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

### 6.4 Marker class en `Sport.Infrastructure` — `[EfCoreConverter<T>]`

Vogen 8 soporta un patrón "marker class" para Onion/Clean Architecture: el atributo `[EfCoreConverter<T>]` se coloca en un archivo del proyecto consumidor (`Sport.Infrastructure`), NO en el VO. Vogen genera los converters dentro del marker class, en `Sport.Infrastructure`.

`Sport.Core` queda 100% dominio puro — sin referencia a `Microsoft.EntityFrameworkCore`.

Path: `apps/api/src/Sport.Infrastructure/VogenEfCoreConverters.cs`

```csharp
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Participants;
using Sport.Core.Structure;
using Vogen;

namespace Sport.Infrastructure;

[EfCoreConverter<CompetitionId>]
[EfCoreConverter<CompetitionDisciplineId>]
[EfCoreConverter<CompetitionCode>]
[EfCoreConverter<EventId>]
[EfCoreConverter<PhaseId>]
[EfCoreConverter<UnitId>]
[EfCoreConverter<SubunitId>]
[EfCoreConverter<Rsc>]
[EfCoreConverter<EventTypeCode>]
[EfCoreConverter<EventModifierCode>]
[EfCoreConverter<PhaseCode>]
[EfCoreConverter<UnitCode>]
[EfCoreConverter<SubunitCode>]
[EfCoreConverter<PersonId>]
[EfCoreConverter<OrganisationId>]
[EfCoreConverter<TeamId>]
[EfCoreConverter<EntryId>]
[EfCoreConverter<OrganisationCode>]
[EfCoreConverter<TeamCode>]
[EfCoreConverter<Bib>]
[EfCoreConverter<OfficialAssignmentId>]
[EfCoreConverter<FunctionCode>]
[EfCoreConverter<DisciplineCode>]
internal partial class VogenEfCoreConverters;
```

Vogen genera converters como `VogenEfCoreConverters.CompetitionIdEfCoreValueConverter` (nested). Las configurations los referencian o usan la extension `HasVogenConversion()` que Vogen también genera.

NetArchTest confirma que `Sport.Core` NO referencia `Microsoft.EntityFrameworkCore.*`.

## 7. Migraciones

### 7.1 Tooling

`dotnet-ef` se instala vía manifest:

```
dotnet new tool-manifest
dotnet tool install dotnet-ef
```

`.config/dotnet-tools.json` se commitea. Versión: latest stable compatible con EF 10.

### 7.2 Comandos de generación

```
dotnet ef migrations add <NombreMigración> \
    --project apps/api/src/Sport.Infrastructure \
    --startup-project apps/api/src/Sport.Api \
    --output-dir Migrations

dotnet ef database update \
    --project apps/api/src/Sport.Infrastructure \
    --startup-project apps/api/src/Sport.Api
```

### 7.3 `SportMigrationRunner`

```csharp
public sealed class SportMigrationRunner(
    SportDbContext db,
    IHostEnvironment env,
    ILogger<SportMigrationRunner> logger)
{
    public async Task ApplyAsync(CancellationToken ct = default)
    {
        if (!env.IsDevelopment())
        {
            logger.LogInformation(
                "Auto-migration skipped (env={Env}). Apply migrations manually via `dotnet ef database update`.",
                env.EnvironmentName);
            return;
        }

        var pending = (await db.Database.GetPendingMigrationsAsync(ct)).ToList();
        if (pending.Count == 0)
        {
            logger.LogInformation("No pending migrations.");
            return;
        }

        logger.LogInformation(
            "Applying {N} pending migrations: {Names}",
            pending.Count, string.Join(", ", pending));
        await db.Database.MigrateAsync(ct);
        logger.LogInformation("Migrations applied.");
    }
}
```

### 7.4 Invocación en `Program.cs` de `Sport.Api`

Después de `app.Services.BuildSportRegistry()`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<SportMigrationRunner>();
    await runner.ApplyAsync();
}
```

### 7.5 Migration inicial

Se genera con `dotnet ef migrations add InitialCreate` durante la implementación. Output esperado en `Sport.Infrastructure/Migrations/`:

- `YYYYMMDDHHMMSS_InitialCreate.cs` con todas las tablas, columnas, índices, FKs.
- `YYYYMMDDHHMMSS_InitialCreate.Designer.cs`.
- `SportDbContextModelSnapshot.cs`.

## 8. `/health/ready` y endpoint pipeline

Cambios en `Program.cs`:

```csharp
// Mantener liveness simple:
app.MapGet("/health", () => Results.Ok(new { status = "alive" }));

// Reemplazar el ready endpoint:
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
});
```

`AddDbContextCheck<SportDbContext>` ya quedó registrado en `AddSportInfrastructure` (Sección 4.4).

### 8.1 Semántica

| Endpoint | Significado | Status si Postgres no responde |
|---|---|---|
| `/health` | Liveness — proceso .NET responde HTTP | 200 (no toca DB) |
| `/health/ready` | Readiness — listo para tráfico real | 503 |

### 8.2 Output

Default de ASP.NET Health Checks: `text/plain` con `Healthy`/`Unhealthy`. Suficiente para el healthcheck de Docker y para diagnóstico manual. JSON estructurado se deja para una spec de observabilidad.

### 8.3 Tests

- En `Sport.Api.Tests`: el test `Ready_returns_200_with_status_ready` se actualiza para verificar que el endpoint existe y devuelve un status code 200 o 503 (no asume DB up). Si actualmente está acoplado a `"ready"` en el body, se ajusta para chequear status code.
- En `Sport.Infrastructure.Tests/HealthChecks/ReadyEndpointTests.cs`: dos tests con `WebApplicationFactory<Program>` overrideando connection string para target a Testcontainers (200) o a un host inválido (503).

## 9. Compose — `pg_stat_statements`

### 9.1 Cambios en `docker-compose.yml`

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
    # ... healthcheck y resto sin cambios
```

### 9.2 Init script

Path: `apps/api/docker/postgres/init/01-create-extension.sql`

```sql
CREATE EXTENSION IF NOT EXISTS pg_stat_statements;
```

Postgres ejecuta scripts en `/docker-entrypoint-initdb.d/` la primera vez que arranca con un volumen vacío. Si el volumen ya existía de antes de esta spec, el script NO corre — hay que `docker compose down -v` y rearmar para que se ejecute.

## 10. Tests — `Sport.Infrastructure.Tests`

### 10.1 Layout

```
apps/api/tests/Sport.Infrastructure.Tests/
├── Sport.Infrastructure.Tests.csproj
├── Fixtures/
│   ├── PostgresFixture.cs
│   └── SportDbContextFixture.cs
├── Persistence/
│   ├── CompetitionPersistenceTests.cs
│   ├── EventPersistenceTests.cs
│   ├── PhaseUnitSubunitPersistenceTests.cs
│   ├── PersonOrganisationTeamPersistenceTests.cs
│   ├── EntryPersistenceTests.cs
│   └── OfficialAssignmentPersistenceTests.cs
├── HealthChecks/
│   └── ReadyEndpointTests.cs
├── Schema/
│   └── ForeignKeyIndexTests.cs
└── TestHelpers/
    └── SqlCommandCounterInterceptor.cs
```

### 10.2 Packages

xUnit + FluentAssertions 7.2.0 + Mvc.Testing + `Testcontainers.PostgreSql` + `Respawn` + `Npgsql`.

### 10.3 `PostgresFixture` (collection-shared)

```csharp
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
    public async Task DisposeAsync()    => await _container.DisposeAsync();
}

[CollectionDefinition("Postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture> { }
```

### 10.4 `SportDbContextFixture` (per-class)

Migra una vez en `InitializeAsync`. Expone `CreateContext(params IInterceptor[])` y `ResetAsync()` (Respawn).

### 10.5 Estilo

Cada test class:
- `[Collection("Postgres")]` + `IClassFixture<SportDbContextFixture>`.
- En el constructor llama `fixture.ResetAsync()` para empezar limpio.
- Tests son `async Task`.
- Persist en un `SaveChangesAsync()`, leer en otro contexto, asertar.

### 10.6 Cobertura objetivo

| Test class | Cubre |
|---|---|
| `CompetitionPersistenceTests` | Round-trip de Competition + disciplines, unique constraint en code, EnabledGenders como array |
| `EventPersistenceTests` | Round-trip de Event, RSC persistido, unique RSC por CompetitionDiscipline |
| `PhaseUnitSubunitPersistenceTests` | Jerarquía Phase→Unit→Subunit, RSC composition, DisciplineUnitRef nullable |
| `PersonOrganisationTeamPersistenceTests` | VOs de strings, OrganisationCode unique, Team.DisciplineCode |
| `EntryPersistenceTests` | CompositionMember tabla hija, transiciones de Status, FK a Event |
| `OfficialAssignmentPersistenceTests` | OfficialScope como owned type, FunctionCode roundtrip |
| `ForeignKeyIndexTests` | Cada FK column tiene índice (query a pg_constraint vs pg_index) |
| `ReadyEndpointTests` | /health/ready 200 con DB up, 503 con DB down |

### 10.7 Test helper `SqlCommandCounterInterceptor`

```csharp
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
    // Equivalentes async + para NonQuery/Scalar.
}
```

Disponible para tests que quieran asertar máximo N de queries por escenario (detecta N+1).

## 11. NetArchTest — reglas nuevas

En `Sport.Architecture.Tests/ArchitectureRules.cs`:

1. `Sport.Core` NO referencia `Sport.Infrastructure`.
2. `Sport.Disciplines.*` NO referencia `Sport.Infrastructure`.
3. `Sport.Infrastructure` referencia `Sport.Core`.
4. `Sport.Core` NO referencia `Microsoft.EntityFrameworkCore` (mantiene dominio puro).
5. Todas las `IEntityTypeConfiguration<>` en `Sport.Infrastructure` son `internal sealed`.

## 12. README — sección "Auditing queries"

Se agrega al `README.md` raíz:

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

## 13. Cross-cutting

- **Connection string**: vive en `IConfiguration["ConnectionStrings:Postgres"]`. La consumen `SportDbContext` (via `AddSportInfrastructure`). Para tests, `Testcontainers` la provee dinámicamente.
- **Tool manifest**: `.config/dotnet-tools.json` commiteado con `dotnet-ef`.
- **Slow query threshold**: 200 ms default, configurable vía `SlowQuery:ThresholdMs` en `appsettings`.
- **Logging**: `ILogger<SlowQueryInterceptor>` y `ILogger<SportMigrationRunner>` con campos estructurados. Cuando llegue Serilog, los campos quedan compatibles.

## 14. Riesgos abiertos

1. **PK Vogen → uuid Postgres**: verificar en tests que la conversion Guid v7 ↔ uuid mantiene el orden temporal. Si Vogen emite `Guid → Guid` directo, no hay problema.
2. **`Throw` en warnings durante queries de dominio**: cuando lleguen endpoints, las queries multi-Include forzarán `AsSplitQuery()` o `AsSingleQuery()` explícito. Aceptado.
3. **`PropertyAccessMode.Field`**: exige patrón `private List<T> _items; public IReadOnlyList<T> Items => _items;`. Ya se cumple; cualquier nueva entidad debe mantenerlo.
4. **Vogen `[EfCoreConverter]` en versión 8.0.5**: verificar al implementar; downgrade a converters manuales si la versión no lo soporta.
5. **Migration drift entre devs**: los timestamps de migrations evitan colisión hard pero pueden requerir regenerar en la branch que mergea segunda.
6. **`pg_stat_statements` en Testcontainers**: la imagen `postgres:18-alpine` sin custom command no carga la extensión. No es problema porque los tests no dependen de ella; si en el futuro testeamos algo que la requiera, configuramos `WithCommand("postgres", "-c", "shared_preload_libraries=pg_stat_statements")` en el builder.
7. **Respawn vs migrations**: Respawn limpia datos, no schema. Una migration mid-suite causaría drift. Mitigación: `MigrateAsync` una sola vez al boot del fixture, Respawn entre tests.
8. **Volumen postgres existente sin extension**: si el dev ya tenía datos persistidos del compose anterior, el init script no corre (Postgres solo ejecuta `/docker-entrypoint-initdb.d/` cuando el volumen está vacío). Workaround: `docker compose down -v` y re-up para que se aplique. Documentar en el plan.

## 15. Dependencias hacia adelante

| Spec siguiente | Depende en |
|---|---|
| Endpoints de dominio (CRUD por agregado) | `SportDbContext` + mappings. |
| Persistencia para disciplinas (FootballMatch, etc.) | Patrón de mapping, mecanismo para que módulos contribuyan DbSets. |
| Live ops / play-by-play | Mapping de Entry, Unit, lineup. |
| Outbox / domain events | Interceptor pattern. |
| Multi-tenancy | Aditivo (TenantId en agregados raíz + filtro global). |
| Serilog + structured logging | Reemplaza `LogTo(Console.WriteLine, ...)` y consume los campos estructurados de los interceptors. |
| Observabilidad operacional (`/internal/db-stats`) | Endpoint que consulta pg_stat_statements + auth. |

## 16. Verificación post-implementación

Forma la última tarea del plan:

1. `dotnet build apps/api/Sport.slnx` → 0 warnings, 0 errors.
2. `dotnet test apps/api/Sport.slnx` → todos los tests verde (Sport.Core.Tests: 98 + Sport.Disciplines.*: 20 + Sport.Architecture.Tests extendido + Sport.Api.Tests: 5 + Sport.Infrastructure.Tests: ~25).
3. `docker compose down -v && docker compose up -d postgres` → healthy en < 10 s; init script aplicó la extensión.
4. `docker compose exec postgres psql -U sport -d sport -c "SELECT extname FROM pg_extension WHERE extname = 'pg_stat_statements';"` → 1 fila.
5. `dotnet run --project apps/api/src/Sport.Api/Sport.Api.csproj` → log muestra "Applying N pending migrations" la primera vez, "No pending migrations" la segunda.
6. `curl http://localhost:5080/health` → 200 con `alive`.
7. `curl -i http://localhost:5080/health/ready` → 200 con `Healthy` mientras postgres up; bajás postgres y devuelve 503.
8. `docker compose exec postgres psql -U sport -d sport -c "\dt"` → ~13 tablas (12 del modelo + `__EFMigrationsHistory`).
9. `docker compose --profile api up --build` → ambos healthy, API en 8080 contesta `/health/ready` con 200.
10. NetArchTest pasa con las 5 reglas nuevas.
11. Inspección visual del log de la API durante `dotnet test` muestra los SQL emitidos (en Development) y eventual "Slow query" si algún test es lento.

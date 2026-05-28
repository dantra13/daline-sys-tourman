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

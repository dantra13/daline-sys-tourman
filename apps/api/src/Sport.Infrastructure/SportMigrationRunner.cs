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

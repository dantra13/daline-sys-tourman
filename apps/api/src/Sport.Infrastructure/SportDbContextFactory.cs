using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sport.Infrastructure;

/// <summary>
/// Design-time factory used by <c>dotnet ef</c> to create a <see cref="SportDbContext"/>
/// without requiring a running application host.  The connection string here is only used
/// during migration scaffolding; at runtime the context is configured via
/// <see cref="DependencyInjection.AddSportInfrastructure"/>.
/// </summary>
internal sealed class SportDbContextFactory : IDesignTimeDbContextFactory<SportDbContext>
{
    public SportDbContext CreateDbContext(string[] args)
    {
        var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
                   ?? "Host=localhost;Port=5432;Database=sport;Username=sport;Password=change-me-locally";

        var options = new DbContextOptionsBuilder<SportDbContext>()
            .UseNpgsql(connStr, npgsql =>
                npgsql.MigrationsAssembly(typeof(SportDbContext).Assembly.FullName))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new SportDbContext(options);
    }
}

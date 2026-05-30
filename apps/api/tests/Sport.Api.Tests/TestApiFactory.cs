using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Sport.Api.Tests.Fakes;
using Sport.Application.Abstractions;

namespace Sport.Api.Tests;

/// <summary>
/// WebApplicationFactory that boots the API host in the "Testing" environment, swapping the
/// EF-backed repositories with in-memory fakes so endpoint tests don't need Postgres.
/// A placeholder connection string is provided so the SportDbContext factory's startup guard
/// passes; nothing actually connects because the repositories are replaced.
/// </summary>
public class TestApiFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Hosting environment the API boots under. Defaults to "Testing". Override to exercise
    /// environment-specific framework behaviour (e.g. minimal-API binder ThrowOnBadRequest,
    /// which the framework defaults to true only in "Development").
    /// </summary>
    protected virtual string EnvironmentName => "Testing";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(EnvironmentName);

        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = "Host=test-not-real;Database=test;Username=test;Password=test",
                // Repositories are replaced with in-memory fakes; never touch a real database.
                ["RunMigrationsOnStartup"] = "false",
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ICompetitionRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.AddSingleton<InMemoryStore>();
            services.AddScoped<ICompetitionRepository, InMemoryCompetitionRepository>();
            services.AddScoped<IUnitOfWork, NoopUnitOfWork>();
        });
    }
}

/// <summary>
/// Boots the same in-memory host but in the "Development" environment, where the minimal-API
/// binder sets ThrowOnBadRequest = true by default. Used to guard that malformed request bodies
/// still produce the unified 400 envelope instead of leaking a 500 from the exception middleware.
/// </summary>
public sealed class DevelopmentApiFactory : TestApiFactory
{
    public CapturingLoggerProvider Logs { get; } = new();

    protected override string EnvironmentName => "Development";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureLogging(logging => logging.AddProvider(Logs));
    }
}

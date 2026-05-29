using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sport.Api.Tests.Fakes;
using Sport.Application.Abstractions;

namespace Sport.Api.Tests;

/// <summary>
/// WebApplicationFactory that boots the API host in the "Testing" environment, swapping the
/// EF-backed repositories with in-memory fakes so endpoint tests don't need Postgres.
/// A placeholder connection string is provided so the SportDbContext factory's startup guard
/// passes; nothing actually connects because the repositories are replaced.
/// </summary>
public sealed class TestApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = "Host=test-not-real;Database=test;Username=test;Password=test",
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

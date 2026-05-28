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

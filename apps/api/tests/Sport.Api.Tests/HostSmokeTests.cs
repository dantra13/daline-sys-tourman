using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Sport.Core.DisciplineRegistry;

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

    [Fact]
    public async Task Ready_returns_200_with_status_ready()
    {
        var response = await _client.GetAsync("/health/ready");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("ready");
    }

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
}

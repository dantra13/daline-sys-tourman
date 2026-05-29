using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Sport.Core.DisciplineRegistry;

namespace Sport.Api.Tests;

public class HostSmokeTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;
    private readonly TestApiFactory _factory;

    public HostSmokeTests(TestApiFactory factory)
    {
        _factory = factory;
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
    public async Task Ready_endpoint_responds_with_health_check_status()
    {
        var response = await _client.GetAsync("/health/ready");

        // Without a DB available, expect 503 (Unhealthy). The body is implementation-defined
        // by the health check pipeline ("Healthy" or "Unhealthy" in text/plain).
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public void Registry_has_all_seven_disciplines_registered()
    {
        using var factory  = new TestApiFactory();
        using var scope    = factory.Services.CreateScope();
        var registry       = scope.ServiceProvider.GetRequiredService<IDisciplineRegistry>();

        registry.RegisteredCodes.Should().HaveCount(7);
        var codes = registry.RegisteredCodes.Select(c => c.Value).ToHashSet();
        codes.Should().BeEquivalentTo(new[] { "FBL", "BKB", "BDM", "VBV", "BOX", "ATH", "JUD" });
    }

    [Fact]
    public async Task OpenApi_document_is_served()
    {
        var response = await _client.GetAsync("/openapi/v1.json");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("openapi");
    }

    [Fact]
    public async Task Scalar_ui_is_reachable()
    {
        var response = await _client.GetAsync("/scalar/v1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var contentType = response.Content.Headers.ContentType?.MediaType;
        contentType.Should().Be("text/html");
    }
}

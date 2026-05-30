using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Sport.Api.Tests;

/// <summary>
/// Regression coverage for the 500 hole that only surfaces in the "Development" environment,
/// where the minimal-API binder throws BadHttpRequestException (ThrowOnBadRequest = true) instead
/// of short-circuiting with a 400. Body parse failures must still resolve to the unified
/// 400 request.malformed envelope, never internal.unexpected/500.
/// </summary>
public class MalformedBodyInDevelopmentTests : IClassFixture<DevelopmentApiFactory>
{
    private readonly DevelopmentApiFactory _factory;
    private readonly HttpClient _client;

    public MalformedBodyInDevelopmentTests(DevelopmentApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_with_empty_date_string_returns_400_with_code_request_malformed()
    {
        // Reproduces the reported case: well-formed JSON whose dates.start cannot bind to DateOnly.
        var payload = new
        {
            code = "",
            name = "",
            dates = new { start = "", end = "" },
            disciplines = new[] { new { code = "", genders = new[] { "" } } },
        };

        var resp = await _client.PostAsJsonAsync("/competitions", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        resp.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        var body = await resp.Content.ReadFromJsonAsync<ProblemBody>();
        body!.Code.Should().Be("request.malformed");
    }

    [Fact]
    public async Task POST_with_malformed_json_returns_400_with_code_request_malformed()
    {
        var resp = await _client.PostAsync(
            "/competitions",
            new StringContent("{this is not json", Encoding.UTF8, "application/json"));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadFromJsonAsync<ProblemBody>();
        body!.Code.Should().Be("request.malformed");
    }

    [Fact]
    public async Task Malformed_body_emits_a_single_structured_warning_log()
    {
        // The factory is shared across the class, so assert on the delta: exactly one record is
        // emitted for THIS request (no double-logging), not one in the whole queue.
        var before = _factory.Logs.Entries.Count(e => e.Category == "Sport.Api.MalformedRequest");

        await _client.PostAsync(
            "/competitions",
            new StringContent("{this is not json", Encoding.UTF8, "application/json"));

        var entries = _factory.Logs.Entries
            .Where(e => e.Category == "Sport.Api.MalformedRequest")
            .ToList();

        entries.Count.Should().Be(before + 1);
        var entry = entries[^1];
        entry.Level.Should().Be(LogLevel.Warning);
        entry.Message.Should().Contain("request.malformed");
        entry.Message.Should().Contain("/competitions");
    }

    private sealed record ProblemBody(
        string Type,
        string Title,
        int Status,
        string? Detail,
        string Code,
        string? TraceId);
}

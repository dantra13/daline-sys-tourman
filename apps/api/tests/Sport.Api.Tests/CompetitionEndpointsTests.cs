using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;

namespace Sport.Api.Tests;

public class CompetitionEndpointsTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;

    public CompetitionEndpointsTests(TestApiFactory factory) => _client = factory.CreateClient();

    private static object ValidPayload(string code = "jud-2026") => new
    {
        code,
        name = "Judo Open 2026",
        dates = new { start = "2026-08-01", end = "2026-08-05" },
        disciplines = new[] { new { code = "FBL", genders = new[] { "M" } } },
    };

    [Fact]
    public async Task POST_creates_and_returns_201_with_location()
    {
        var resp = await _client.PostAsJsonAsync("/competitions", ValidPayload("e2e-create"));

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        resp.Headers.Location!.ToString().Should().StartWith("/competitions/");

        var body = await resp.Content.ReadFromJsonAsync<CompetitionDtoBody>();
        body!.Code.Should().Be("e2e-create");
        body.Disciplines.Should().HaveCount(1);
    }

    [Fact]
    public async Task POST_with_blank_name_returns_422_with_code_name_required()
    {
        var payload = new
        {
            code = "e2e-blank-name",
            name = "   ",
            dates = new { start = "2026-08-01", end = "2026-08-05" },
            disciplines = new[] { new { code = "FBL", genders = new[] { "M" } } },
        };

        var resp = await _client.PostAsJsonAsync("/competitions", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        resp.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        var body = await resp.Content.ReadFromJsonAsync<ProblemBody>();
        body!.Code.Should().Be("competition.name_required");
        body.Errors.Should().ContainSingle(e => e.Code == "competition.name_required");
    }

    [Fact]
    public async Task POST_with_duplicate_disciplines_returns_422_with_code_duplicate_discipline()
    {
        var payload = new
        {
            code = "e2e-dup-disc",
            name = "Dup",
            dates = new { start = "2026-08-01", end = "2026-08-05" },
            disciplines = new[]
            {
                new { code = "FBL", genders = new[] { "M" } },
                new { code = "FBL", genders = new[] { "M" } },
            },
        };

        var resp = await _client.PostAsJsonAsync("/competitions", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var body = await resp.Content.ReadFromJsonAsync<ProblemBody>();
        body!.Code.Should().Be("competition.duplicate_discipline");
    }

    [Fact]
    public async Task POST_with_unregistered_discipline_returns_422_with_code_discipline_not_registered()
    {
        var payload = new
        {
            code = "e2e-unreg",
            name = "Unreg",
            dates = new { start = "2026-08-01", end = "2026-08-05" },
            disciplines = new[] { new { code = "ZZZ", genders = new[] { "M" } } },
        };

        var resp = await _client.PostAsJsonAsync("/competitions", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var body = await resp.Content.ReadFromJsonAsync<ProblemBody>();
        body!.Code.Should().Be("competition.discipline_not_registered");
    }

    [Fact]
    public async Task POST_with_duplicate_code_returns_409_with_code_code_already_exists()
    {
        (await _client.PostAsJsonAsync("/competitions", ValidPayload("e2e-conflict"))).EnsureSuccessStatusCode();

        var second = await _client.PostAsJsonAsync("/competitions", ValidPayload("e2e-conflict"));

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await second.Content.ReadFromJsonAsync<ProblemBody>();
        body!.Code.Should().Be("competition.code_already_exists");
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
    public async Task GET_returns_200_with_items_envelope()
    {
        var resp = await _client.GetAsync("/competitions");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<ItemsEnvelope>();
        body!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task GET_by_id_returns_200_when_found()
    {
        var create = await _client.PostAsJsonAsync("/competitions", ValidPayload("e2e-get-found"));
        var dto = await create.Content.ReadFromJsonAsync<CompetitionDtoBody>();

        var resp = await _client.GetAsync($"/competitions/{dto!.Id}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var loaded = await resp.Content.ReadFromJsonAsync<CompetitionDtoBody>();
        loaded!.Id.Should().Be(dto.Id);
    }

    [Fact]
    public async Task GET_by_id_returns_404_with_code_not_found_when_missing()
    {
        var resp = await _client.GetAsync($"/competitions/{Guid.NewGuid()}");

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await resp.Content.ReadFromJsonAsync<ProblemBody>();
        body!.Code.Should().Be("competition.not_found");
    }

    [Fact]
    public async Task Error_envelope_includes_traceId()
    {
        var resp = await _client.GetAsync($"/competitions/{Guid.NewGuid()}");
        var body = await resp.Content.ReadFromJsonAsync<ProblemBody>();

        body!.TraceId.Should().NotBeNullOrWhiteSpace();
    }

    private sealed record CompetitionDtoBody(
        Guid Id,
        string Code,
        string Name,
        DateRangeBody Dates,
        IReadOnlyList<DisciplineBody> Disciplines);

    private sealed record DateRangeBody(DateOnly Start, DateOnly End);
    private sealed record DisciplineBody(Guid Id, string Code, IReadOnlyList<string> Genders);

    private sealed record ItemsEnvelope(IReadOnlyList<CompetitionDtoBody> Items);

    private sealed record ProblemBody(
        string Type,
        string Title,
        int Status,
        string? Detail,
        string Code,
        IReadOnlyList<ProblemErrorEntry> Errors,
        string? TraceId);

    private sealed record ProblemErrorEntry(string Code, string? Target);
}

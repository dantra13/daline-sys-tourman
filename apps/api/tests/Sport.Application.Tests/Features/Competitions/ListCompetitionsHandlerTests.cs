using FluentAssertions;
using Sport.Application.Features.Competitions;
using Sport.Application.Features.Competitions.ListCompetitions;
using Sport.Application.Tests.Fakes;

namespace Sport.Application.Tests.Features.Competitions;

public class ListCompetitionsHandlerTests
{
    [Fact]
    public async Task Returns_empty_list_when_no_competitions()
    {
        var repo = new InMemoryCompetitionRepository();

        var result = await ListCompetitionsHandler.Handle(
            new ListCompetitions(), repo, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Returns_competitions_in_code_order()
    {
        var registry = TestRegistry.WithFblM();
        var repo = new InMemoryCompetitionRepository();
        await repo.AddAsync(CompetitionFactory.JudOpen(registry), CancellationToken.None);
        await repo.AddAsync(
            CompetitionFactory.Custom(registry, code: "abc-2026", name: "Earlier"),
            CancellationToken.None);

        var result = await ListCompetitionsHandler.Handle(
            new ListCompetitions(), repo, CancellationToken.None);

        result.Select(c => c.Code).Should().Equal("abc-2026", "jud-2026");
    }
}

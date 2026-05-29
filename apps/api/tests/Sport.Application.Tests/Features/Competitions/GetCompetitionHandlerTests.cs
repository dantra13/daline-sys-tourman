using FluentAssertions;
using Sport.Application.Common;
using Sport.Application.Features.Competitions;
using Sport.Application.Features.Competitions.GetCompetition;
using Sport.Application.Tests.Fakes;

namespace Sport.Application.Tests.Features.Competitions;

public class GetCompetitionHandlerTests
{
    [Fact]
    public async Task Returns_dto_when_competition_exists()
    {
        var repo = new InMemoryCompetitionRepository();
        var registry = TestRegistry.WithFblM();
        var existing = CompetitionFactory.JudOpen(registry);
        await repo.AddAsync(existing, CancellationToken.None);

        var dto = await GetCompetitionHandler.Handle(
            new GetCompetition(existing.Id.Value), repo, CancellationToken.None);

        dto.Id.Should().Be(existing.Id.Value);
        dto.Code.Should().Be("jud-2026");
        dto.Disciplines.Should().HaveCount(1);
    }

    [Fact]
    public async Task Throws_NotFoundException_with_code_when_missing()
    {
        var repo = new InMemoryCompetitionRepository();

        Func<Task<CompetitionDto>> act = () => GetCompetitionHandler.Handle(
            new GetCompetition(Guid.NewGuid()), repo, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.Which.Code.Should().Be("competition.not_found");
    }
}

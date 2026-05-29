using Sport.Application.Abstractions;
using Sport.Application.Features.Competitions;

namespace Sport.Application.Features.Competitions.ListCompetitions;

public static class ListCompetitionsHandler
{
    public static async Task<IReadOnlyList<CompetitionDto>> Handle(
        ListCompetitions _,
        ICompetitionRepository repo,
        CancellationToken ct)
    {
        var competitions = await repo.ListAsync(ct);
        return competitions.Select(CompetitionDto.From).ToList();
    }
}

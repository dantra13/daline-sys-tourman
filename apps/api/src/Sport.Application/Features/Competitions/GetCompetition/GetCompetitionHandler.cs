using Sport.Application.Abstractions;
using Sport.Application.Common;
using Sport.Core.Competitions;

namespace Sport.Application.Features.Competitions.GetCompetition;

public static class GetCompetitionHandler
{
    public static async Task<CompetitionDto> Handle(
        GetCompetition query,
        ICompetitionRepository repo,
        CancellationToken ct)
    {
        var id = CompetitionId.From(query.Id);
        var competition = await repo.GetByIdAsync(id, ct);
        if (competition is null)
        {
            throw new NotFoundException(
                code: "competition.not_found",
                @params: new Dictionary<string, object?> { ["id"] = query.Id });
        }

        return CompetitionDto.From(competition);
    }
}

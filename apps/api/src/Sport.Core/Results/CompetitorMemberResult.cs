using Sport.Core.Participants;

namespace Sport.Core.Results;

public sealed record CompetitorMemberResult(PersonId PersonId, int Order)
{
    public Bib? Bib { get; init; }
    public int? StartSortOrder { get; init; }
    public IReadOnlyList<ResultExtension> Extensions { get; init; } = Array.Empty<ResultExtension>();
}

using Sport.Core.Participants;

namespace Sport.Core.Results;

public sealed record CompetitorResult(EntryId EntryId, int SortOrder)
{
    public string? ResultValue { get; init; }
    public ResultTypeCode? ResultType { get; init; }
    public Wlt? Wlt { get; init; }
    public int? Rank { get; init; }
    public bool RankEqual { get; init; }
    public Irm? Irm { get; init; }
    public int? StartOrder { get; init; }
    public int? StartSortOrder { get; init; }
    public IReadOnlyList<CompetitorMemberResult> Composition { get; init; } = Array.Empty<CompetitorMemberResult>();
    public IReadOnlyList<ResultExtension> Extensions { get; init; } = Array.Empty<ResultExtension>();
}

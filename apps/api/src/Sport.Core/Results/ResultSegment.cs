namespace Sport.Core.Results;

public sealed record ResultSegment(SegmentCode Code, int Order)
{
    public IReadOnlyList<SegmentScore> Scores { get; init; } = Array.Empty<SegmentScore>();
    public IReadOnlyList<ResultExtension> Extensions { get; init; } = Array.Empty<ResultExtension>();
}

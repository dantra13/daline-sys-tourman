using Sport.Core.Participants;

namespace Sport.Core.Results;

public sealed record SegmentScore(EntryId EntryId)
{
    public string? CumulativeValue { get; init; }
    public string? SegmentValue { get; init; }
}

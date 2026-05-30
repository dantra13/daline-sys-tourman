using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;

namespace Sport.Disciplines.FBL;

public sealed record FootballPeriodScore(EntryId EntryId, string? Cumulative, string? Segment);

public sealed record FootballPeriod(string Code, IReadOnlyList<FootballPeriodScore> Scores);

public sealed record FootballMatchResult(
    IReadOnlyDictionary<EntryId, string> TeamScores,
    IReadOnlyList<FootballPeriod> Periods) : DisciplineResultProjection;

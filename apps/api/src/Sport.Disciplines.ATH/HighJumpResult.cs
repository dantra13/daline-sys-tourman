using Sport.Core.DisciplineRegistry;

namespace Sport.Disciplines.ATH;

public sealed record HighJumpAthleteResult(string? BestMark, int? Rank, IReadOnlyList<string> Attempts);

public sealed record HighJumpResult(
    IReadOnlyList<string> Heights,
    IReadOnlyList<HighJumpAthleteResult> Athletes) : DisciplineResultProjection;

using Sport.Core.DisciplineRegistry;

namespace Sport.Disciplines.BOX;

public sealed record BoxJudgeTotal(string Judge, string? Value);

public sealed record BoxingBoutResult(IReadOnlyList<BoxJudgeTotal> JudgeTotals, string? Decision) : DisciplineResultProjection;

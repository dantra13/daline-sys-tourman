using Sport.Core.Results;

namespace Sport.Core.DisciplineRegistry;

public sealed record ResultRollup(
    IReadOnlyList<CompetitorResult> Competitors,
    IReadOnlyList<ResultExtension> Extensions,
    ResultStatus? SuggestedStatus);

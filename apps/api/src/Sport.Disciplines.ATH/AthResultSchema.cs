using Sport.Core.DisciplineRegistry;
using Sport.Core.Results;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.ATH;

public sealed class AthResultSchema : DefaultResultSchema
{
    private static readonly ExtensionCode Intermediate = ExtensionCode.From("INTERMEDIATE");

    public override OutcomeMode OutcomeModeFor(EventTypeCode type) => OutcomeMode.Ranked;

    public override Result Validate(UnitResultDocument document)
    {
        foreach (var row in document.Competitors)
        {
            if (row.Wlt is not null)
                return Result.Fail("ATH is ranked: WLT must be absent.");
            if (row.SortOrder <= 0)
                return Result.Fail("ATH requires a positive SortOrder on every row.");
        }
        return Result.Ok();
    }

    public override DisciplineResultProjection Project(UnitResultDocument document)
    {
        var heights = document.Extensions
            .Where(e => e.Code == Intermediate)
            .OrderBy(e => e.Pos)
            .Select(e => e.Value ?? string.Empty)
            .ToArray();

        var athletes = document.Competitors
            .Select(c => new HighJumpAthleteResult(
                c.ResultValue,
                c.Rank,
                c.Extensions.Where(e => e.Code == Intermediate).OrderBy(e => e.Pos).Select(e => e.Value ?? string.Empty).ToArray()))
            .ToArray();

        return new HighJumpResult(heights, athletes);
    }
}

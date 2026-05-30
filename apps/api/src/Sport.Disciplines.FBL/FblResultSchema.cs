using Sport.Core.DisciplineRegistry;
using Sport.Core.Results;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.FBL;

public sealed class FblResultSchema : DefaultResultSchema
{
    public override OutcomeMode OutcomeModeFor(EventTypeCode type) => OutcomeMode.HeadToHead;

    public override Result Validate(UnitResultDocument document)
    {
        foreach (var row in document.Competitors)
        {
            if (row.Rank is not null)
                return Result.Fail("I-RES-5", "FBL does not use Rank; use WLT.");
            var scored = document.Status != ResultStatus.StartList && row.Irm is null;
            if (scored && row.Wlt is null)
                return Result.Fail("I-RES-5", "FBL is head-to-head: each scored competitor must carry a WLT.");
        }
        return Result.Ok();
    }

    public override DisciplineResultProjection Project(UnitResultDocument document)
    {
        var scores = document.Competitors
            .Where(c => c.ResultValue is not null)
            .ToDictionary(c => c.EntryId, c => c.ResultValue!);
        var periods = document.Segments
            .OrderBy(s => s.Order)
            .Select(s => new FootballPeriod(
                s.Code.Value,
                s.Scores.Select(sc => new FootballPeriodScore(sc.EntryId, sc.CumulativeValue, sc.SegmentValue)).ToArray()))
            .ToArray();
        return new FootballMatchResult(scores, periods);
    }
}

using Sport.Core.DisciplineRegistry;
using Sport.Core.Results;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.BOX;

public sealed class BoxResultSchema : DefaultResultSchema
{
    // BOX moves START_LIST -> LIVE -> OFFICIAL (no UNOFFICIAL stage), plus PROVISIONAL.
    private static readonly IReadOnlySet<ResultStatus> BoxStatuses = new HashSet<ResultStatus>
    {
        ResultStatus.StartList, ResultStatus.Live, ResultStatus.Official, ResultStatus.Provisional,
    };

    public override OutcomeMode OutcomeModeFor(EventTypeCode type) => OutcomeMode.HeadToHead;
    public override IReadOnlySet<ResultStatus> StatusesFor(EventTypeCode type) => BoxStatuses;

    public override Result Validate(UnitResultDocument document)
    {
        foreach (var row in document.Competitors)
        {
            var scored = document.Status != ResultStatus.StartList && row.Irm is null;
            if (scored && row.Wlt is null)
                return Result.Fail("I-RES-5", "BOX is head-to-head: each scored competitor must carry a WLT.");
        }
        return Result.Ok();
    }

    public override DisciplineResultProjection Project(UnitResultDocument document)
    {
        var totals = document.Competitors
            .SelectMany(c => c.Extensions)
            .Where(e => e.Code == ExtensionCode.From("JUDGE"))
            .Select(e => new BoxJudgeTotal(e.Pos ?? string.Empty, e.Value))
            .ToArray();
        var decision = document.Extensions.FirstOrDefault(e => e.Code == ExtensionCode.From("RES_CODE"))?.Value;
        return new BoxingBoutResult(totals, decision);
    }
}

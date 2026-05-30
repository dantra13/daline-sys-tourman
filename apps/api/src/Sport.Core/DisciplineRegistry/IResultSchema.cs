using Sport.Core.Results;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Core.DisciplineRegistry;

public interface IResultSchema
{
    OutcomeMode OutcomeModeFor(EventTypeCode type);
    IReadOnlySet<ResultStatus> StatusesFor(EventTypeCode type);
    bool CanTransition(ResultStatus from, ResultStatus to, EventTypeCode type);
    IReadOnlySet<Irm> IrmCodesFor(EventTypeCode type);
    Result Validate(UnitResultDocument document);
    DisciplineResultProjection Project(UnitResultDocument document);
    ResultRollup AggregateSubunits(UnitResultDocument parent, IReadOnlyList<UnitResultDocument> contestResults);
}

// Permissive default used by disciplines without a real grammar yet.
public class DefaultResultSchema : IResultSchema
{
    private static readonly IReadOnlySet<ResultStatus> AllStatuses =
        new HashSet<ResultStatus>(Enum.GetValues<ResultStatus>());

    private static readonly IReadOnlySet<Irm> CoreIrms =
        new HashSet<Irm> { Irm.From("DNS"), Irm.From("DNF"), Irm.From("DSQ"), Irm.From("WDR") };

    public virtual OutcomeMode OutcomeModeFor(EventTypeCode type) => OutcomeMode.HeadToHead;
    public virtual IReadOnlySet<ResultStatus> StatusesFor(EventTypeCode type) => AllStatuses;

    // Coarse-grained default (non-vacuous): no self-transition, never rewind to StartList,
    // Official is terminal except on protest. Disciplines may override with a finer graph.
    public virtual bool CanTransition(ResultStatus from, ResultStatus to, EventTypeCode type)
    {
        if (from == to) return false;
        if (to == ResultStatus.StartList) return false;
        if (from == ResultStatus.Official && to != ResultStatus.Protested) return false;
        return true;
    }

    public virtual IReadOnlySet<Irm> IrmCodesFor(EventTypeCode type) => CoreIrms;
    public virtual Result Validate(UnitResultDocument document) => Result.Ok();
    public virtual DisciplineResultProjection Project(UnitResultDocument document) => new DefaultResultProjection();

    public virtual ResultRollup AggregateSubunits(UnitResultDocument parent, IReadOnlyList<UnitResultDocument> contestResults) =>
        throw new NotSupportedException("This discipline does not host subunits.");
}

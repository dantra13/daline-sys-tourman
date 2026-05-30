using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Core.Results;

public sealed class UnitResultDocument
{
    public UnitResultId Id { get; }
    public UnitId TargetUnitId { get; }
    public SubunitId? TargetSubunitId { get; }
    public Rsc TargetRsc { get; }
    public DisciplineCode DisciplineCode { get; }
    public EventTypeCode EventType { get; }
    public ResultStatus Status { get; private set; }
    public int Version { get; private set; }

    private readonly List<CompetitorResult> _competitors = new();
    public IReadOnlyList<CompetitorResult> Competitors => _competitors;

    private readonly List<ResultSegment> _segments = new();
    public IReadOnlyList<ResultSegment> Segments => _segments;

    private readonly List<ResultExtension> _extensions = new();
    public IReadOnlyList<ResultExtension> Extensions => _extensions;

    private UnitResultDocument(
        UnitResultId id, UnitId unitId, SubunitId? subunitId, Rsc rsc,
        DisciplineCode discipline, EventTypeCode eventType)
    {
        Id = id; TargetUnitId = unitId; TargetSubunitId = subunitId; TargetRsc = rsc;
        DisciplineCode = discipline; EventType = eventType;
        Status = ResultStatus.StartList; Version = 1;
    }

    public static UnitResultDocument CreateForUnit(
        UnitResultId id, UnitId unitId, Rsc unitRsc, DisciplineCode discipline, EventTypeCode eventType) =>
        new(id, unitId, null, unitRsc, discipline, eventType);

    public static UnitResultDocument CreateForSubunit(
        UnitResultId id, UnitId unitId, SubunitId subunitId, Rsc subunitRsc, DisciplineCode discipline, EventTypeCode eventType) =>
        new(id, unitId, subunitId, subunitRsc, discipline, eventType);

    public void ApplySnapshot(
        IReadOnlyList<CompetitorResult> competitors,
        IReadOnlyList<ResultSegment> segments,
        IReadOnlyList<ResultExtension> extensions,
        IResultSchema schema)
    {
        // Guards that can reject the change must run BEFORE the previous-state capture below, so a rejected snapshot leaves the document untouched.
        EnsureUniqueSortOrder(competitors);

        var prevC = _competitors.ToList();
        var prevS = _segments.ToList();
        var prevE = _extensions.ToList();

        Replace(_competitors, competitors);
        Replace(_segments, segments);
        Replace(_extensions, extensions);

        var validation = schema.Validate(this);
        if (!validation.IsSuccess)
        {
            Replace(_competitors, prevC);
            Replace(_segments, prevS);
            Replace(_extensions, prevE);
            throw new DomainException(validation.Code ?? "I-RES-8", validation.Error!);
        }
        Version++;
    }

    public void TransitionTo(ResultStatus to, IResultSchema schema)
    {
        EnsureTransition(to, schema);
        Status = to;
        Version++;
    }

    public void ApplyRollup(ResultRollup rollup, IResultSchema schema)
    {
        if (TargetSubunitId is not null)
            throw new DomainException("I-RES-10", "Rollup applies only to a parent (unit-level) document.");
        EnsureUniqueSortOrder(rollup.Competitors);

        // Validate the suggested transition BEFORE mutating, so a rejected status leaves the document untouched.
        var willTransition = rollup.SuggestedStatus is { } s && s != Status;
        if (willTransition)
            EnsureTransition(rollup.SuggestedStatus!.Value, schema);

        // A rollup updates competitor rows + extensions only; segments are not part of a rollup.
        Replace(_competitors, rollup.Competitors);
        Replace(_extensions, rollup.Extensions);
        if (willTransition)
            Status = rollup.SuggestedStatus!.Value;
        Version++;
    }

    private void EnsureTransition(ResultStatus to, IResultSchema schema)
    {
        if (!schema.StatusesFor(EventType).Contains(to))
            throw new DomainException("I-RES-4", $"ResultStatus '{to}' is not used by this discipline/event.");
        if (!schema.CanTransition(Status, to, EventType))
            throw new DomainException("I-RES-4", $"Illegal ResultStatus transition '{Status}' -> '{to}'.");
    }

    private static void EnsureUniqueSortOrder(IReadOnlyList<CompetitorResult> competitors)
    {
        var orders = competitors.Select(c => c.SortOrder).ToList();
        if (orders.Count != orders.Distinct().Count())
            throw new DomainException("I-RES-6", "CompetitorResult.SortOrder must be unique within a document.");
    }

    private static void Replace<T>(List<T> target, IReadOnlyList<T> source)
    {
        target.Clear();
        target.AddRange(source);
    }
}

using Sport.Core.Shared;

namespace Sport.Core.Structure;

public sealed class Phase
{
    public PhaseId Id { get; }
    public EventId EventId { get; }
    public PhaseCode Code { get; }
    public int Order { get; }
    public Rsc Rsc { get; }

    private readonly List<Unit> _units;
    public IReadOnlyList<Unit> Units => _units;

    private Phase(PhaseId id, EventId eventId, PhaseCode code, int order, Rsc rsc)
    {
        Id = id; EventId = eventId; Code = code; Order = order; Rsc = rsc;
        _units = new List<Unit>();
    }

    public static Phase Create(PhaseId id, EventId eventId, PhaseCode code, int order, Rsc eventRsc)
    {
        if (order < 0) throw new DomainException("I-STR-3", "Phase.Order must be non-negative.");
        var composed = string.Concat(
            eventRsc.Value.AsSpan(0, 22),
            code.Value.PadRight(4, Rsc.Filler),
            "--------".AsSpan());
        var rsc = Rsc.From(composed);
        return new Phase(id, eventId, code, order, rsc);
    }

    public void AddUnit(Unit unit)
    {
        if (unit.PhaseId != Id)
            throw new DomainException("I-STR-9", "Unit.PhaseId must match parent Phase.Id.");
        if (_units.Any(u => u.Code == unit.Code))
            throw new DomainException("I-STR-6", $"UnitCode '{unit.Code.Value}' already exists in Phase.");
        _units.Add(unit);
    }
}

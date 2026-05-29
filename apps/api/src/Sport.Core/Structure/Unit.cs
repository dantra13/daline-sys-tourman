using Sport.Core.Shared;

namespace Sport.Core.Structure;

public sealed class Unit
{
    public UnitId Id { get; }
    public PhaseId PhaseId { get; }
    public UnitCode Code { get; }
    public DateTimeOffset? ScheduledStart { get; private set; }
    public Rsc Rsc { get; }

    private readonly List<Subunit> _subunits;
    public IReadOnlyList<Subunit> Subunits => _subunits;

    public Guid? DisciplineUnitRef { get; private set; }

    // Required by EF Core materializer (cannot bind collection nav params).
#pragma warning disable CS8618
    private Unit() { _subunits = new List<Subunit>(); }
#pragma warning restore CS8618

    private Unit(UnitId id, PhaseId phaseId, UnitCode code, DateTimeOffset? start, Rsc rsc, List<Subunit> subunits)
    {
        Id = id; PhaseId = phaseId; Code = code; ScheduledStart = start; Rsc = rsc; _subunits = subunits;
    }

    public static Unit CreateAtomic(UnitId id, PhaseId phaseId, UnitCode code, Rsc phaseRsc, DateTimeOffset? scheduledStart)
    {
        if (!code.Value.EndsWith("--", StringComparison.Ordinal))
            throw new DomainException("I-STR-14", "Atomic UnitCode must end with '--' (positions 7-8 are reserved for subunits).");
        var rsc = ComposeUnitRsc(code, phaseRsc);
        return new Unit(id, phaseId, code, scheduledStart, rsc, new List<Subunit>());
    }

    public static Unit CreateParentForSubunits(UnitId id, PhaseId phaseId, UnitCode code, Rsc phaseRsc, DateTimeOffset? scheduledStart)
    {
        if (!code.Value.EndsWith("00", StringComparison.Ordinal))
            throw new DomainException("I-STR-7", "UnitCode for a parent of subunits must end with '00'.");
        var rsc = ComposeUnitRsc(code, phaseRsc);
        return new Unit(id, phaseId, code, scheduledStart, rsc, new List<Subunit>());
    }

    public void AddSubunit(Subunit subunit)
    {
        if (subunit.UnitId != Id)
            throw new DomainException("I-STR-10", "Subunit.UnitId must match parent Unit.Id.");
        if (_subunits.Any(s => s.Code == subunit.Code))
            throw new DomainException("I-STR-8", $"SubunitCode '{subunit.Code.Value}' already exists in Unit.");
        _subunits.Add(subunit);
    }

    public void LinkDisciplineRef(Guid disciplineRef)
    {
        if (DisciplineUnitRef is not null)
            throw new DomainException("I-STR-11", "Unit already linked to a discipline-specific entity.");
        DisciplineUnitRef = disciplineRef;
    }

    private static Rsc ComposeUnitRsc(UnitCode code, Rsc phaseRsc)
    {
        var s = phaseRsc.Value;
        var composed = string.Concat(s.AsSpan(0, 26), code.Value);
        return Rsc.From(composed);
    }
}

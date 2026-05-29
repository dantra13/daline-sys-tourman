using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;

namespace Sport.Core.Structure;

public sealed class Event
{
    public EventId Id { get; }
    public CompetitionDisciplineId CompetitionDisciplineId { get; }
    public DisciplineCode DisciplineCode { get; }
    public GenderCode Gender { get; }
    public EventTypeCode EventType { get; }
    public EventModifierCode? EventModifier { get; }
    public string Name { get; }
    public Rsc Rsc { get; }

    private readonly List<Phase> _phases;
    public IReadOnlyList<Phase> Phases => _phases;

    private Event(
        EventId id,
        CompetitionDisciplineId competitionDisciplineId,
        DisciplineCode disciplineCode,
        GenderCode gender,
        EventTypeCode eventType,
        EventModifierCode? eventModifier,
        string name,
        Rsc rsc)
    {
        Id = id;
        CompetitionDisciplineId = competitionDisciplineId;
        DisciplineCode = disciplineCode;
        Gender = gender;
        EventType = eventType;
        EventModifier = eventModifier;
        Name = name;
        Rsc = rsc;
        _phases = new List<Phase>();
    }

    public static Event Create(
        EventId id,
        CompetitionDisciplineId competitionDisciplineId,
        DisciplineCode disciplineCode,
        GenderCode gender,
        EventTypeCode eventType,
        EventModifierCode? modifier,
        string name,
        IDisciplineModule disciplineModule)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("I-STR-2", "Event.Name is required.");

        if (!disciplineModule.SupportedGenders.Contains(gender))
            throw new DomainException("I-STR-1", $"Gender '{gender}' is not supported by discipline '{disciplineCode.Value}'.");

        var validation = disciplineModule.ValidateEventType(eventType, modifier);
        if (!validation.IsSuccess) throw new DomainException("I-STR-12", validation.Error!);

        var rsc = Rsc.Compose(disciplineCode, gender, eventType, modifier, phase: null, unit: null, subunit: null);
        return new Event(id, competitionDisciplineId, disciplineCode, gender, eventType, modifier, name, rsc);
    }

    public Phase AddPhase(PhaseCode code, int order, IDisciplineModule disciplineModule)
    {
        var validation = disciplineModule.ValidatePhaseForEventType(EventType, code);
        if (!validation.IsSuccess) throw new DomainException("I-STR-13", validation.Error!);

        if (_phases.Any(p => p.Order == order))
            throw new DomainException("I-STR-4", $"Phase.Order {order} already exists in Event.");
        if (_phases.Any(p => p.Code == code))
            throw new DomainException("I-STR-5", $"PhaseCode '{code.Value}' already exists in Event.");

        var phase = Phase.Create(PhaseId.New(), Id, code, order, Rsc);
        _phases.Add(phase);
        return phase;
    }

    public Unit AddAtomicUnit(
        PhaseCode phaseCode,
        UnitCode code,
        IDisciplineModule disciplineModule,
        DateTimeOffset? scheduledStart)
    {
        if (disciplineModule.HostsSubunits(EventType))
            throw new DomainException("I-STR-16",
                $"EventType '{EventType.Value}' hosts subunits; use AddTeamMatchUnit.");

        var phase = FindPhase(phaseCode);
        var unit = Unit.CreateAtomic(UnitId.New(), phase.Id, code, phase.Rsc, scheduledStart);
        phase.AddUnit(unit);
        return unit;
    }

    public Unit AddTeamMatchUnit(
        PhaseCode phaseCode,
        UnitCode parentCode,
        IReadOnlyCollection<SubunitCode> contests,
        IDisciplineModule disciplineModule,
        DateTimeOffset? scheduledStart)
    {
        if (!disciplineModule.HostsSubunits(EventType))
            throw new DomainException("I-STR-15",
                $"EventType '{EventType.Value}' does not host subunits.");

        if (contests.Count == 0)
            throw new DomainException("I-STR-19",
                $"EventType '{EventType.Value}' requires at least one subunit.");

        foreach (var contest in contests)
        {
            var validation = disciplineModule.ValidateSubunitCode(EventType, contest);
            if (!validation.IsSuccess) throw new DomainException("I-STR-17", validation.Error!);
        }

        var phase = FindPhase(phaseCode);
        var parent = Unit.CreateParentForSubunits(UnitId.New(), phase.Id, parentCode, phase.Rsc, scheduledStart);
        foreach (var contest in contests)
            parent.AddSubunit(Subunit.Create(SubunitId.New(), parent.Id, contest, parent.Rsc));
        phase.AddUnit(parent);
        return parent;
    }

    private Phase FindPhase(PhaseCode code) =>
        _phases.FirstOrDefault(p => p.Code == code)
            ?? throw new DomainException("I-STR-18", $"Phase '{code.Value}' not found in Event.");
}

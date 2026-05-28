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
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Event.Name is required.");

        if (!disciplineModule.SupportedGenders.Contains(gender))
            throw new DomainException($"Gender '{gender}' is not supported by discipline '{disciplineCode.Value}' (I-STR-1).");

        var validation = disciplineModule.ValidateEventType(eventType, modifier);
        if (!validation.IsSuccess) throw new DomainException(validation.Error!);

        var rsc = Rsc.Compose(disciplineCode, gender, eventType, modifier, phase: null, unit: null, subunit: null);
        return new Event(id, competitionDisciplineId, disciplineCode, gender, eventType, modifier, name, rsc);
    }

    public Phase AddPhase(PhaseCode code, int order, IDisciplineModule disciplineModule)
    {
        var validation = disciplineModule.ValidatePhaseForEventType(EventType, code);
        if (!validation.IsSuccess) throw new DomainException(validation.Error!);

        if (_phases.Any(p => p.Order == order))
            throw new DomainException($"Phase.Order {order} already exists in Event (I-STR-4).");
        if (_phases.Any(p => p.Code == code))
            throw new DomainException($"PhaseCode '{code.Value}' already exists in Event (I-STR-5).");

        var phase = Phase.Create(PhaseId.New(), Id, code, order, Rsc);
        _phases.Add(phase);
        return phase;
    }
}

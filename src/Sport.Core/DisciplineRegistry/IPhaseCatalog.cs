using Sport.Core.Structure;

namespace Sport.Core.DisciplineRegistry;

public interface IPhaseCatalog
{
    IReadOnlyCollection<PhaseCode> AllowedPhases { get; }
    bool IsAllowedForEventType(EventTypeCode eventType, PhaseCode phase);
}

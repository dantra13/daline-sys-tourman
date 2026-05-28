using Sport.Core.Officials;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Core.DisciplineRegistry;

public interface IDisciplineModule
{
    DisciplineCode Code { get; }
    string DisplayName { get; }
    IReadOnlySet<GenderCode> SupportedGenders { get; }

    IReadOnlyCollection<EventTypeDescriptor> EventTypes { get; }
    IPhaseCatalog PhaseCatalog { get; }
    IUnitCodeStrategy UnitCodeStrategy { get; }
    IReadOnlyCollection<FunctionDescriptor> Functions { get; }
    IEntryRules EntryRules { get; }

    Result ValidateEventType(EventTypeCode type, EventModifierCode? modifier);
    Result ValidatePhaseForEventType(EventTypeCode type, PhaseCode phase);
    Result ValidateUnitCode(EventTypeCode type, PhaseCode phase, UnitCode code);
    Result ValidateEntry(EntryCandidate candidate);
    Result ValidateOfficialFunctionInScope(FunctionCode function, ScopeLevel level);
}

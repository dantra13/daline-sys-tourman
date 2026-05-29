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

    // Subunit-hosting (default implementations read from EventTypes; disciplines opt in
    // by declaring HostsSubunits/CanonicalSubunits on the matching EventTypeDescriptor).
    bool HostsSubunits(EventTypeCode type) =>
        EventTypes.FirstOrDefault(e => e.Code == type)?.HostsSubunits ?? false;

    IReadOnlyCollection<SubunitCode> SubunitsFor(EventTypeCode type) =>
        EventTypes.FirstOrDefault(e => e.Code == type)?.CanonicalSubunits ?? Array.Empty<SubunitCode>();

    Result ValidateSubunitCode(EventTypeCode type, SubunitCode code) =>
        SubunitsFor(type).Contains(code)
            ? Result.Ok()
            : Result.Fail($"SubunitCode '{code.Value}' is not valid for event type '{type.Value}'.");
}

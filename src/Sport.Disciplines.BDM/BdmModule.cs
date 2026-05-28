using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.BDM;

public sealed class BdmModule : IDisciplineModule
{
    public DisciplineCode Code { get; } = DisciplineCode.From("BDM");
    public string DisplayName => "Badminton";
    public IReadOnlySet<GenderCode> SupportedGenders { get; } =
        new HashSet<GenderCode> { GenderCode.M, GenderCode.W, GenderCode.X };

    public IReadOnlyCollection<EventTypeDescriptor> EventTypes { get; } = new[]
    {
        new EventTypeDescriptor(EventTypeCode.From("SINGLES"),  "Singles",        new HashSet<GenderCode> { GenderCode.M, GenderCode.W }, ModifierContract.Forbidden),
        new EventTypeDescriptor(EventTypeCode.From("DOUBLES"),  "Doubles",        new HashSet<GenderCode> { GenderCode.M, GenderCode.W }, ModifierContract.Forbidden),
        new EventTypeDescriptor(EventTypeCode.From("MIXEDDOU"), "Mixed Doubles",  new HashSet<GenderCode> { GenderCode.X },               ModifierContract.Forbidden),
    };

    public IPhaseCatalog PhaseCatalog { get; } = new BdmPhaseCatalog();
    public IUnitCodeStrategy UnitCodeStrategy { get; } = new BdmUnitCodeStrategy();

    public IReadOnlyCollection<FunctionDescriptor> Functions { get; } = new[]
    {
        new FunctionDescriptor(FunctionCode.From("BDM.UMP"),  "Umpire",         new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
        new FunctionDescriptor(FunctionCode.From("BDM.SVJU"), "Service Judge",  new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
        new FunctionDescriptor(FunctionCode.From("BDM.LIJU"), "Line Judge",     new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
    };

    public IEntryRules EntryRules { get; } = new BdmEntryRules();

    public Result ValidateEventType(EventTypeCode type, EventModifierCode? modifier)
    {
        if (!EventTypes.Any(e => e.Code == type))
            return Result.Fail($"EventType '{type.Value}' not supported by BDM.");
        if (modifier is not null)
            return Result.Fail("BDM EventType does not accept a modifier.");
        return Result.Ok();
    }

    public Result ValidatePhaseForEventType(EventTypeCode type, PhaseCode phase) =>
        PhaseCatalog.IsAllowedForEventType(type, phase)
            ? Result.Ok()
            : Result.Fail($"Phase '{phase.Value}' is not allowed for BDM EventType '{type.Value}'.");

    public Result ValidateUnitCode(EventTypeCode type, PhaseCode phase, UnitCode code) =>
        UnitCodeStrategy.IsValid(code) ? Result.Ok() : Result.Fail("Invalid UnitCode for BDM.");

    public Result ValidateEntry(EntryCandidate candidate) => EntryRules.Validate(candidate);

    public Result ValidateOfficialFunctionInScope(FunctionCode function, ScopeLevel level)
    {
        var f = Functions.FirstOrDefault(x => x.Code == function);
        if (f is null) return Result.Fail($"Function '{function.Value}' not registered in BDM.");
        if (!f.ValidScopes.Contains(level)) return Result.Fail($"Function '{function.Value}' not valid at scope '{level}'.");
        return Result.Ok();
    }
}

using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.VBV;

public sealed class VbvModule : IDisciplineModule
{
    public DisciplineCode Code { get; } = DisciplineCode.From("VBV");
    public string DisplayName => "Beach Volleyball";
    public IReadOnlySet<GenderCode> SupportedGenders { get; } =
        new HashSet<GenderCode> { GenderCode.M, GenderCode.W };

    public IReadOnlyCollection<EventTypeDescriptor> EventTypes { get; } = new[]
    {
        new EventTypeDescriptor(
            EventTypeCode.From("TEAM2"),
            "Beach Volleyball Pairs",
            new HashSet<GenderCode> { GenderCode.M, GenderCode.W },
            ModifierContract.Forbidden),
    };

    public IPhaseCatalog PhaseCatalog { get; } = new VbvPhaseCatalog();
    public IUnitCodeStrategy UnitCodeStrategy { get; } = new VbvUnitCodeStrategy();

    public IReadOnlyCollection<FunctionDescriptor> Functions { get; } = new[]
    {
        new FunctionDescriptor(FunctionCode.From("VBV.REF1"), "First Referee",  new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
        new FunctionDescriptor(FunctionCode.From("VBV.REF2"), "Second Referee", new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
        new FunctionDescriptor(FunctionCode.From("VBV.LIJU"), "Line Judge",     new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
    };

    public IEntryRules EntryRules { get; } = new VbvEntryRules();

    public Result ValidateEventType(EventTypeCode type, EventModifierCode? modifier)
    {
        if (!EventTypes.Any(e => e.Code == type))
            return Result.Fail($"EventType '{type.Value}' not supported by VBV.");
        if (modifier is not null)
            return Result.Fail("VBV EventType does not accept a modifier.");
        return Result.Ok();
    }

    public Result ValidatePhaseForEventType(EventTypeCode type, PhaseCode phase) =>
        PhaseCatalog.IsAllowedForEventType(type, phase)
            ? Result.Ok()
            : Result.Fail($"Phase '{phase.Value}' is not allowed for VBV EventType '{type.Value}'.");

    public Result ValidateUnitCode(EventTypeCode type, PhaseCode phase, UnitCode code) =>
        UnitCodeStrategy.IsValid(code) ? Result.Ok() : Result.Fail("Invalid UnitCode for VBV.");

    public Result ValidateEntry(EntryCandidate candidate) => EntryRules.Validate(candidate);

    public Result ValidateOfficialFunctionInScope(FunctionCode function, ScopeLevel level)
    {
        var f = Functions.FirstOrDefault(x => x.Code == function);
        if (f is null) return Result.Fail($"Function '{function.Value}' not registered in VBV.");
        if (!f.ValidScopes.Contains(level)) return Result.Fail($"Function '{function.Value}' not valid at scope '{level}'.");
        return Result.Ok();
    }
}

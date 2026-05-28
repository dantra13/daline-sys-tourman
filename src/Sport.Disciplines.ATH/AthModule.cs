using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.ATH;

public sealed class AthModule : IDisciplineModule
{
    public DisciplineCode Code { get; } = DisciplineCode.From("ATH");
    public string DisplayName => "Athletics";
    public IReadOnlySet<GenderCode> SupportedGenders { get; } =
        new HashSet<GenderCode> { GenderCode.M, GenderCode.W };

    public IReadOnlyCollection<EventTypeDescriptor> EventTypes { get; } = new[]
    {
        new EventTypeDescriptor(
            EventTypeCode.From("HJ"),
            "High Jump",
            new HashSet<GenderCode> { GenderCode.M, GenderCode.W },
            ModifierContract.Forbidden),
    };

    public IPhaseCatalog PhaseCatalog { get; } = new AthPhaseCatalog();
    public IUnitCodeStrategy UnitCodeStrategy { get; } = new AthUnitCodeStrategy();

    public IReadOnlyCollection<FunctionDescriptor> Functions { get; } = new[]
    {
        new FunctionDescriptor(FunctionCode.From("ATH.STJU"),     "Starter Judge",  new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
        new FunctionDescriptor(FunctionCode.From("ATH.PHOTOFIN"), "Photo Finish",   new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
    };

    public IEntryRules EntryRules { get; } = new AthEntryRules();

    public Result ValidateEventType(EventTypeCode type, EventModifierCode? modifier)
    {
        if (!EventTypes.Any(e => e.Code == type))
            return Result.Fail($"EventType '{type.Value}' not supported by ATH.");
        if (modifier is not null)
            return Result.Fail("ATH EventType does not accept a modifier.");
        return Result.Ok();
    }

    public Result ValidatePhaseForEventType(EventTypeCode type, PhaseCode phase) =>
        PhaseCatalog.IsAllowedForEventType(type, phase)
            ? Result.Ok()
            : Result.Fail($"Phase '{phase.Value}' is not allowed for ATH EventType '{type.Value}'.");

    public Result ValidateUnitCode(EventTypeCode type, PhaseCode phase, UnitCode code) =>
        UnitCodeStrategy.IsValid(code) ? Result.Ok() : Result.Fail("Invalid UnitCode for ATH.");

    public Result ValidateEntry(EntryCandidate candidate) => EntryRules.Validate(candidate);

    public Result ValidateOfficialFunctionInScope(FunctionCode function, ScopeLevel level)
    {
        var f = Functions.FirstOrDefault(x => x.Code == function);
        if (f is null) return Result.Fail($"Function '{function.Value}' not registered in ATH.");
        if (!f.ValidScopes.Contains(level)) return Result.Fail($"Function '{function.Value}' not valid at scope '{level}'.");
        return Result.Ok();
    }
}

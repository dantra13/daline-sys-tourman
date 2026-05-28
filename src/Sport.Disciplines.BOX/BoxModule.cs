using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.BOX;

public sealed class BoxModule : IDisciplineModule
{
    private static readonly string[] WeightCategories =
        { "48KG", "54KG", "60KG", "75KG", "81KG", "91KG" };

    public DisciplineCode Code { get; } = DisciplineCode.From("BOX");
    public string DisplayName => "Boxing";
    public IReadOnlySet<GenderCode> SupportedGenders { get; } =
        new HashSet<GenderCode> { GenderCode.M, GenderCode.W };

    public IReadOnlyCollection<EventTypeDescriptor> EventTypes { get; } =
        WeightCategories
            .Select(c => new EventTypeDescriptor(
                EventTypeCode.From(c),
                $"Boxing {c}",
                new HashSet<GenderCode> { GenderCode.M, GenderCode.W },
                ModifierContract.Forbidden))
            .ToArray();

    public IPhaseCatalog PhaseCatalog { get; } = new BoxPhaseCatalog();
    public IUnitCodeStrategy UnitCodeStrategy { get; } = new BoxUnitCodeStrategy();

    public IReadOnlyCollection<FunctionDescriptor> Functions { get; } = new[]
    {
        new FunctionDescriptor(FunctionCode.From("BOX.REF"),  "Referee",  new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
        new FunctionDescriptor(FunctionCode.From("BOX.JUD1"), "Judge 1",  new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
        new FunctionDescriptor(FunctionCode.From("BOX.JUD2"), "Judge 2",  new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
        new FunctionDescriptor(FunctionCode.From("BOX.JUD3"), "Judge 3",  new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
        new FunctionDescriptor(FunctionCode.From("BOX.JUD4"), "Judge 4",  new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
        new FunctionDescriptor(FunctionCode.From("BOX.JUD5"), "Judge 5",  new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
        new FunctionDescriptor(FunctionCode.From("BOX.TIMK"), "Timekeeper", new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
    };

    public IEntryRules EntryRules { get; } = new BoxEntryRules();

    public Result ValidateEventType(EventTypeCode type, EventModifierCode? modifier)
    {
        if (!EventTypes.Any(e => e.Code == type))
            return Result.Fail($"EventType '{type.Value}' not supported by BOX.");
        if (modifier is not null)
            return Result.Fail("BOX EventType does not accept a modifier.");
        return Result.Ok();
    }

    public Result ValidatePhaseForEventType(EventTypeCode type, PhaseCode phase) =>
        PhaseCatalog.IsAllowedForEventType(type, phase)
            ? Result.Ok()
            : Result.Fail($"Phase '{phase.Value}' is not allowed for BOX EventType '{type.Value}'.");

    public Result ValidateUnitCode(EventTypeCode type, PhaseCode phase, UnitCode code) =>
        UnitCodeStrategy.IsValid(code) ? Result.Ok() : Result.Fail("Invalid UnitCode for BOX.");

    public Result ValidateEntry(EntryCandidate candidate) => EntryRules.Validate(candidate);

    public Result ValidateOfficialFunctionInScope(FunctionCode function, ScopeLevel level)
    {
        var f = Functions.FirstOrDefault(x => x.Code == function);
        if (f is null) return Result.Fail($"Function '{function.Value}' not registered in BOX.");
        if (!f.ValidScopes.Contains(level)) return Result.Fail($"Function '{function.Value}' not valid at scope '{level}'.");
        return Result.Ok();
    }
}

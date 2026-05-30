using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD;

public sealed class JudModule : IDisciplineModule
{
    public DisciplineCode Code { get; } = DisciplineCode.From("JUD");
    public string DisplayName => "Judo";
    public IReadOnlySet<GenderCode> SupportedGenders { get; } =
        new HashSet<GenderCode> { GenderCode.M, GenderCode.W, GenderCode.X };

    public IReadOnlyCollection<EventTypeDescriptor> EventTypes { get; } = BuildEventTypes();

    public IPhaseCatalog PhaseCatalog { get; } = new JudPhaseCatalog();
    public IUnitCodeStrategy UnitCodeStrategy { get; } = new JudUnitCodeStrategy();

    public IReadOnlyCollection<FunctionDescriptor> Functions { get; } = new[]
    {
        new FunctionDescriptor(FunctionCode.From("JUD.REF"),  "Referee",     new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
        new FunctionDescriptor(FunctionCode.From("JUD.JUD1"), "Judge 1",     new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
        new FunctionDescriptor(FunctionCode.From("JUD.JUD2"), "Judge 2",     new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
        new FunctionDescriptor(FunctionCode.From("JUD.CARE"), "CARE System", new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
    };

    public IEntryRules EntryRules { get; } = new JudEntryRules();
    public IResultSchema ResultSchema { get; } = new JudResultSchema();

    private static EventTypeDescriptor[] BuildEventTypes()
    {
        var list = new List<EventTypeDescriptor>();

        foreach (var c in JudWeightCategories.Men)
            list.Add(new EventTypeDescriptor(
                EventTypeCode.From(c), $"Men's {c}",
                new HashSet<GenderCode> { GenderCode.M }, ModifierContract.Forbidden));

        foreach (var c in JudWeightCategories.Women)
            list.Add(new EventTypeDescriptor(
                EventTypeCode.From(c), $"Women's {c}",
                new HashSet<GenderCode> { GenderCode.W }, ModifierContract.Forbidden));

        list.Add(new EventTypeDescriptor(
            EventTypeCode.From(JudWeightCategories.TeamEventType), "Mixed Team",
            new HashSet<GenderCode> { GenderCode.X }, ModifierContract.Forbidden)
        {
            HostsSubunits = true,
            CanonicalSubunits = JudWeightCategories.TeamContests,
        });

        return list.ToArray();
    }

    public Result ValidateEventType(EventTypeCode type, EventModifierCode? modifier)
    {
        if (!EventTypes.Any(e => e.Code == type))
            return Result.Fail($"EventType '{type.Value}' not supported by JUD.");
        if (modifier is not null)
            return Result.Fail("JUD EventType does not accept a modifier.");
        return Result.Ok();
    }

    public Result ValidatePhaseForEventType(EventTypeCode type, PhaseCode phase) =>
        PhaseCatalog.IsAllowedForEventType(type, phase)
            ? Result.Ok()
            : Result.Fail($"Phase '{phase.Value}' is not allowed for JUD EventType '{type.Value}'.");

    public Result ValidateUnitCode(EventTypeCode type, PhaseCode phase, UnitCode code) =>
        UnitCodeStrategy.IsValid(code) ? Result.Ok() : Result.Fail("Invalid UnitCode for JUD.");

    public Result ValidateEntry(EntryCandidate candidate) => EntryRules.Validate(candidate);

    public Result ValidateOfficialFunctionInScope(FunctionCode function, ScopeLevel level)
    {
        var f = Functions.FirstOrDefault(x => x.Code == function);
        if (f is null) return Result.Fail($"Function '{function.Value}' not registered in JUD.");
        if (!f.ValidScopes.Contains(level)) return Result.Fail($"Function '{function.Value}' not valid at scope '{level}'.");
        return Result.Ok();
    }
}

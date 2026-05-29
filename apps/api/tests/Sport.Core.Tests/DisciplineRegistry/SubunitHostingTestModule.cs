using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Core.Tests.DisciplineRegistry;

/// <summary>
/// Test double whose EventTypes include one subunit-hosting team type (TEAM2, X, contests 01/02)
/// and one atomic individual type (57KG, W). Phase/unit-code validation always succeeds so the
/// double can drive Event assembly tests.
/// </summary>
internal sealed class SubunitHostingTestModule : IDisciplineModule
{
    public DisciplineCode Code => DisciplineCode.From("TST");
    public string DisplayName => "Test";
    public IReadOnlySet<GenderCode> SupportedGenders { get; } =
        new HashSet<GenderCode> { GenderCode.X, GenderCode.W };

    public IReadOnlyCollection<EventTypeDescriptor> EventTypes { get; } = new[]
    {
        new EventTypeDescriptor(EventTypeCode.From("TEAM2"), "Test Team",
            new HashSet<GenderCode> { GenderCode.X }, ModifierContract.Forbidden)
        {
            HostsSubunits = true,
            CanonicalSubunits = new[] { SubunitCode.From("01"), SubunitCode.From("02") },
        },
        new EventTypeDescriptor(EventTypeCode.From("57KG"), "Test 57KG",
            new HashSet<GenderCode> { GenderCode.W }, ModifierContract.Forbidden),
    };

    public IPhaseCatalog PhaseCatalog => throw new NotImplementedException();
    public IUnitCodeStrategy UnitCodeStrategy => throw new NotImplementedException();
    public IReadOnlyCollection<FunctionDescriptor> Functions => Array.Empty<FunctionDescriptor>();
    public IEntryRules EntryRules => throw new NotImplementedException();

    public Result ValidateEventType(EventTypeCode type, EventModifierCode? modifier) => Result.Ok();
    public Result ValidatePhaseForEventType(EventTypeCode type, PhaseCode phase) => Result.Ok();
    public Result ValidateUnitCode(EventTypeCode type, PhaseCode phase, UnitCode code) => Result.Ok();
    public Result ValidateEntry(EntryCandidate candidate) => Result.Ok();
    public Result ValidateOfficialFunctionInScope(FunctionCode function, ScopeLevel level) => Result.Ok();
}

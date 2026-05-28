using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Core.Tests.DisciplineRegistry;

public class DisciplineRegistryTests
{
    [Fact]
    public void Register_then_get_returns_module()
    {
        var module = new FakeModule(DisciplineCode.From("FBL"));
        var registry = new Sport.Core.DisciplineRegistry.DisciplineRegistry();
        registry.Register(module);

        registry.IsRegistered(DisciplineCode.From("FBL")).Should().BeTrue();
        registry.Get(DisciplineCode.From("FBL")).Should().BeSameAs(module);
    }

    [Fact]
    public void Registering_the_same_discipline_twice_throws()
    {
        var registry = new Sport.Core.DisciplineRegistry.DisciplineRegistry();
        registry.Register(new FakeModule(DisciplineCode.From("FBL")));

        var act = () => registry.Register(new FakeModule(DisciplineCode.From("FBL")));
        act.Should().Throw<DomainException>().WithMessage("*already registered*");
    }

    [Fact]
    public void Get_unknown_discipline_throws()
    {
        var registry = new Sport.Core.DisciplineRegistry.DisciplineRegistry();
        var act = () => registry.Get(DisciplineCode.From("XXX"));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CommonFunctions_exposes_coach_manager_medical()
    {
        var codes = Sport.Core.DisciplineRegistry.CommonFunctions.All.Select(f => f.Code.Value).ToHashSet();
        codes.Should().Contain(new[] { "COMMON.COACH", "COMMON.MANAGER", "COMMON.MEDICAL" });
    }

    private sealed class FakeModule(DisciplineCode code) : IDisciplineModule
    {
        public DisciplineCode Code { get; } = code;
        public string DisplayName => "Fake";
        public IReadOnlySet<GenderCode> SupportedGenders { get; } = new HashSet<GenderCode> { GenderCode.M };
        public IReadOnlyCollection<EventTypeDescriptor> EventTypes => Array.Empty<EventTypeDescriptor>();
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
}

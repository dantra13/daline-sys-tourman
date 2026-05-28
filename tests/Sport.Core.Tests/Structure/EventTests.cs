using FluentAssertions;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Core.Tests.Structure;

public class EventTests
{
    private static readonly DisciplineCode Fbl = DisciplineCode.From("FBL");
    private static readonly EventTypeCode Team11 = EventTypeCode.From("TEAM11");

    [Fact]
    public void Create_with_valid_inputs_composes_event_rsc()
    {
        var module = new ValidatingModule(Fbl);
        var ev = Event.Create(
            EventId.New(), CompetitionDisciplineId.New(), Fbl, GenderCode.M,
            Team11, modifier: null, name: "Men's Football",
            disciplineModule: module);

        ev.Rsc.Value.Should().Be("FBLMTEAM11------------------------");
    }

    [Fact]
    public void Create_rejects_gender_not_supported_I_STR_1()
    {
        var module = new ValidatingModule(Fbl, supportedGenders: new HashSet<GenderCode> { GenderCode.M });
        var act = () => Event.Create(
            EventId.New(), CompetitionDisciplineId.New(), Fbl, GenderCode.W,
            Team11, null, "x", module);
        act.Should().Throw<DomainException>().WithMessage("*not supported*");
    }

    [Fact]
    public void Create_rejects_invalid_event_type_I_STR_2()
    {
        var module = new ValidatingModule(Fbl, eventTypeResult: Result.Fail("bad event"));
        var act = () => Event.Create(
            EventId.New(), CompetitionDisciplineId.New(), Fbl, GenderCode.M,
            Team11, null, "x", module);
        act.Should().Throw<DomainException>().WithMessage("*bad event*");
    }

    [Fact]
    public void AddPhase_calls_module_to_validate_phase_I_STR_3()
    {
        var module = new ValidatingModule(Fbl, phaseResult: Result.Fail("bad phase"));
        var ev = Event.Create(
            EventId.New(), CompetitionDisciplineId.New(), Fbl, GenderCode.M,
            Team11, null, "x", module);

        var act = () => ev.AddPhase(PhaseCode.From("QFNL"), 1, module);
        act.Should().Throw<DomainException>().WithMessage("*bad phase*");
    }

    [Fact]
    public void AddPhase_rejects_duplicate_order_I_STR_4()
    {
        var module = new ValidatingModule(Fbl);
        var ev = Event.Create(EventId.New(), CompetitionDisciplineId.New(), Fbl, GenderCode.M, Team11, null, "x", module);
        ev.AddPhase(PhaseCode.From("QFNL"), 1, module);

        var act = () => ev.AddPhase(PhaseCode.From("SFNL"), 1, module);
        act.Should().Throw<DomainException>().WithMessage("*Order*");
    }

    [Fact]
    public void AddPhase_rejects_duplicate_phase_code_I_STR_5()
    {
        var module = new ValidatingModule(Fbl);
        var ev = Event.Create(EventId.New(), CompetitionDisciplineId.New(), Fbl, GenderCode.M, Team11, null, "x", module);
        ev.AddPhase(PhaseCode.From("QFNL"), 1, module);

        var act = () => ev.AddPhase(PhaseCode.From("QFNL"), 2, module);
        act.Should().Throw<DomainException>().WithMessage("*PhaseCode*already*");
    }

    private sealed class ValidatingModule : IDisciplineModule
    {
        private readonly Result _eventTypeResult;
        private readonly Result _phaseResult;

        public ValidatingModule(
            DisciplineCode code,
            IReadOnlySet<GenderCode>? supportedGenders = null,
            Result? eventTypeResult = null,
            Result? phaseResult = null)
        {
            Code = code;
            SupportedGenders = supportedGenders ?? new HashSet<GenderCode> { GenderCode.M, GenderCode.W };
            _eventTypeResult = eventTypeResult ?? Result.Ok();
            _phaseResult = phaseResult ?? Result.Ok();
        }

        public DisciplineCode Code { get; }
        public string DisplayName => "fake";
        public IReadOnlySet<GenderCode> SupportedGenders { get; }
        public IReadOnlyCollection<EventTypeDescriptor> EventTypes => Array.Empty<EventTypeDescriptor>();
        public IPhaseCatalog PhaseCatalog => throw new NotImplementedException();
        public IUnitCodeStrategy UnitCodeStrategy => throw new NotImplementedException();
        public IReadOnlyCollection<FunctionDescriptor> Functions => Array.Empty<FunctionDescriptor>();
        public IEntryRules EntryRules => throw new NotImplementedException();
        public Result ValidateEventType(EventTypeCode type, EventModifierCode? modifier) => _eventTypeResult;
        public Result ValidatePhaseForEventType(EventTypeCode type, PhaseCode phase) => _phaseResult;
        public Result ValidateUnitCode(EventTypeCode type, PhaseCode phase, UnitCode code) => Result.Ok();
        public Result ValidateEntry(EntryCandidate candidate) => Result.Ok();
        public Result ValidateOfficialFunctionInScope(FunctionCode function, ScopeLevel level) => Result.Ok();
    }
}

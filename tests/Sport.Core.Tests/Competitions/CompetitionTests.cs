using FluentAssertions;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Core.Tests.Competitions;

public class CompetitionTests
{
    private static readonly DateOnly D1 = new(2026, 6, 1);
    private static readonly DateOnly D2 = new(2026, 6, 10);
    private static readonly DisciplineCode Fbl = DisciplineCode.From("FBL");
    private static readonly DisciplineCode Bkb = DisciplineCode.From("BKB");

    private static FakeRegistry MakeRegistry(params DisciplineCode[] supported)
    {
        var reg = new FakeRegistry();
        foreach (var d in supported) reg.SupportedCodes.Add(d);
        reg.GendersByCode[Fbl] = new HashSet<GenderCode> { GenderCode.M, GenderCode.W };
        reg.GendersByCode[Bkb] = new HashSet<GenderCode> { GenderCode.M, GenderCode.W };
        return reg;
    }

    [Fact]
    public void Create_with_one_registered_discipline_succeeds()
    {
        var registry = MakeRegistry(Fbl);
        var comp = Competition.Create(
            CompetitionId.New(),
            CompetitionCode.From("copa-2026"),
            "Copa 2026",
            DateRange.Create(D1, D2),
            new[] { (Fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.M }) },
            registry);

        comp.Disciplines.Should().HaveCount(1);
    }

    [Fact]
    public void Create_with_zero_disciplines_throws_I_COMP_1()
    {
        var registry = MakeRegistry(Fbl);
        var act = () => Competition.Create(
            CompetitionId.New(),
            CompetitionCode.From("copa-2026"),
            "Copa",
            DateRange.Create(D1, D2),
            Array.Empty<(DisciplineCode, IReadOnlySet<GenderCode>)>(),
            registry);
        act.Should().Throw<DomainException>().WithMessage("*at least 1*");
    }

    [Fact]
    public void Create_with_unregistered_discipline_throws_I_COMP_2()
    {
        var registry = MakeRegistry(Fbl);
        var act = () => Competition.Create(
            CompetitionId.New(),
            CompetitionCode.From("copa-2026"),
            "Copa",
            DateRange.Create(D1, D2),
            new[] { (Bkb, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.M }) },
            registry);
        act.Should().Throw<DomainException>().WithMessage("*not registered*");
    }

    [Fact]
    public void Create_with_duplicate_discipline_throws_I_COMP_3()
    {
        var registry = MakeRegistry(Fbl);
        var act = () => Competition.Create(
            CompetitionId.New(),
            CompetitionCode.From("copa-2026"),
            "Copa",
            DateRange.Create(D1, D2),
            new[]
            {
                (Fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.M }),
                (Fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.W }),
            },
            registry);
        act.Should().Throw<DomainException>().WithMessage("*Duplicate*");
    }

    [Fact]
    public void Create_with_unsupported_gender_throws_I_COMP_4()
    {
        var registry = MakeRegistry(Fbl);
        registry.GendersByCode[Fbl] = new HashSet<GenderCode> { GenderCode.M };
        var act = () => Competition.Create(
            CompetitionId.New(),
            CompetitionCode.From("copa-2026"),
            "Copa",
            DateRange.Create(D1, D2),
            new[] { (Fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.W }) },
            registry);
        act.Should().Throw<DomainException>().WithMessage("*not supported*");
    }

    private sealed class FakeRegistry : IDisciplineRegistry
    {
        public HashSet<DisciplineCode> SupportedCodes { get; } = new();
        public Dictionary<DisciplineCode, IReadOnlySet<GenderCode>> GendersByCode { get; } = new();

        public IDisciplineModule Get(DisciplineCode code) =>
            new FakeModule(code, GendersByCode[code]);
        public bool IsRegistered(DisciplineCode code) => SupportedCodes.Contains(code);
        public IReadOnlyCollection<DisciplineCode> RegisteredCodes => SupportedCodes.ToArray();
        public IReadOnlyCollection<FunctionDescriptor> CommonFunctions => Array.Empty<FunctionDescriptor>();

        private sealed class FakeModule(DisciplineCode code, IReadOnlySet<GenderCode> genders) : IDisciplineModule
        {
            public DisciplineCode Code { get; } = code;
            public string DisplayName => "fake";
            public IReadOnlySet<GenderCode> SupportedGenders { get; } = genders;
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
}

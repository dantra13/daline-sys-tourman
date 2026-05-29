using FluentAssertions;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;

namespace Sport.Core.Tests.Competitions;

public class CompetitionInvariantCodesTests
{
    private static readonly DisciplineCode FBL = DisciplineCode.From("FBL");
    private static readonly GenderCode M = GenderCode.M;

    private static (CompetitionId id, CompetitionCode code, DateRange dates, IDisciplineRegistry registry) ValidArgs()
    {
        var registry = TestDisciplineRegistry.With(FBL, new[] { M });
        return (
            CompetitionId.New(),
            CompetitionCode.From("jud-2026"),
            DateRange.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 5)),
            registry);
    }

    [Fact]
    public void Create_with_blank_name_throws_with_code_I_COMP_5()
    {
        var (id, code, dates, registry) = ValidArgs();

        var act = () => Competition.Create(id, code, name: "  ", dates,
            new[] { (FBL, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { M }) }, registry);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-COMP-5");
    }

    [Fact]
    public void Create_with_empty_disciplines_throws_with_code_I_COMP_1()
    {
        var (id, code, dates, registry) = ValidArgs();

        var act = () => Competition.Create(id, code, "Name", dates,
            Array.Empty<(DisciplineCode, IReadOnlySet<GenderCode>)>(), registry);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-COMP-1");
    }

    [Fact]
    public void Create_with_unregistered_discipline_throws_with_code_I_COMP_2()
    {
        var (id, code, dates, _) = ValidArgs();
        var emptyRegistry = TestDisciplineRegistry.Empty();

        var act = () => Competition.Create(id, code, "Name", dates,
            new[] { (FBL, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { M }) }, emptyRegistry);

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("I-COMP-2");
    }

    [Fact]
    public void Create_with_duplicate_disciplines_throws_with_code_I_COMP_3()
    {
        var (id, code, dates, registry) = ValidArgs();

        var act = () => Competition.Create(id, code, "Name", dates,
            new[]
            {
                (FBL, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { M }),
                (FBL, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { M }),
            }, registry);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-COMP-3");
    }

    [Fact]
    public void Create_with_unsupported_gender_throws_with_code_I_COMP_4()
    {
        var (id, code, dates, registry) = ValidArgs();
        var F = GenderCode.W;

        var act = () => Competition.Create(id, code, "Name", dates,
            new[] { (FBL, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { F }) }, registry);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-COMP-4");
    }
}

using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Structure;

namespace Sport.Core.Tests.Structure;

public class RscCompositionTests
{
    private static readonly DisciplineCode Fbl = DisciplineCode.From("FBL");

    [Fact]
    public void Composes_event_level_with_filler_in_phase_and_unit()
    {
        var rsc = Rsc.Compose(
            discipline: Fbl,
            gender: GenderCode.M,
            eventType: EventTypeCode.From("TEAM11"),
            modifier: null,
            phase: null,
            unit: null,
            subunit: null);

        rsc.Value.Length.Should().Be(34);
        rsc.Value.Should().Be("FBLMTEAM11------------------------");
    }

    [Fact]
    public void Composes_phase_level()
    {
        var rsc = Rsc.Compose(
            Fbl, GenderCode.M, EventTypeCode.From("TEAM11"),
            modifier: null,
            phase: PhaseCode.From("GPA"),
            unit: null,
            subunit: null);

        rsc.Value.Should().Be("FBLMTEAM11------------GPA---------");
    }

    [Fact]
    public void Composes_unit_level_atomic()
    {
        var rsc = Rsc.Compose(
            Fbl, GenderCode.M, EventTypeCode.From("TEAM11"),
            modifier: null,
            phase: PhaseCode.From("QFNL"),
            unit: UnitCode.From("000100--"),
            subunit: null);

        rsc.Value.Should().Be("FBLMTEAM11------------QFNL000100--");
    }

    [Fact]
    public void Level_reports_correct_value_for_each_input()
    {
        Rsc.From("FBL-------------------------------").Level.Should().Be(RscLevel.Discipline);
        Rsc.From("FBLMTEAM11------------------------").Level.Should().Be(RscLevel.Event);
        Rsc.From("FBLMTEAM11------------GPA---------").Level.Should().Be(RscLevel.Phase);
        Rsc.From("FBLMTEAM11------------QFNL000100--").Level.Should().Be(RscLevel.Unit);
    }
}

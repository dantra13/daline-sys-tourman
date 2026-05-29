using FluentAssertions;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD.Tests;

public class JudTeamMatchSubunitTests
{
    private static readonly DisciplineCode Jud = DisciplineCode.From("JUD");

    private static (Event ev, IDisciplineModule module) NewMixedTeamEvent()
    {
        var registry = new DisciplineRegistry();
        registry.Register(new JudModule());
        var module = registry.Get(Jud);

        var comp = Competition.Create(
            CompetitionId.New(), CompetitionCode.From("jud-2026"), "Judo 2026",
            DateRange.Create(new DateOnly(2026, 7, 27), new DateOnly(2026, 8, 3)),
            new[] { (Jud, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.X }) },
            registry);
        var compDisc = comp.Disciplines.Single();

        var ev = Event.Create(
            EventId.New(), compDisc.Id, Jud, GenderCode.X,
            EventTypeCode.From("TEAM6"), modifier: null, name: "Mixed Team",
            disciplineModule: module);
        ev.AddPhase(PhaseCode.From("FNL"), 1, module);
        return (ev, module);
    }

    [Fact]
    public void Team_match_decomposes_into_six_weight_category_subunits()
    {
        var (ev, module) = NewMixedTeamEvent();

        var match = ev.AddTeamMatchUnit(
            PhaseCode.From("FNL"), UnitCode.From("00010000"),
            module.SubunitsFor(EventTypeCode.From("TEAM6")), module, scheduledStart: null);

        match.Subunits.Should().HaveCount(6);
        match.Code.Value.Should().EndWith("00");
        match.Subunits.Select(s => s.Rsc.Value[^2..]).Should()
            .BeEquivalentTo(new[] { "01", "02", "03", "04", "05", "06" });
        match.Rsc.Value.Should().EndWith("00");
    }

    [Fact]
    public void Individual_event_cannot_be_assembled_as_team_match()
    {
        var registry = new DisciplineRegistry();
        registry.Register(new JudModule());
        var module = registry.Get(Jud);

        var comp = Competition.Create(
            CompetitionId.New(), CompetitionCode.From("jud-ind-2026"), "Judo Individual 2026",
            DateRange.Create(new DateOnly(2026, 7, 27), new DateOnly(2026, 8, 3)),
            new[] { (Jud, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.M }) },
            registry);

        var ev = Event.Create(
            EventId.New(), comp.Disciplines.Single().Id, Jud, GenderCode.M,
            EventTypeCode.From("73KG"), modifier: null, name: "Men's 73KG",
            disciplineModule: module);
        ev.AddPhase(PhaseCode.From("FNL"), 1, module);

        var act = () => ev.AddTeamMatchUnit(
            PhaseCode.From("FNL"), UnitCode.From("00010000"),
            new[] { SubunitCode.From("01") }, module, null);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-STR-15");
    }
}

using FluentAssertions;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;
using Sport.Core.Structure;
using Sport.Core.Tests.DisciplineRegistry;

namespace Sport.Core.Tests.Structure;

public class EventGovernedAssemblyTests
{
    private static readonly IDisciplineModule Module = new SubunitHostingTestModule();

    private static Event TeamEvent()
    {
        var ev = Event.Create(
            EventId.New(), CompetitionDisciplineId.From(Guid.NewGuid()),
            DisciplineCode.From("TST"), GenderCode.X,
            EventTypeCode.From("TEAM2"), modifier: null, name: "Test Team",
            disciplineModule: Module);
        ev.AddPhase(PhaseCode.From("FNL"), 1, Module);
        return ev;
    }

    private static Event IndividualEvent()
    {
        var ev = Event.Create(
            EventId.New(), CompetitionDisciplineId.From(Guid.NewGuid()),
            DisciplineCode.From("TST"), GenderCode.W,
            EventTypeCode.From("57KG"), modifier: null, name: "Test 57KG",
            disciplineModule: Module);
        ev.AddPhase(PhaseCode.From("FNL"), 1, Module);
        return ev;
    }

    [Fact]
    public void AddTeamMatchUnit_builds_parent_with_governed_subunits()
    {
        var ev = TeamEvent();

        var match = ev.AddTeamMatchUnit(
            PhaseCode.From("FNL"), UnitCode.From("00010000"),
            new[] { SubunitCode.From("01"), SubunitCode.From("02") },
            Module, scheduledStart: null);

        match.Subunits.Should().HaveCount(2);
        match.Code.Value.Should().EndWith("00");
        match.Subunits.Select(s => s.Rsc.Value[^2..]).Should().BeEquivalentTo(new[] { "01", "02" });
    }

    [Fact]
    public void AddTeamMatchUnit_rejects_out_of_catalog_subunit()
    {
        var ev = TeamEvent();
        var act = () => ev.AddTeamMatchUnit(
            PhaseCode.From("FNL"), UnitCode.From("00010000"),
            new[] { SubunitCode.From("09") }, Module, null);
        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-STR-17");
    }

    [Fact]
    public void AddTeamMatchUnit_rejects_non_hosting_event()
    {
        var ev = IndividualEvent();
        var act = () => ev.AddTeamMatchUnit(
            PhaseCode.From("FNL"), UnitCode.From("00010000"),
            new[] { SubunitCode.From("01") }, Module, null);
        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-STR-15");
    }

    [Fact]
    public void AddAtomicUnit_builds_atomic_unit_for_individual_event()
    {
        var ev = IndividualEvent();
        var unit = ev.AddAtomicUnit(PhaseCode.From("FNL"), UnitCode.From("000100--"), Module, null);
        unit.Subunits.Should().BeEmpty();
        unit.Code.Value.Should().EndWith("--");
    }

    [Fact]
    public void AddAtomicUnit_rejects_hosting_event()
    {
        var ev = TeamEvent();
        var act = () => ev.AddAtomicUnit(PhaseCode.From("FNL"), UnitCode.From("000100--"), Module, null);
        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-STR-16");
    }

    [Fact]
    public void Assembly_rejects_unknown_phase()
    {
        var ev = IndividualEvent();
        var act = () => ev.AddAtomicUnit(PhaseCode.From("SFNL"), UnitCode.From("000100--"), Module, null);
        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-STR-18");
    }
}

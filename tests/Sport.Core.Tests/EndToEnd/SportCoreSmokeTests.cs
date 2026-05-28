using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Participants;
using Sport.Core.Structure;
using Sport.Disciplines.FBL;

namespace Sport.Core.Tests.EndToEnd;

public class SportCoreSmokeTests
{
    [Fact]
    public void Build_a_minimal_FBL_competition_end_to_end()
    {
        var services = new ServiceCollection()
            .AddSportCore()
            .AddDisciplineModule<FblModule>();
        var registry = services.BuildServiceProvider().BuildSportRegistry();

        var compId = CompetitionId.New();
        var fbl = DisciplineCode.From("FBL");
        var comp = Competition.Create(
            compId,
            CompetitionCode.From("copa-2026"),
            "Copa 2026",
            DateRange.Create(new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30)),
            new[] { (fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.M }) },
            registry);

        var compDisc = comp.Disciplines.Single();
        var fblModule = registry.Get(fbl);

        var ev = Event.Create(EventId.New(), compDisc.Id, fbl, GenderCode.M,
            EventTypeCode.From("TEAM11"), null, "Men's Football", fblModule);

        var phase = ev.AddPhase(PhaseCode.From("QFNL"), 1, fblModule);
        var unit = Unit.CreateAtomic(UnitId.New(), phase.Id, UnitCode.From("000100--"), phase.Rsc, null);
        phase.AddUnit(unit);

        unit.Rsc.Value.Should().Be("FBLMTEAM11------------QFNL000100--");

        var orgA = Organisation.Create(OrganisationId.New(), OrganisationCode.From("ESP"), "Spain", OrganisationType.Noc);
        var orgB = Organisation.Create(OrganisationId.New(), OrganisationCode.From("BRA"), "Brazil", OrganisationType.Noc);
        var teamA = Team.Create(TeamId.New(), TeamCode.From("ESP-FBL-M"), "Spain", orgA.Id, fbl);
        var teamB = Team.Create(TeamId.New(), TeamCode.From("BRA-FBL-M"), "Brazil", orgB.Id, fbl);

        var members = Enumerable.Range(1, 18)
            .Select(i => (PersonId.New(), i, (Bib?)null))
            .ToArray();

        var entryA = Entry.Create(EntryId.New(), ev.Id, EntryType.Team, orgA.Id, teamA.Id, null, null, members);
        var entryB = Entry.Create(EntryId.New(), ev.Id, EntryType.Team, orgB.Id, teamB.Id, null, null, members);

        entryA.Composition.Should().HaveCount(18);
        entryB.Composition.Should().HaveCount(18);

        var refereeDescriptor = fblModule.Functions.Single(f => f.Code.Value == "FBL.REF");
        var assignment = OfficialAssignment.Create(
            OfficialAssignmentId.New(),
            PersonId.New(),
            refereeDescriptor,
            OfficialScope.Create(ScopeLevel.Unit, unit.Id.Value),
            organisationId: null);

        assignment.Status.Should().Be(OfficialAssignmentStatus.Active);
    }
}

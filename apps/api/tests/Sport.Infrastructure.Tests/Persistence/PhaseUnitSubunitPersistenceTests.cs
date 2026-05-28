using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Structure;
using Sport.Infrastructure.Tests.Fixtures;
using Sport.Infrastructure.Tests.TestHelpers;

namespace Sport.Infrastructure.Tests.Persistence;

[Collection("Postgres")]
public sealed class PhaseUnitSubunitPersistenceTests : IClassFixture<SportDbContextFixture>
{
    private static readonly DisciplineCode Fbl = DisciplineCode.From("FBL");
    private readonly SportDbContextFixture _fixture;

    public PhaseUnitSubunitPersistenceTests(SportDbContextFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Round_trips_phase_unit_subunit_hierarchy()
    {
        var registry = MakeRegistry();
        var comp = Competition.Create(
            CompetitionId.New(),
            CompetitionCode.From("copa-2026"),
            "Copa 2026",
            DateRange.Create(new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30)),
            new[] { (Fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.M }) },
            registry);
        var compDisc = comp.Disciplines.Single();

        var ev = Event.Create(
            EventId.New(), compDisc.Id, Fbl, GenderCode.M,
            EventTypeCode.From("TEAM11"), modifier: null, name: "Men's Football",
            disciplineModule: registry.Get(Fbl));
        var phase = ev.AddPhase(PhaseCode.From("QFNL"), 1, registry.Get(Fbl));
        var parentUnit = Unit.CreateParentForSubunits(
            UnitId.New(), phase.Id, UnitCode.From("00010000"), phase.Rsc, null);
        var sub = Subunit.Create(SubunitId.New(), parentUnit.Id, SubunitCode.From("01"), parentUnit.Rsc);
        parentUnit.AddSubunit(sub);
        phase.AddUnit(parentUnit);

        await using (var write = _fixture.CreateContext())
        {
            write.Add(comp);
            write.Add(ev);
            await write.SaveChangesAsync();
        }

        await using var read = _fixture.CreateContext();
        var loaded = await read.Phases
            .Include(p => p.Units).ThenInclude(u => u.Subunits)
            .AsSplitQuery()
            .SingleAsync(p => p.Id == phase.Id);

        loaded.Units.Should().HaveCount(1);
        loaded.Units.Single().Subunits.Should().HaveCount(1);
        loaded.Units.Single().Subunits.Single().Rsc.Value.Should().EndWith("01");
    }

    private static FakeRegistry MakeRegistry()
    {
        var reg = new FakeRegistry();
        reg.SupportedCodes.Add(Fbl);
        reg.GendersByCode[Fbl] = new HashSet<GenderCode> { GenderCode.M };
        return reg;
    }
}

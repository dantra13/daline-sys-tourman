using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Sport.Core.Structure;
using Sport.Infrastructure.Tests.Fixtures;

namespace Sport.Infrastructure.Tests.Persistence;

[Collection("Postgres")]
public sealed class PhaseUnitSubunitPersistenceTests : IClassFixture<SportDbContextFixture>
{
    private readonly SportDbContextFixture _fixture;

    public PhaseUnitSubunitPersistenceTests(SportDbContextFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync().GetAwaiter().GetResult();
    }

    [Fact(Skip = "Run after InitialCreate migration is generated in Task 14")]
    public async Task Round_trips_phase_unit_subunit_hierarchy()
    {
        var phase = Phase.Create(PhaseId.New(), EventId.New(), PhaseCode.From("QFNL"), 1,
                                  Rsc.From("FBLMTEAM11------------------------"));
        var parentUnit = Unit.CreateParentForSubunits(
            UnitId.New(), phase.Id, UnitCode.From("00010000"), phase.Rsc, null);
        var sub = Subunit.Create(SubunitId.New(), parentUnit.Id, SubunitCode.From("01"), parentUnit.Rsc);
        parentUnit.AddSubunit(sub);
        phase.AddUnit(parentUnit);

        await using (var write = _fixture.CreateContext())
        {
            write.Add(phase);
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
}

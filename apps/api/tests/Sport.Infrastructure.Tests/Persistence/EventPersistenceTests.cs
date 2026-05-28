using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Structure;
using Sport.Infrastructure.Tests.Fixtures;
using Sport.Infrastructure.Tests.TestHelpers;

namespace Sport.Infrastructure.Tests.Persistence;

[Collection("Postgres")]
public sealed class EventPersistenceTests : IClassFixture<SportDbContextFixture>
{
    private static readonly DisciplineCode Fbl = DisciplineCode.From("FBL");
    private static readonly EventTypeCode Team11 = EventTypeCode.From("TEAM11");
    private readonly SportDbContextFixture _fixture;

    public EventPersistenceTests(SportDbContextFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync().GetAwaiter().GetResult();
    }

    [Fact(Skip = "Run after InitialCreate migration is generated in Task 14")]
    public async Task Round_trips_an_event_with_its_rsc()
    {
        var ev = Event.Create(
            EventId.New(), CompetitionDisciplineId.New(), Fbl, GenderCode.M,
            Team11, modifier: null, name: "Men's Football",
            disciplineModule: BuildModule());

        await using (var write = _fixture.CreateContext())
        {
            write.Add(ev);
            await write.SaveChangesAsync();
        }

        await using var read = _fixture.CreateContext();
        var loaded = await read.Events.SingleAsync(e => e.Id == ev.Id);
        loaded.Rsc.Value.Should().Be("FBLMTEAM11------------------------");
    }

    private static IDisciplineModule BuildModule()
    {
        var reg = new FakeRegistry();
        reg.SupportedCodes.Add(Fbl);
        reg.GendersByCode[Fbl] = new HashSet<GenderCode> { GenderCode.M };
        return reg.Get(Fbl);
    }
}

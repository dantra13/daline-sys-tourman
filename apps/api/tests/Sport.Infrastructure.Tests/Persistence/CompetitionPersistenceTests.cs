using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Infrastructure.Tests.Fixtures;
using Sport.Infrastructure.Tests.TestHelpers;

namespace Sport.Infrastructure.Tests.Persistence;

[Collection("Postgres")]
public sealed class CompetitionPersistenceTests : IClassFixture<SportDbContextFixture>
{
    private static readonly DisciplineCode Fbl = DisciplineCode.From("FBL");
    private readonly SportDbContextFixture _fixture;

    public CompetitionPersistenceTests(SportDbContextFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync().GetAwaiter().GetResult();
    }

    private static FakeRegistry MakeRegistry()
    {
        var reg = new FakeRegistry();
        reg.SupportedCodes.Add(Fbl);
        reg.GendersByCode[Fbl] = new HashSet<GenderCode> { GenderCode.M, GenderCode.W };
        return reg;
    }

    [Fact]
    public async Task Round_trips_a_competition_with_disciplines_and_genders_array()
    {
        var comp = Competition.Create(
            CompetitionId.New(),
            CompetitionCode.From("copa-2026"),
            "Copa 2026",
            DateRange.Create(new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30)),
            new[] { (Fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.M }) },
            MakeRegistry());

        await using (var write = _fixture.CreateContext())
        {
            write.Add(comp);
            await write.SaveChangesAsync();
        }

        await using var read = _fixture.CreateContext();
        var loaded = await read.Competitions
            .Include(c => c.Disciplines)
            .SingleAsync(c => c.Id == comp.Id);

        loaded.Code.Should().Be(comp.Code);
        loaded.Name.Should().Be("Copa 2026");
        loaded.Dates.Start.Should().Be(new DateOnly(2026, 6, 1));
        loaded.Dates.End.Should().Be(new DateOnly(2026, 6, 30));
        loaded.Disciplines.Should().HaveCount(1);
        loaded.Disciplines.Single().Code.Should().Be(Fbl);
        loaded.Disciplines.Single().EnabledGenders.Should().BeEquivalentTo(new[] { GenderCode.M });
    }

    [Fact]
    public async Task Code_unique_constraint_is_enforced_at_db_level()
    {
        var registry = MakeRegistry();
        var first = MakeCompetition("copa-2026", registry);
        var second = MakeCompetition("copa-2026", registry);

        await using (var ctx = _fixture.CreateContext())
        {
            ctx.Add(first);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = _fixture.CreateContext())
        {
            ctx.Add(second);
            var act = () => ctx.SaveChangesAsync();
            await act.Should().ThrowAsync<DbUpdateException>();
        }
    }

    private static Competition MakeCompetition(string code, FakeRegistry registry) =>
        Competition.Create(
            CompetitionId.New(),
            CompetitionCode.From(code),
            "Copa",
            DateRange.Create(new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30)),
            new[] { (Fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.M }) },
            registry);
}

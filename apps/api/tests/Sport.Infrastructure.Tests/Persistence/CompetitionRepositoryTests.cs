using FluentAssertions;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Infrastructure.Persistence;
using Sport.Infrastructure.Tests.Fixtures;

namespace Sport.Infrastructure.Tests.Persistence;

[Collection("Postgres")]
public class CompetitionRepositoryTests : IClassFixture<SportDbContextFixture>, IAsyncLifetime
{
    private readonly SportDbContextFixture _fixture;

    public CompetitionRepositoryTests(SportDbContextFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AddAsync_persists_with_disciplines()
    {
        await using var ctx = _fixture.CreateContext();
        var repo = new CompetitionRepository(ctx);
        var registry = InfraTestRegistry.WithFblM();

        var comp = Competition.Create(
            CompetitionId.From(Guid.NewGuid()),
            CompetitionCode.From("jud-2026"),
            "Judo Open 2026",
            DateRange.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 5)),
            new[]
            {
                (DisciplineCode.From("FBL"),
                    (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.M }),
            },
            registry);

        await repo.AddAsync(comp, CancellationToken.None);
        await ctx.SaveChangesAsync();

        await using var verify = _fixture.CreateContext();
        var verifyRepo = new CompetitionRepository(verify);
        var loaded = await verifyRepo.GetByIdAsync(comp.Id, CancellationToken.None);

        loaded.Should().NotBeNull();
        loaded!.Code.Value.Should().Be("jud-2026");
        loaded.Disciplines.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_when_missing()
    {
        await using var ctx = _fixture.CreateContext();
        var repo = new CompetitionRepository(ctx);

        var result = await repo.GetByIdAsync(
            CompetitionId.From(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByCodeAsync_true_when_present_false_otherwise()
    {
        await using var ctx = _fixture.CreateContext();
        var repo = new CompetitionRepository(ctx);
        var registry = InfraTestRegistry.WithFblM();
        var comp = Competition.Create(
            CompetitionId.From(Guid.NewGuid()),
            CompetitionCode.From("dup-test"),
            "T",
            DateRange.Create(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 2)),
            new[]
            {
                (DisciplineCode.From("FBL"),
                    (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.M }),
            },
            registry);
        await repo.AddAsync(comp, CancellationToken.None);
        await ctx.SaveChangesAsync();

        var present = await repo.ExistsByCodeAsync(CompetitionCode.From("dup-test"), CancellationToken.None);
        var absent  = await repo.ExistsByCodeAsync(CompetitionCode.From("missing"),   CancellationToken.None);

        present.Should().BeTrue();
        absent.Should().BeFalse();
    }

    [Fact]
    public async Task ListAsync_returns_items_ordered_by_code()
    {
        await using var ctx = _fixture.CreateContext();
        var repo = new CompetitionRepository(ctx);
        var registry = InfraTestRegistry.WithFblM();

        foreach (var code in new[] { "zzz-late", "aaa-early", "mmm-mid" })
        {
            var comp = Competition.Create(
                CompetitionId.From(Guid.NewGuid()),
                CompetitionCode.From(code),
                code,
                DateRange.Create(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 2)),
                new[]
                {
                    (DisciplineCode.From("FBL"),
                        (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.M }),
                },
                registry);
            await repo.AddAsync(comp, CancellationToken.None);
        }
        await ctx.SaveChangesAsync();

        await using var verify = _fixture.CreateContext();
        var verifyRepo = new CompetitionRepository(verify);
        var list = await verifyRepo.ListAsync(CancellationToken.None);

        list.Select(c => c.Code.Value).Should().Equal("aaa-early", "mmm-mid", "zzz-late");
    }
}

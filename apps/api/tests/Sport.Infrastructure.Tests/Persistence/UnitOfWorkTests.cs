using FluentAssertions;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Infrastructure.Persistence;
using Sport.Infrastructure.Tests.Fixtures;

namespace Sport.Infrastructure.Tests.Persistence;

[Collection("Postgres")]
public class UnitOfWorkTests : IClassFixture<SportDbContextFixture>, IAsyncLifetime
{
    private readonly SportDbContextFixture _fixture;

    public UnitOfWorkTests(SportDbContextFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SaveChangesAsync_commits_pending_changes()
    {
        await using var ctx = _fixture.CreateContext();
        var repo = new CompetitionRepository(ctx);
        var uow = new UnitOfWork(ctx);
        var registry = InfraTestRegistry.WithFblM();

        var comp = Competition.Create(
            CompetitionId.From(Guid.NewGuid()),
            CompetitionCode.From("uow-test"),
            "UoW",
            DateRange.Create(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 2)),
            new[]
            {
                (DisciplineCode.From("FBL"),
                    (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.M }),
            },
            registry);

        await repo.AddAsync(comp, CancellationToken.None);
        await uow.SaveChangesAsync(CancellationToken.None);

        await using var verify = _fixture.CreateContext();
        var verifyRepo = new CompetitionRepository(verify);
        (await verifyRepo.GetByIdAsync(comp.Id, CancellationToken.None)).Should().NotBeNull();
    }
}

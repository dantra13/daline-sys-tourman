using Sport.Application.Abstractions;
using Sport.Core.Competitions;

namespace Sport.Api.Tests.Fakes;

public sealed class InMemoryStore
{
    public List<Competition> Competitions { get; } = new();
}

public sealed class InMemoryCompetitionRepository : ICompetitionRepository
{
    private readonly InMemoryStore _store;
    public InMemoryCompetitionRepository(InMemoryStore store) => _store = store;

    public Task AddAsync(Competition competition, CancellationToken ct)
    {
        _store.Competitions.Add(competition);
        return Task.CompletedTask;
    }

    public Task<Competition?> GetByIdAsync(CompetitionId id, CancellationToken ct)
        => Task.FromResult(_store.Competitions.FirstOrDefault(c => c.Id == id));

    public Task<bool> ExistsByCodeAsync(CompetitionCode code, CancellationToken ct)
        => Task.FromResult(_store.Competitions.Any(c => c.Code == code));

    public Task<IReadOnlyList<Competition>> ListAsync(CancellationToken ct)
        => Task.FromResult<IReadOnlyList<Competition>>(
            _store.Competitions.OrderBy(c => c.Code.Value).ToList());
}

public sealed class NoopUnitOfWork : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}

using Sport.Application.Abstractions;
using Sport.Core.Competitions;

namespace Sport.Application.Tests.Fakes;

public sealed class InMemoryCompetitionRepository : ICompetitionRepository
{
    private readonly List<Competition> _items = new();

    public IReadOnlyList<Competition> Snapshot => _items;

    public Task AddAsync(Competition competition, CancellationToken ct)
    {
        _items.Add(competition);
        return Task.CompletedTask;
    }

    public Task<Competition?> GetByIdAsync(CompetitionId id, CancellationToken ct)
        => Task.FromResult(_items.FirstOrDefault(c => c.Id == id));

    public Task<bool> ExistsByCodeAsync(CompetitionCode code, CancellationToken ct)
        => Task.FromResult(_items.Any(c => c.Code == code));

    public Task<IReadOnlyList<Competition>> ListAsync(CancellationToken ct)
        => Task.FromResult<IReadOnlyList<Competition>>(_items.OrderBy(c => c.Code.Value).ToList());
}

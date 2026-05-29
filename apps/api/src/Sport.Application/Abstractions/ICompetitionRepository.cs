using Sport.Core.Competitions;

namespace Sport.Application.Abstractions;

public interface ICompetitionRepository
{
    Task AddAsync(Competition competition, CancellationToken ct);
    Task<Competition?> GetByIdAsync(CompetitionId id, CancellationToken ct);
    Task<bool> ExistsByCodeAsync(CompetitionCode code, CancellationToken ct);
    Task<IReadOnlyList<Competition>> ListAsync(CancellationToken ct);
}

using Microsoft.EntityFrameworkCore;
using Sport.Application.Abstractions;
using Sport.Core.Competitions;

namespace Sport.Infrastructure.Persistence;

public sealed class CompetitionRepository : ICompetitionRepository
{
    private readonly SportDbContext _db;

    public CompetitionRepository(SportDbContext db) => _db = db;

    public async Task AddAsync(Competition competition, CancellationToken ct)
        => await _db.Competitions.AddAsync(competition, ct);

    public Task<Competition?> GetByIdAsync(CompetitionId id, CancellationToken ct)
        => _db.Competitions
            .Include(c => c.Disciplines)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> ExistsByCodeAsync(CompetitionCode code, CancellationToken ct)
        => _db.Competitions.AsNoTracking().AnyAsync(c => c.Code == code, ct);

    public async Task<IReadOnlyList<Competition>> ListAsync(CancellationToken ct)
        => await _db.Competitions
            .AsNoTracking()
            .Include(c => c.Disciplines)
            .OrderBy(c => c.Code)
            .ToListAsync(ct);
}

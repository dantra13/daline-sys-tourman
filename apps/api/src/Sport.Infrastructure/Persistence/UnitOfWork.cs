using Sport.Application.Abstractions;

namespace Sport.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly SportDbContext _db;

    public UnitOfWork(SportDbContext db) => _db = db;

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}

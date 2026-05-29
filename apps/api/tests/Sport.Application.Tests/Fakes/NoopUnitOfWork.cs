using Sport.Application.Abstractions;

namespace Sport.Application.Tests.Fakes;

public sealed class NoopUnitOfWork : IUnitOfWork
{
    public int SaveCalls { get; private set; }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        SaveCalls++;
        return Task.CompletedTask;
    }
}

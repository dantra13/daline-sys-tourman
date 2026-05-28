using Testcontainers.PostgreSql;

namespace Sport.Infrastructure.Tests.Fixtures;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("sport_test")
        .WithUsername("sport")
        .WithPassword("test-password")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync() => await _container.StartAsync();
    public async Task DisposeAsync()    => await _container.DisposeAsync().AsTask();
}

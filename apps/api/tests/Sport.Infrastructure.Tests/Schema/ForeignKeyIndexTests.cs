using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Sport.Infrastructure.Tests.Fixtures;

namespace Sport.Infrastructure.Tests.Schema;

[Collection("Postgres")]
public sealed class ForeignKeyIndexTests : IClassFixture<SportDbContextFixture>
{
    private readonly SportDbContextFixture _fixture;

    public ForeignKeyIndexTests(SportDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Every_foreign_key_column_has_a_supporting_index()
    {
        await using var ctx = _fixture.CreateContext();
        var conn = ctx.Database.GetDbConnection();
        await conn.OpenAsync();

        const string sql = """
            SELECT conrelid::regclass::text AS table_name, a.attname AS column_name
              FROM pg_constraint c
              JOIN pg_attribute a ON a.attrelid = c.conrelid AND a.attnum = ANY(c.conkey)
             WHERE c.contype = 'f'
               AND NOT EXISTS (
                   SELECT 1 FROM pg_index i
                    WHERE i.indrelid = c.conrelid
                      AND a.attnum = ANY(i.indkey)
               );
            """;

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        await using var reader = await cmd.ExecuteReaderAsync();

        var missing = new List<string>();
        while (await reader.ReadAsync())
            missing.Add($"{reader.GetString(0)}.{reader.GetString(1)}");

        missing.Should().BeEmpty("every FK column should have a supporting index");
    }
}

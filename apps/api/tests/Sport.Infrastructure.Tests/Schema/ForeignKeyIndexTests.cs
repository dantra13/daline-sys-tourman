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

    [Fact]
    public async Task Logical_cross_aggregate_FK_columns_have_indexes()
    {
        // Cross-aggregate ID columns deliberately have no DB-level FK constraint (DDD).
        // We still need indexes on them for query performance.
        var expected = new (string Table, string Column)[]
        {
            ("entries",             "organisation_id"),
            ("entries",             "team_id"),
            ("official_assignments","person_id"),
        };

        await using var ctx = _fixture.CreateContext();
        var conn = ctx.Database.GetDbConnection();
        await conn.OpenAsync();

        var missing = new List<string>();
        foreach (var (table, column) in expected)
        {
            var sql = $"""
                SELECT 1
                  FROM pg_index i
                  JOIN pg_class c ON c.oid = i.indrelid
                  JOIN pg_attribute a ON a.attrelid = i.indrelid AND a.attnum = ANY(i.indkey)
                 WHERE c.relname = '{table}'
                   AND a.attname = '{column}'
                 LIMIT 1;
                """;
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            var any = await cmd.ExecuteScalarAsync();
            if (any is null) missing.Add($"{table}.{column}");
        }

        missing.Should().BeEmpty("every cross-aggregate FK column must have a supporting index");
    }
}

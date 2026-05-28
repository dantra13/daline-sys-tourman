using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Sport.Core.Participants;
using Sport.Core.Structure;
using Sport.Infrastructure.Tests.Fixtures;

namespace Sport.Infrastructure.Tests.Persistence;

[Collection("Postgres")]
public sealed class EntryPersistenceTests : IClassFixture<SportDbContextFixture>
{
    private readonly SportDbContextFixture _fixture;

    public EntryPersistenceTests(SportDbContextFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync().GetAwaiter().GetResult();
    }

    [Fact(Skip = "Run after InitialCreate migration is generated in Task 14")]
    public async Task Round_trips_entry_with_composition()
    {
        var members = Enumerable.Range(1, 3)
            .Select(i => (PersonId.New(), i, (Bib?)null))
            .ToArray();
        var entry = Entry.Create(
            EntryId.New(), EventId.New(),
            EntryType.Team, OrganisationId.New(),
            TeamId.New(), bib: null, seed: null,
            members);

        await using (var write = _fixture.CreateContext())
        {
            write.Add(entry);
            await write.SaveChangesAsync();
        }

        await using var read = _fixture.CreateContext();
        var loaded = await read.Entries
            .Include(e => e.Composition)
            .SingleAsync(e => e.Id == entry.Id);

        loaded.Status.Should().Be(EntryStatus.Registered);
        loaded.Composition.Should().HaveCount(3);
    }

    [Fact(Skip = "Run after InitialCreate migration is generated in Task 14")]
    public async Task Status_transitions_persist()
    {
        var entry = Entry.Create(
            EntryId.New(), EventId.New(), EntryType.Athlete, OrganisationId.New(),
            teamId: null, bib: null, seed: null,
            new[] { (PersonId.New(), 1, (Bib?)null) });
        entry.Disqualify();

        await using (var write = _fixture.CreateContext())
        {
            write.Add(entry);
            await write.SaveChangesAsync();
        }

        await using var read = _fixture.CreateContext();
        var loaded = await read.Entries.SingleAsync(e => e.Id == entry.Id);
        loaded.Status.Should().Be(EntryStatus.Disqualified);
    }
}

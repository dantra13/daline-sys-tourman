using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Infrastructure.Tests.Fixtures;

namespace Sport.Infrastructure.Tests.Persistence;

[Collection("Postgres")]
public sealed class PersonOrganisationTeamPersistenceTests : IClassFixture<SportDbContextFixture>
{
    private readonly SportDbContextFixture _fixture;

    public PersonOrganisationTeamPersistenceTests(SportDbContextFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Round_trips_person_organisation_team()
    {
        var org = Organisation.Create(OrganisationId.New(), OrganisationCode.From("ESP"), "Spain", OrganisationType.Noc);
        var person = Person.Create(PersonId.New(), "Pérez", "Juan", GenderCode.M, new DateOnly(1990, 1, 1), null);
        var team = Team.Create(TeamId.New(), TeamCode.From("ESP-FBL-M"), "Spain", org.Id, DisciplineCode.From("FBL"));

        await using (var write = _fixture.CreateContext())
        {
            write.Add(org);
            write.Add(person);
            write.Add(team);
            await write.SaveChangesAsync();
        }

        await using var read = _fixture.CreateContext();
        (await read.Organisations.SingleAsync(o => o.Id == org.Id)).Code.Value.Should().Be("ESP");
        (await read.Persons.SingleAsync(p => p.Id == person.Id)).FamilyName.Should().Be("Pérez");
        (await read.Teams.SingleAsync(t => t.Id == team.Id)).Code.Value.Should().Be("ESP-FBL-M");
    }
}

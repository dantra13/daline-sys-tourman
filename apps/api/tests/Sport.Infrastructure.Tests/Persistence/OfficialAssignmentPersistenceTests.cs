using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Participants;
using Sport.Infrastructure.Tests.Fixtures;

namespace Sport.Infrastructure.Tests.Persistence;

[Collection("Postgres")]
public sealed class OfficialAssignmentPersistenceTests : IClassFixture<SportDbContextFixture>
{
    private readonly SportDbContextFixture _fixture;

    public OfficialAssignmentPersistenceTests(SportDbContextFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Round_trips_official_assignment_with_owned_scope()
    {
        var descriptor = new FunctionDescriptor(
            FunctionCode.From("FBL.REF"), "Referee",
            new HashSet<ScopeLevel> { ScopeLevel.Unit },
            IsTeamOfficial: false, RequiresOrganisation: false);

        var assignment = OfficialAssignment.Create(
            OfficialAssignmentId.New(), PersonId.New(), descriptor,
            OfficialScope.Create(ScopeLevel.Unit, Guid.NewGuid()),
            organisationId: null);

        await using (var write = _fixture.CreateContext())
        {
            write.Add(assignment);
            await write.SaveChangesAsync();
        }

        await using var read = _fixture.CreateContext();
        var loaded = await read.OfficialAssignments.SingleAsync(a => a.Id == assignment.Id);
        loaded.FunctionCode.Value.Should().Be("FBL.REF");
        loaded.Scope.Level.Should().Be(ScopeLevel.Unit);
        loaded.Status.Should().Be(OfficialAssignmentStatus.Active);
    }
}

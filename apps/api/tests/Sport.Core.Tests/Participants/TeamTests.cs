using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;

namespace Sport.Core.Tests.Participants;

public class TeamTests
{
    [Fact]
    public void Create_with_valid_data_succeeds()
    {
        var t = Team.Create(TeamId.New(), TeamCode.From("FCB-FBL-M"), "Barcelona FBL M", OrganisationId.New(), DisciplineCode.From("FBL"));
        t.Code.Value.Should().Be("FCB-FBL-M");
    }
}

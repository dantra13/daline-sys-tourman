using FluentAssertions;
using Sport.Core.Participants;
using Sport.Core.Shared;

namespace Sport.Core.Tests.Participants;

public class OrganisationTests
{
    [Fact]
    public void Create_with_valid_data_succeeds()
    {
        var o = Organisation.Create(OrganisationId.New(), OrganisationCode.From("ESP"), "Spain", OrganisationType.Noc);
        o.Code.Value.Should().Be("ESP");
    }

    [Fact]
    public void Create_with_empty_name_throws()
    {
        var act = () => Organisation.Create(OrganisationId.New(), OrganisationCode.From("ESP"), " ", OrganisationType.Noc);
        act.Should().Throw<DomainException>();
    }
}

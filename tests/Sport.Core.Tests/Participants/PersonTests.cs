using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Shared;

namespace Sport.Core.Tests.Participants;

public class PersonTests
{
    [Fact]
    public void Create_with_family_name_succeeds()
    {
        var p = Person.Create(PersonId.New(), "Pérez", "Juan", GenderCode.M, new DateOnly(1990, 1, 1), ifId: null);
        p.FamilyName.Should().Be("Pérez");
    }

    [Fact]
    public void Create_without_family_name_throws()
    {
        var act = () => Person.Create(PersonId.New(), " ", null, GenderCode.M, null, null);
        act.Should().Throw<DomainException>();
    }
}

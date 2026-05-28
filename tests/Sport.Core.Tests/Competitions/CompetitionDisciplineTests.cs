using FluentAssertions;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;

namespace Sport.Core.Tests.Competitions;

public class CompetitionDisciplineTests
{
    private static readonly DisciplineCode Fbl = DisciplineCode.From("FBL");

    [Fact]
    public void Create_with_valid_genders_succeeds()
    {
        var cd = CompetitionDiscipline.Create(
            CompetitionDisciplineId.New(),
            CompetitionId.New(),
            Fbl,
            new HashSet<GenderCode> { GenderCode.M });

        cd.Code.Should().Be(Fbl);
        cd.EnabledGenders.Should().ContainSingle().Which.Should().Be(GenderCode.M);
    }

    [Fact]
    public void Create_with_empty_enabled_genders_throws()
    {
        var act = () => CompetitionDiscipline.Create(
            CompetitionDisciplineId.New(),
            CompetitionId.New(),
            Fbl,
            new HashSet<GenderCode>());
        act.Should().Throw<DomainException>().WithMessage("*at least one gender*");
    }
}

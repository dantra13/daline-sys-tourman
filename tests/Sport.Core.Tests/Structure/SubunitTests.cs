using FluentAssertions;
using Sport.Core.Structure;

namespace Sport.Core.Tests.Structure;

public class SubunitTests
{
    [Fact]
    public void Create_sets_rsc_with_subunit_chars()
    {
        var parentUnitRsc = Rsc.From("FBLMTEAM11------------QFNL00010000");
        var subunit = Subunit.Create(SubunitId.New(), UnitId.New(), SubunitCode.From("01"), parentUnitRsc);

        subunit.Rsc.Value.Substring(32, 2).Should().Be("01");
    }
}

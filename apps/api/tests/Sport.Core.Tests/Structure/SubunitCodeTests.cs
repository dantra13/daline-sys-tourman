using FluentAssertions;
using Sport.Core.Structure;
using Vogen;

namespace Sport.Core.Tests.Structure;

public class SubunitCodeTests
{
    [Fact]
    public void Rejects_00_parent_marker()
    {
        var act = () => SubunitCode.From("00");
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void Accepts_normal_contest_code()
    {
        SubunitCode.From("01").Value.Should().Be("01");
    }
}

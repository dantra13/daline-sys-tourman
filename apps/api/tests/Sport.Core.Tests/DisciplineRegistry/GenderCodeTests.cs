using FluentAssertions;
using Sport.Core.DisciplineRegistry;

namespace Sport.Core.Tests.DisciplineRegistry;

public class GenderCodeTests
{
    [Theory]
    [InlineData(GenderCode.M, 'M')]
    [InlineData(GenderCode.W, 'W')]
    [InlineData(GenderCode.X, 'X')]
    [InlineData(GenderCode.O, 'O')]
    public void Each_value_maps_to_one_rsc_character(GenderCode value, char rscChar)
    {
        value.ToRscChar().Should().Be(rscChar);
    }
}

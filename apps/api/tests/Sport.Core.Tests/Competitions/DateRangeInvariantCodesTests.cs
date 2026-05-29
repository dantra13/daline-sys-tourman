using FluentAssertions;
using Sport.Core.Competitions;
using Sport.Core.Shared;

namespace Sport.Core.Tests.Competitions;

public class DateRangeInvariantCodesTests
{
    [Fact]
    public void Create_with_end_before_start_throws_with_code_I_DR_1()
    {
        var act = () => DateRange.Create(new DateOnly(2026, 1, 5), new DateOnly(2026, 1, 1));

        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-DR-1");
    }
}

using FluentAssertions;
using Sport.Core.Competitions;
using Sport.Core.Shared;

namespace Sport.Core.Tests.Competitions;

public class DateRangeTests
{
    [Fact]
    public void Start_equal_to_end_is_a_single_day_range()
    {
        var d = new DateOnly(2026, 6, 1);
        var range = DateRange.Create(d, d);
        range.Days.Should().Be(1);
    }

    [Fact]
    public void Start_after_end_throws()
    {
        var act = () => DateRange.Create(new DateOnly(2026, 6, 2), new DateOnly(2026, 6, 1));
        act.Should().Throw<DomainException>();
    }
}

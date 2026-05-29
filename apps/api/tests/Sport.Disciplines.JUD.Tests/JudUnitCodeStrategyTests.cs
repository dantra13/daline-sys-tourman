using FluentAssertions;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD.Tests;

public class JudUnitCodeStrategyTests
{
    private readonly JudUnitCodeStrategy _strategy = new();

    [Fact]
    public void First_atomic_code_is_000100_filler()
        => _strategy.NextUnitCode(Array.Empty<UnitCode>()).Value.Should().Be("000100--");

    [Fact]
    public void Next_atomic_code_increments_body()
        => _strategy.NextUnitCode(new[] { UnitCode.From("000100--") }).Value.Should().Be("000200--");
}

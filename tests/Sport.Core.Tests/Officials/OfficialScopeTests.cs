using FluentAssertions;
using Sport.Core.Officials;
using Sport.Core.Shared;

namespace Sport.Core.Tests.Officials;

public class OfficialScopeTests
{
    [Fact]
    public void Create_with_valid_level_and_target_succeeds()
    {
        var scope = OfficialScope.Create(ScopeLevel.Unit, Guid.NewGuid());
        scope.Level.Should().Be(ScopeLevel.Unit);
    }

    [Fact]
    public void Create_with_empty_target_throws()
    {
        var act = () => OfficialScope.Create(ScopeLevel.Unit, Guid.Empty);
        act.Should().Throw<DomainException>();
    }
}

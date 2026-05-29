using FluentAssertions;
using Sport.Application;

namespace Sport.Application.Tests;

public class Smoke
{
    [Fact]
    public void AssemblyMarker_is_resolvable()
    {
        typeof(AssemblyMarker).Assembly.GetName().Name.Should().Be("Sport.Application");
    }
}

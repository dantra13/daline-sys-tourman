using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Structure;

namespace Sport.Core.Tests.DisciplineRegistry;

public class EventTypeDescriptorTests
{
    [Fact]
    public void Defaults_to_no_subunit_hosting()
    {
        var d = new EventTypeDescriptor(
            EventTypeCode.From("57KG"), "Women's 57KG",
            new HashSet<GenderCode> { GenderCode.W }, ModifierContract.Forbidden);

        d.HostsSubunits.Should().BeFalse();
        d.CanonicalSubunits.Should().BeEmpty();
    }

    [Fact]
    public void Can_declare_subunit_hosting()
    {
        var d = new EventTypeDescriptor(
            EventTypeCode.From("TEAM6"), "Mixed Team",
            new HashSet<GenderCode> { GenderCode.X }, ModifierContract.Forbidden)
        {
            HostsSubunits = true,
            CanonicalSubunits = new[] { SubunitCode.From("01"), SubunitCode.From("02") },
        };

        d.HostsSubunits.Should().BeTrue();
        d.CanonicalSubunits.Should().HaveCount(2);
    }
}

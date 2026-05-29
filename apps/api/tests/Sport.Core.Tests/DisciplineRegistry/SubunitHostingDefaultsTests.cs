using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Structure;

namespace Sport.Core.Tests.DisciplineRegistry;

public class SubunitHostingDefaultsTests
{
    [Fact]
    public void Default_members_read_subunit_structure_from_event_types()
    {
        // Default interface members are only visible through the interface type.
        IDisciplineModule m = new SubunitHostingTestModule();

        m.HostsSubunits(EventTypeCode.From("TEAM2")).Should().BeTrue();
        m.HostsSubunits(EventTypeCode.From("57KG")).Should().BeFalse();
        m.HostsSubunits(EventTypeCode.From("ZZZ")).Should().BeFalse();

        m.SubunitsFor(EventTypeCode.From("TEAM2")).Should().HaveCount(2);
        m.SubunitsFor(EventTypeCode.From("57KG")).Should().BeEmpty();

        m.ValidateSubunitCode(EventTypeCode.From("TEAM2"), SubunitCode.From("01")).IsSuccess.Should().BeTrue();
        m.ValidateSubunitCode(EventTypeCode.From("TEAM2"), SubunitCode.From("09")).IsSuccess.Should().BeFalse();
    }
}

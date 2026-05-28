using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Structure;

namespace Sport.Disciplines.VBV.Tests;

public class VbvModuleTests
{
    [Fact]
    public void Module_advertises_VBV_code_and_supported_genders()
    {
        var m = new VbvModule();
        m.Code.Value.Should().Be("VBV");
        m.SupportedGenders.Should().Contain(new[] { GenderCode.M, GenderCode.W });
    }

    [Fact]
    public void Rejects_unsupported_event_type()
    {
        new VbvModule().ValidateEventType(EventTypeCode.From("HJ"), null).IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Validates_pair_composition()
    {
        var m = new VbvModule();
        m.ValidateEntry(new EntryCandidate(EntryType.Team, 2, true, true)).IsSuccess.Should().BeTrue();
        m.ValidateEntry(new EntryCandidate(EntryType.Team, 3, true, true)).IsSuccess.Should().BeFalse();
    }
}

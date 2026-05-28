using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Structure;

namespace Sport.Disciplines.BKB.Tests;

public class BkbModuleTests
{
    [Fact]
    public void Module_advertises_BKB_code_and_supported_genders()
    {
        var m = new BkbModule();
        m.Code.Value.Should().Be("BKB");
        m.SupportedGenders.Should().Contain(new[] { GenderCode.M, GenderCode.W });
    }

    [Fact]
    public void Rejects_unsupported_event_type()
    {
        var m = new BkbModule();
        m.ValidateEventType(EventTypeCode.From("HJ"), null).IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Validates_team_entry_size()
    {
        var m = new BkbModule();
        m.ValidateEntry(new EntryCandidate(EntryType.Team, 8, true, true)).IsSuccess.Should().BeTrue();
        m.ValidateEntry(new EntryCandidate(EntryType.Team, 3, true, true)).IsSuccess.Should().BeFalse();
        m.ValidateEntry(new EntryCandidate(EntryType.Team, 13, true, true)).IsSuccess.Should().BeFalse();
    }
}

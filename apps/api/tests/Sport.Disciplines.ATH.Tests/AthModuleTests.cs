using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Structure;

namespace Sport.Disciplines.ATH.Tests;

public class AthModuleTests
{
    [Fact]
    public void Module_advertises_ATH_code_and_supported_genders()
    {
        var m = new AthModule();
        m.Code.Value.Should().Be("ATH");
        m.SupportedGenders.Should().Contain(new[] { GenderCode.M, GenderCode.W });
    }

    [Fact]
    public void Rejects_unsupported_event_type()
    {
        new AthModule().ValidateEventType(EventTypeCode.From("TEAM11"), null).IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Accepts_high_jump_event_type()
    {
        new AthModule().ValidateEventType(EventTypeCode.From("HJ"), null).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validates_athlete_composition()
    {
        var m = new AthModule();
        m.ValidateEntry(new EntryCandidate(EntryType.Athlete, 1, false, true)).IsSuccess.Should().BeTrue();
    }
}

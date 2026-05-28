using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Structure;

namespace Sport.Disciplines.BOX.Tests;

public class BoxModuleTests
{
    [Fact]
    public void Module_advertises_BOX_code_and_supported_genders()
    {
        var m = new BoxModule();
        m.Code.Value.Should().Be("BOX");
        m.SupportedGenders.Should().Contain(new[] { GenderCode.M, GenderCode.W });
    }

    [Fact]
    public void Rejects_unsupported_event_type()
    {
        new BoxModule().ValidateEventType(EventTypeCode.From("HJ"), null).IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Accepts_weight_category_event_type()
    {
        new BoxModule().ValidateEventType(EventTypeCode.From("75KG"), null).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validates_athlete_composition()
    {
        var m = new BoxModule();
        m.ValidateEntry(new EntryCandidate(EntryType.Athlete, 1, false, true)).IsSuccess.Should().BeTrue();
        m.ValidateEntry(new EntryCandidate(EntryType.Team, 1, true, true)).IsSuccess.Should().BeFalse();
    }
}

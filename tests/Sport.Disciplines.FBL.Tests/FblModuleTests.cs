using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Structure;

namespace Sport.Disciplines.FBL.Tests;

public class FblModuleTests
{
    [Fact]
    public void Module_advertises_FBL_code_and_supported_genders()
    {
        var m = new FblModule();
        m.Code.Value.Should().Be("FBL");
        m.SupportedGenders.Should().Contain(new[] { GenderCode.M, GenderCode.W });
    }

    [Fact]
    public void Rejects_unsupported_event_type()
    {
        var m = new FblModule();
        m.ValidateEventType(EventTypeCode.From("HJ"), null).IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Validates_team_entry_size()
    {
        var m = new FblModule();
        var ok = m.ValidateEntry(new EntryCandidate(EntryType.Team, CompositionSize: 18, HasTeam: true, HasOrganisation: true));
        ok.IsSuccess.Should().BeTrue();

        var bad = m.ValidateEntry(new EntryCandidate(EntryType.Team, CompositionSize: 5, HasTeam: true, HasOrganisation: true));
        bad.IsSuccess.Should().BeFalse();
    }
}

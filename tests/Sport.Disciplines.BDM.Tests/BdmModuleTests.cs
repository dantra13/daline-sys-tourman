using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Structure;

namespace Sport.Disciplines.BDM.Tests;

public class BdmModuleTests
{
    [Fact]
    public void Module_advertises_BDM_code_and_supported_genders()
    {
        var m = new BdmModule();
        m.Code.Value.Should().Be("BDM");
        m.SupportedGenders.Should().Contain(new[] { GenderCode.M, GenderCode.W, GenderCode.X });
    }

    [Fact]
    public void Rejects_unsupported_event_type()
    {
        var m = new BdmModule();
        m.ValidateEventType(EventTypeCode.From("HJ"), null).IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Validates_singles_and_doubles_composition_sizes()
    {
        var m = new BdmModule();
        m.ValidateEntry(new EntryCandidate(EntryType.Athlete, 1, false, true)).IsSuccess.Should().BeTrue();
        m.ValidateEntry(new EntryCandidate(EntryType.Athlete, 2, false, true)).IsSuccess.Should().BeFalse();
        m.ValidateEntry(new EntryCandidate(EntryType.Team, 2, true, true)).IsSuccess.Should().BeTrue();
        m.ValidateEntry(new EntryCandidate(EntryType.Team, 3, true, true)).IsSuccess.Should().BeFalse();
    }
}

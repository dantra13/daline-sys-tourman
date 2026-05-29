using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD.Tests;

public class JudModuleTests
{
    private readonly JudModule _module = new();

    [Fact]
    public void Advertises_JUD_code_and_three_genders()
    {
        _module.Code.Value.Should().Be("JUD");
        _module.SupportedGenders.Should().Contain(new[] { GenderCode.M, GenderCode.W, GenderCode.X });
    }

    [Fact]
    public void Accepts_men_women_and_team_event_types()
    {
        _module.ValidateEventType(EventTypeCode.From("73KG"), null).IsSuccess.Should().BeTrue();
        _module.ValidateEventType(EventTypeCode.From("O100KG"), null).IsSuccess.Should().BeTrue();
        _module.ValidateEventType(EventTypeCode.From("57KG"), null).IsSuccess.Should().BeTrue();
        _module.ValidateEventType(EventTypeCode.From("TEAM6"), null).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Rejects_unknown_event_type_and_modifier()
    {
        _module.ValidateEventType(EventTypeCode.From("99KG"), null).IsSuccess.Should().BeFalse();
        _module.ValidateEventType(EventTypeCode.From("73KG"), EventModifierCode.From("X")).IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Team_event_hosts_six_weight_category_subunits()
    {
        IDisciplineModule m = _module;
        m.HostsSubunits(EventTypeCode.From("TEAM6")).Should().BeTrue();
        m.SubunitsFor(EventTypeCode.From("TEAM6")).Should().HaveCount(6);
        m.ValidateSubunitCode(EventTypeCode.From("TEAM6"), SubunitCode.From("06")).IsSuccess.Should().BeTrue();
        m.ValidateSubunitCode(EventTypeCode.From("TEAM6"), SubunitCode.From("07")).IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Individual_event_hosts_no_subunits()
        => ((IDisciplineModule)_module).HostsSubunits(EventTypeCode.From("73KG")).Should().BeFalse();
}

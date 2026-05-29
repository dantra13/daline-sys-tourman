using FluentAssertions;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD.Tests;

public class JudPhaseCatalogTests
{
    private readonly JudPhaseCatalog _catalog = new();

    [Fact]
    public void Individual_event_allows_repechage()
    {
        _catalog.IsAllowedForEventType(EventTypeCode.From("73KG"), PhaseCode.From("REP1")).Should().BeTrue();
        _catalog.IsAllowedForEventType(EventTypeCode.From("73KG"), PhaseCode.From("FNL")).Should().BeTrue();
    }

    [Fact]
    public void Team_event_excludes_repechage()
    {
        _catalog.IsAllowedForEventType(EventTypeCode.From("TEAM6"), PhaseCode.From("REP1")).Should().BeFalse();
        _catalog.IsAllowedForEventType(EventTypeCode.From("TEAM6"), PhaseCode.From("FNL")).Should().BeTrue();
    }

    [Fact]
    public void Unknown_phase_rejected()
        => _catalog.IsAllowedForEventType(EventTypeCode.From("73KG"), PhaseCode.From("R8")).Should().BeFalse();
}

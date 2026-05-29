using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;

namespace Sport.Disciplines.JUD.Tests;

public class JudEntryRulesTests
{
    private readonly JudEntryRules _rules = new();

    [Fact]
    public void Accepts_single_athlete()
        => _rules.Validate(new EntryCandidate(EntryType.Athlete, 1, false, true)).IsSuccess.Should().BeTrue();

    [Fact]
    public void Rejects_athlete_pair()
        => _rules.Validate(new EntryCandidate(EntryType.Athlete, 2, false, true)).IsSuccess.Should().BeFalse();

    [Fact]
    public void Accepts_team_of_six()
        => _rules.Validate(new EntryCandidate(EntryType.Team, 6, true, true)).IsSuccess.Should().BeTrue();

    [Fact]
    public void Rejects_team_of_five()
        => _rules.Validate(new EntryCandidate(EntryType.Team, 5, true, true)).IsSuccess.Should().BeFalse();

    [Fact]
    public void Rejects_group_entry()
        => _rules.Validate(new EntryCandidate(EntryType.Group, 6, true, true)).IsSuccess.Should().BeFalse();
}

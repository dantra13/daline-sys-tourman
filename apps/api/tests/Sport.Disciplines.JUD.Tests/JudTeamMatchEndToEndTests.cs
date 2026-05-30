using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Results;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD.Tests;

public class JudTeamMatchEndToEndTests
{
    private static readonly EventTypeCode Team6 = EventTypeCode.From("TEAM6");
    private static readonly DisciplineCode Jud = DisciplineCode.From("JUD");

    [Fact]
    public void Six_contests_decompose_into_a_stored_parent_aggregate_consumed_by_progression()
    {
        var schema = new JudResultSchema();
        var parentUnitId = UnitId.New();          // contests hang off the SAME unit id
        var homeTeam = EntryId.New(); var awayTeam = EntryId.New();

        var parent = UnitResultDocument.CreateForUnit(
            UnitResultId.New(), parentUnitId, Rsc.From("JUDXTEAM6-------------FNL-00010000"), Jud, Team6);
        parent.ApplySnapshot(
            new[] { new CompetitorResult(homeTeam, 1), new CompetitorResult(awayTeam, 2) },
            Array.Empty<ResultSegment>(), Array.Empty<ResultExtension>(), schema);

        var outcomes = new[] { Wlt.W, Wlt.W, Wlt.W, Wlt.W, Wlt.L, Wlt.L };   // home 4, away 2
        var contests = new List<UnitResultDocument>();
        for (var i = 0; i < 6; i++)
        {
            var rsc = Rsc.From($"JUDXTEAM6-------------FNL-0001000{i + 1}");
            var c = UnitResultDocument.CreateForSubunit(UnitResultId.New(), parentUnitId, SubunitId.New(), rsc, Jud, Team6);
            c.ApplySnapshot(
                new[]
                {
                    new CompetitorResult(EntryId.New(), 1) { Wlt = outcomes[i], Composition = new[] { new CompetitorMemberResult(PersonId.New(), 1) } },
                    new CompetitorResult(EntryId.New(), 2) { Wlt = outcomes[i] == Wlt.W ? Wlt.L : Wlt.W, Composition = new[] { new CompetitorMemberResult(PersonId.New(), 1) } },
                },
                Array.Empty<ResultSegment>(),
                new[] { new ResultExtension(ExtensionType.From("UI"), ExtensionCode.From("WEIGHT_CATEGORY")) { Value = $"JUDWCAT{i + 1}" } },
                schema);
            c.TransitionTo(ResultStatus.Official, schema);
            contests.Add(c);
        }

        // Each contest is a real subunit of the parent unit.
        contests.Should().OnlyContain(c => c.TargetSubunitId != null && c.TargetUnitId == parentUnitId);

        parent.ApplyRollup(schema.AggregateSubunits(parent, contests), schema);

        parent.Status.Should().Be(ResultStatus.Official);
        parent.Competitors.Single(c => c.EntryId == homeTeam).ResultValue.Should().Be("4");
        parent.Competitors.Single(c => c.EntryId == awayTeam).ResultValue.Should().Be("2");
        parent.Extensions.Count(e => e.Code == ExtensionCode.From("COMP")).Should().Be(6);

        // What a progression layer reads: ONLY the parent outcome, never the contests.
        parent.Competitors.Single(c => c.Wlt == Wlt.W).EntryId.Should().Be(homeTeam);
    }
}

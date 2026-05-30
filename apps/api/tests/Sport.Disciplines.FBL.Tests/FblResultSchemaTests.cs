using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Results;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.FBL.Tests;

public class FblResultSchemaTests
{
    private static readonly EventTypeCode Team11 = EventTypeCode.From("TEAM11");
    private static readonly Rsc UnitRsc = Rsc.From("FBLMTEAM11------------FNL-000100--");

    private static UnitResultDocument Match(FblResultSchema schema, out EntryId home, out EntryId away)
    {
        home = EntryId.New(); away = EntryId.New();
        var doc = UnitResultDocument.CreateForUnit(UnitResultId.New(), UnitId.New(), UnitRsc, DisciplineCode.From("FBL"), Team11);
        doc.ApplySnapshot(
            new[]
            {
                new CompetitorResult(home, 1) { Wlt = Wlt.W, ResultValue = "2", ResultType = ResultTypeCode.From("POINTS") },
                new CompetitorResult(away, 2) { Wlt = Wlt.L, ResultValue = "1", ResultType = ResultTypeCode.From("POINTS") },
            },
            new[]
            {
                new ResultSegment(SegmentCode.From("H1"), 1) { Scores = new[] { new SegmentScore(home) { CumulativeValue = "1", SegmentValue = "1" }, new SegmentScore(away) { CumulativeValue = "0", SegmentValue = "0" } } },
                new ResultSegment(SegmentCode.From("H2"), 2) { Scores = new[] { new SegmentScore(home) { CumulativeValue = "2", SegmentValue = "1" }, new SegmentScore(away) { CumulativeValue = "1", SegmentValue = "1" } } },
            },
            Array.Empty<ResultExtension>(), schema);
        return doc;
    }

    [Fact]
    public void Schema_is_head_to_head_and_validates_a_two_row_match_with_periods()
    {
        var schema = new FblResultSchema();
        var doc = Match(schema, out _, out _);
        schema.OutcomeModeFor(Team11).Should().Be(OutcomeMode.HeadToHead);
        doc.Segments.Should().HaveCount(2);
    }

    [Fact]
    public void Schema_rejects_ranked_outcome_with_a_rank_field()
    {
        var schema = new FblResultSchema();
        var doc = UnitResultDocument.CreateForUnit(UnitResultId.New(), UnitId.New(), UnitRsc, DisciplineCode.From("FBL"), Team11);
        FluentActions.Invoking(() => doc.ApplySnapshot(
                new[] { new CompetitorResult(EntryId.New(), 1) { Rank = 1 } },
                Array.Empty<ResultSegment>(), Array.Empty<ResultExtension>(), schema))
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-8");
    }

    [Fact]
    public void Projection_exposes_team_scores_and_per_period_scores()
    {
        var schema = new FblResultSchema();
        var doc = Match(schema, out var home, out _);
        var proj = (FootballMatchResult)schema.Project(doc);
        proj.TeamScores[home].Should().Be("2");
        proj.Periods.Select(p => p.Code).Should().ContainInOrder("H1", "H2");
        var h2 = proj.Periods.Single(p => p.Code == "H2");
        h2.Scores.Single(s => s.EntryId == home).Cumulative.Should().Be("2");
        h2.Scores.Single(s => s.EntryId == home).Segment.Should().Be("1");
    }
}

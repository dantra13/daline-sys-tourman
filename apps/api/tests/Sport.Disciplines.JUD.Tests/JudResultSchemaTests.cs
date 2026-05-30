using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Results;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD.Tests;

public class JudResultSchemaTests
{
    private static readonly EventTypeCode Team6 = EventTypeCode.From("TEAM6");
    private static readonly DisciplineCode Jud = DisciplineCode.From("JUD");

    private static UnitResultDocument Contest(UnitId parentUnitId, int pos, Wlt homeOutcome)
    {
        var rsc = Rsc.From($"JUDXTEAM6-------------FNL-0001000{pos}");
        var doc = UnitResultDocument.CreateForSubunit(UnitResultId.New(), parentUnitId, SubunitId.New(), rsc, Jud, Team6);
        doc.ApplySnapshot(
            new[]
            {
                new CompetitorResult(EntryId.New(), 1) { Wlt = homeOutcome, Composition = new[] { new CompetitorMemberResult(PersonId.New(), 1) } },
                new CompetitorResult(EntryId.New(), 2) { Wlt = homeOutcome == Wlt.W ? Wlt.L : Wlt.W, Composition = new[] { new CompetitorMemberResult(PersonId.New(), 1) } },
            },
            Array.Empty<ResultSegment>(),
            new[] { new ResultExtension(ExtensionType.From("UI"), ExtensionCode.From("WEIGHT_CATEGORY")) { Value = $"JUDWCAT{pos}" } },
            new JudResultSchema());
        doc.TransitionTo(ResultStatus.Official, new JudResultSchema());
        return doc;
    }

    private static UnitResultDocument Parent(UnitId unitId, out EntryId home, out EntryId away)
    {
        home = EntryId.New(); away = EntryId.New();
        var parent = UnitResultDocument.CreateForUnit(UnitResultId.New(), unitId, Rsc.From("JUDXTEAM6-------------FNL-00010000"), Jud, Team6);
        parent.ApplySnapshot(
            new[] { new CompetitorResult(home, 1), new CompetitorResult(away, 2) },
            Array.Empty<ResultSegment>(), Array.Empty<ResultExtension>(), new JudResultSchema());
        return parent;
    }

    [Fact]
    public void Irm_set_is_judo_specific()
    {
        var schema = new JudResultSchema();
        schema.IrmCodesFor(Team6).Should().Contain(Irm.From("DQB"));
        schema.IrmCodesFor(Team6).Should().NotContain(Irm.From("DNF"));
    }

    [Fact]
    public void Aggregate_counts_wins_and_emits_team_comp_with_children()
    {
        var schema = new JudResultSchema();
        var unitId = UnitId.New();
        var parent = Parent(unitId, out var home, out var away);
        var contests = new[]
        {
            Contest(unitId, 1, Wlt.W), Contest(unitId, 2, Wlt.W),
            Contest(unitId, 3, Wlt.L), Contest(unitId, 4, Wlt.W),
        };

        var rollup = schema.AggregateSubunits(parent, contests);

        rollup.Competitors.Single(c => c.EntryId == home).ResultValue.Should().Be("3");
        rollup.Competitors.Single(c => c.EntryId == home).Wlt.Should().Be(Wlt.W);
        rollup.Competitors.Single(c => c.EntryId == away).ResultValue.Should().Be("1");
        rollup.Extensions.Should().HaveCount(4);
        rollup.Extensions[0].Code.Should().Be(ExtensionCode.From("COMP"));
        rollup.Extensions[0].Children.Select(ch => ch.Code).Should()
            .Contain(new[] { ExtensionCode.From("WEIGHT_CATEGORY"), ExtensionCode.From("HOME"), ExtensionCode.From("AWAY") });
        rollup.SuggestedStatus.Should().Be(ResultStatus.Official);
    }

    [Fact]
    public void Aggregate_does_not_fabricate_a_tie_outcome()
    {
        var schema = new JudResultSchema();
        var unitId = UnitId.New();
        var parent = Parent(unitId, out var home, out _);
        var contests = new[]
        {
            Contest(unitId, 1, Wlt.W), Contest(unitId, 2, Wlt.W),
            Contest(unitId, 3, Wlt.L), Contest(unitId, 4, Wlt.L),
        };

        var rollup = schema.AggregateSubunits(parent, contests);

        rollup.Competitors.Single(c => c.EntryId == home).Wlt.Should().BeNull();
        rollup.SuggestedStatus.Should().BeNull();   // unresolved, awaiting golden-score contest
    }

    [Fact]
    public void Aggregate_rejects_a_contest_that_is_not_a_subunit_of_the_parent()
    {
        var schema = new JudResultSchema();
        var parent = Parent(UnitId.New(), out _, out _);
        var foreign = Contest(UnitId.New(), 1, Wlt.W);   // belongs to a different unit
        FluentActions.Invoking(() => schema.AggregateSubunits(parent, new[] { foreign }))
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-2");
    }
}

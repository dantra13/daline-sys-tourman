using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Results;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.ATH.Tests;

public class AthResultSchemaTests
{
    private static readonly EventTypeCode Hj = EventTypeCode.From("HJ");
    private static readonly Rsc UnitRsc = Rsc.From("ATHWHJ----------------FNL-000100--");

    private static UnitResultDocument HighJump(AthResultSchema schema)
    {
        var doc = UnitResultDocument.CreateForUnit(UnitResultId.New(), UnitId.New(), UnitRsc, DisciplineCode.From("ATH"), Hj);
        doc.ApplySnapshot(
            new[]
            {
                new CompetitorResult(EntryId.New(), 1)
                {
                    Rank = 1, ResultValue = "1.92", ResultType = ResultTypeCode.From("DISTANCE"),
                    Extensions = new[]
                    {
                        new ResultExtension(ExtensionType.From("ER"), ExtensionCode.From("INTERMEDIATE")) { Pos = "1", Value = "o" },
                        new ResultExtension(ExtensionType.From("ER"), ExtensionCode.From("INTERMEDIATE")) { Pos = "2", Value = "xo" },
                    },
                },
            },
            Array.Empty<ResultSegment>(),
            new[]
            {
                new ResultExtension(ExtensionType.From("UI"), ExtensionCode.From("INTERMEDIATE")) { Pos = "1", Value = "1.88" },
                new ResultExtension(ExtensionType.From("UI"), ExtensionCode.From("INTERMEDIATE")) { Pos = "2", Value = "1.92" },
            },
            schema);
        return doc;
    }

    [Fact]
    public void Schema_is_ranked_and_validates_without_wlt()
    {
        var schema = new AthResultSchema();
        schema.OutcomeModeFor(Hj).Should().Be(OutcomeMode.Ranked);
        HighJump(schema).Competitors.Should().ContainSingle();
    }

    [Fact]
    public void Schema_rejects_a_wlt_outcome()
    {
        var schema = new AthResultSchema();
        var doc = UnitResultDocument.CreateForUnit(UnitResultId.New(), UnitId.New(), UnitRsc, DisciplineCode.From("ATH"), Hj);
        FluentActions.Invoking(() => doc.ApplySnapshot(
                new[] { new CompetitorResult(EntryId.New(), 1) { Wlt = Wlt.W } },
                Array.Empty<ResultSegment>(), Array.Empty<ResultExtension>(), schema))
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-5");
    }

    [Fact]
    public void Projection_exposes_heights_and_attempt_series()
    {
        var schema = new AthResultSchema();
        var proj = (HighJumpResult)schema.Project(HighJump(schema));
        proj.Heights.Should().ContainInOrder("1.88", "1.92");
        proj.Athletes.Should().ContainSingle();
        proj.Athletes[0].BestMark.Should().Be("1.92");
        proj.Athletes[0].Attempts.Should().ContainInOrder("o", "xo");
    }
}

using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Results;
using Sport.Core.Structure;

namespace Sport.Disciplines.BOX.Tests;

public class BoxResultSchemaTests
{
    private static readonly EventTypeCode Cat = EventTypeCode.From("75KG");
    private static readonly Rsc UnitRsc = Rsc.From("BOXM75KG--------------FNL-000100--");

    private static UnitResultDocument Bout(BoxResultSchema schema)
    {
        var red = EntryId.New(); var blue = EntryId.New();
        var doc = UnitResultDocument.CreateForUnit(UnitResultId.New(), UnitId.New(), UnitRsc, DisciplineCode.From("BOX"), Cat);
        doc.ApplySnapshot(
            new[]
            {
                new CompetitorResult(red, 1)
                {
                    Wlt = Wlt.W, ResultValue = "WP 3:0", ResultType = ResultTypeCode.From("RM_POINTS"),
                    Extensions = new[] { new ResultExtension(ExtensionType.From("ER"), ExtensionCode.From("JUDGE")) { Pos = "J1", Value = "30" } },
                },
                new CompetitorResult(blue, 2) { Wlt = Wlt.L, ResultValue = "0:3", ResultType = ResultTypeCode.From("RM_POINTS") },
            },
            Array.Empty<ResultSegment>(),
            new[] { new ResultExtension(ExtensionType.From("UI"), ExtensionCode.From("RES_CODE")) { Value = "WP" } },
            schema);
        return doc;
    }

    [Fact]
    public void Status_set_omits_Unofficial()
    {
        var schema = new BoxResultSchema();
        schema.StatusesFor(Cat).Should().NotContain(ResultStatus.Unofficial);
        schema.StatusesFor(Cat).Should().Contain(ResultStatus.Official);
    }

    [Fact]
    public void Validates_a_two_row_bout()
    {
        var schema = new BoxResultSchema();
        Bout(schema).Competitors.Should().HaveCount(2);
    }

    [Fact]
    public void Projection_exposes_judge_totals_and_decision()
    {
        var schema = new BoxResultSchema();
        var proj = (BoxingBoutResult)schema.Project(Bout(schema));
        proj.JudgeTotals.Should().ContainSingle(j => j.Judge == "J1" && j.Value == "30");
        proj.Decision.Should().Be("WP");
    }
}

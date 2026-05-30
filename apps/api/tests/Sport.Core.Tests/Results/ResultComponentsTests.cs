using FluentAssertions;
using Sport.Core.Participants;
using Sport.Core.Results;

namespace Sport.Core.Tests.Results;

public class ResultComponentsTests
{
    [Fact]
    public void ResultExtension_nests_children_for_team_comp()
    {
        var ext = new ResultExtension(ExtensionType.From("TEAM"), ExtensionCode.From("COMP"))
        {
            Pos = "1",
            Children = new[]
            {
                new ResultExtension(ExtensionType.From("TEAM"), ExtensionCode.From("WEIGHT_CATEGORY")) { Value = "JUDW57KG" },
            },
        };
        ext.Children.Should().ContainSingle().Which.Value.Should().Be("JUDW57KG");
    }

    [Fact]
    public void CompetitorMemberResult_carries_lineup_order()
    {
        var m = new CompetitorMemberResult(PersonId.New(), Order: 1) { StartSortOrder = 1 };
        m.Order.Should().Be(1);
        m.Extensions.Should().BeEmpty();
    }

    [Fact]
    public void SegmentScore_separates_cumulative_and_segment_values()
    {
        var s = new SegmentScore(EntryId.New()) { CumulativeValue = "2", SegmentValue = "1" };
        s.CumulativeValue.Should().Be("2");
        s.SegmentValue.Should().Be("1");
    }
}

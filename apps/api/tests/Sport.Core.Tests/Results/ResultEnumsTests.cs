using FluentAssertions;
using Sport.Core.Results;

namespace Sport.Core.Tests.Results;

public class ResultEnumsTests
{
    [Fact]
    public void UnitResultId_New_produces_unique_non_empty_ids()
    {
        var a = UnitResultId.New();
        a.Value.Should().NotBe(Guid.Empty);
        a.Should().NotBe(UnitResultId.New());
    }

    [Fact]
    public void ResultStatus_includes_all_nine_odf_common_codes()
    {
        Enum.GetValues<ResultStatus>().Should().HaveCount(9);
        Enum.IsDefined(ResultStatus.Partial).Should().BeTrue();
        Enum.IsDefined(ResultStatus.Protested).Should().BeTrue();
        Enum.IsDefined(ResultStatus.Provisional).Should().BeTrue();
    }

    [Fact]
    public void Wlt_and_OutcomeMode_expose_expected_members()
    {
        Enum.GetValues<Wlt>().Should().HaveCount(3);
        Enum.GetValues<OutcomeMode>().Should().BeEquivalentTo(new[] { OutcomeMode.HeadToHead, OutcomeMode.Ranked });
    }
}

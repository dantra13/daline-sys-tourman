using FluentAssertions;
using Sport.Core.Results;
using Vogen;

namespace Sport.Core.Tests.Results;

public class ResultStringVoTests
{
    [Theory]
    [InlineData("DNS")]
    [InlineData("DQB")]
    [InlineData("WDR")]
    public void Irm_accepts_uppercase_codes(string code) => Irm.From(code).Value.Should().Be(code);

    [Theory]
    [InlineData("")]
    [InlineData("dns")]
    [InlineData("TOO-LONG-IRM")]
    public void Irm_rejects_invalid_codes(string code) =>
        FluentActions.Invoking(() => Irm.From(code)).Should().Throw<ValueObjectValidationException>();

    [Fact]
    public void SegmentCode_accepts_hyphenated_odf_period_codes()
    {
        SegmentCode.From("ET-H1").Value.Should().Be("ET-H1");
        SegmentCode.From("H1").Value.Should().Be("H1");
    }

    [Fact]
    public void Vocab_codes_round_trip()
    {
        ResultTypeCode.From("RM_POINTS").Value.Should().Be("RM_POINTS");
        ExtensionType.From("ER").Value.Should().Be("ER");
        ExtensionCode.From("INTERMEDIATE").Value.Should().Be("INTERMEDIATE");
    }
}

using FluentAssertions;
using Sport.Core.Structure;
using Vogen;

namespace Sport.Core.Tests.Structure;

public class StructureValueObjectsTests
{
    [Theory]
    [InlineData("TEAM11")]
    [InlineData("HJ")]
    [InlineData("75KG")]
    public void EventTypeCode_accepts_1_to_8_uppercase_alphanumeric(string value)
        => ((Action)(() => EventTypeCode.From(value))).Should().NotThrow();

    [Theory]
    [InlineData("")]
    [InlineData("verylongcode")]
    [InlineData("team11")]
    public void EventTypeCode_rejects_invalid(string value)
        => ((Action)(() => EventTypeCode.From(value))).Should().Throw<ValueObjectValidationException>();

    [Theory]
    [InlineData("QFNL")]
    [InlineData("GPA")]
    [InlineData("FNL")]
    public void PhaseCode_accepts_1_to_4_uppercase_alphanumeric(string value)
        => ((Action)(() => PhaseCode.From(value))).Should().NotThrow();

    [Theory]
    [InlineData("QFNLZ")]
    [InlineData("qfnl")]
    public void PhaseCode_rejects_invalid(string value)
        => ((Action)(() => PhaseCode.From(value))).Should().Throw<ValueObjectValidationException>();

    [Theory]
    [InlineData("000100--")]
    [InlineData("0001SJ--")]
    [InlineData("00010000")]
    public void UnitCode_accepts_exactly_8_chars_in_allowed_charset(string value)
        => ((Action)(() => UnitCode.From(value))).Should().NotThrow();

    [Theory]
    [InlineData("000100-")]
    [InlineData("000100---")]
    [InlineData("00010o--")]
    public void UnitCode_rejects_invalid(string value)
        => ((Action)(() => UnitCode.From(value))).Should().Throw<ValueObjectValidationException>();

    [Theory]
    [InlineData("01")]
    [InlineData("SJ")]
    public void SubunitCode_accepts_exactly_2_chars(string value)
        => ((Action)(() => SubunitCode.From(value))).Should().NotThrow();

    [Theory]
    [InlineData("1")]
    [InlineData("111")]
    public void SubunitCode_rejects_invalid_length(string value)
        => ((Action)(() => SubunitCode.From(value))).Should().Throw<ValueObjectValidationException>();
}

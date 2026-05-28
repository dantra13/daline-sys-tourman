using FluentAssertions;
using Sport.Core.Structure;
using Vogen;

namespace Sport.Core.Tests.Structure;

public class RscValidationTests
{
    private const string ValidUnitRsc  = "FBLMTEAM11------------QFNL000100--";
    private const string ValidPhaseRsc = "FBLMTEAM11------------GPA---------";

    [Theory]
    [InlineData(ValidUnitRsc)]
    [InlineData(ValidPhaseRsc)]
    public void Accepts_valid_34_char_uppercase_input(string value)
    {
        var act = () => Rsc.From(value);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("FBL")]
    [InlineData("FBLMTEAM11------------QFNL000100---")]
    [InlineData("fblmteam11------------qfnl000100--")]
    [InlineData("FBL_TEAM11------------QFNL000100--")]
    public void Rejects_invalid_input(string value)
    {
        var act = () => Rsc.From(value);
        act.Should().Throw<ValueObjectValidationException>();
    }
}

using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Vogen;

namespace Sport.Core.Tests.DisciplineRegistry;

public class DisciplineCodeTests
{
    [Theory]
    [InlineData("FBL")]
    [InlineData("BKB")]
    [InlineData("ATH")]
    public void Accepts_three_uppercase_letters(string value)
    {
        var act = () => DisciplineCode.From(value);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("FB")]
    [InlineData("FBLX")]
    [InlineData("fbl")]
    [InlineData("F1L")]
    public void Rejects_invalid_inputs(string value)
    {
        var act = () => DisciplineCode.From(value);
        act.Should().Throw<ValueObjectValidationException>();
    }
}

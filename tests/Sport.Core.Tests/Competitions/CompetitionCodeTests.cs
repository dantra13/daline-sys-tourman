using FluentAssertions;
using Sport.Core.Competitions;
using Vogen;

namespace Sport.Core.Tests.Competitions;

public class CompetitionCodeTests
{
    [Theory]
    [InlineData("copa-2026")]
    [InlineData("torneo-clausura")]
    [InlineData("c1")]
    public void Accepts_lowercase_kebab_slugs(string value)
        => ((Action)(() => CompetitionCode.From(value))).Should().NotThrow();

    [Theory]
    [InlineData("")]
    [InlineData("Copa-2026")]
    [InlineData("copa 2026")]
    [InlineData("copa_2026")]
    public void Rejects_invalid_input(string value)
        => ((Action)(() => CompetitionCode.From(value))).Should().Throw<ValueObjectValidationException>();
}

using FluentAssertions;
using Sport.Core.Shared;

namespace Sport.Core.Tests.Shared;

public class ResultTests
{
    [Fact]
    public void Ok_has_no_error_and_no_code()
    {
        var r = Result.Ok();
        r.IsSuccess.Should().BeTrue();
        r.Error.Should().BeNull();
        r.Code.Should().BeNull();
    }

    [Fact]
    public void Fail_with_message_only_leaves_code_null()
    {
        var r = Result.Fail("boom");
        r.IsSuccess.Should().BeFalse();
        r.Error.Should().Be("boom");
        r.Code.Should().BeNull();
    }

    [Fact]
    public void Fail_with_code_sets_both_code_and_message()
    {
        var r = Result.Fail("I-RES-5", "boom");
        r.IsSuccess.Should().BeFalse();
        r.Code.Should().Be("I-RES-5");
        r.Error.Should().Be("boom");
    }

    [Fact]
    public void Fail_with_blank_code_throws()
    {
        var act = () => Result.Fail("  ", "boom");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Fail_with_blank_message_throws()
    {
        var act = () => Result.Fail("I-RES-5", "  ");
        act.Should().Throw<ArgumentException>();
    }
}

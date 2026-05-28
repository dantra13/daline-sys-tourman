using FluentAssertions;
using Sport.Core.Shared;

namespace Sport.Core.Tests.Shared;

public class ResultTests
{
    [Fact]
    public void Ok_creates_a_success_result_with_no_error()
    {
        var result = Result.Ok();

        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Fail_creates_a_failure_result_with_error_message()
    {
        var result = Result.Fail("bad input");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("bad input");
    }

    [Fact]
    public void Fail_with_empty_message_throws()
    {
        var act = () => Result.Fail("");
        act.Should().Throw<System.ArgumentException>();
    }
}

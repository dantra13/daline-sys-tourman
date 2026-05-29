using FluentAssertions;
using Sport.Core.Shared;

namespace Sport.Core.Tests.Shared;

public class DomainExceptionTests
{
    [Fact]
    public void Ctor_with_code_message_params_exposes_all_three()
    {
        var ex = new DomainException(
            code: "I-COMP-3",
            message: "Duplicate discipline.",
            @params: new Dictionary<string, object?> { ["discipline"] = "FBL" });

        ex.Code.Should().Be("I-COMP-3");
        ex.Message.Should().Be("Duplicate discipline.");
        ex.Params.Should().ContainKey("discipline").WhoseValue.Should().Be("FBL");
    }

    [Fact]
    public void Ctor_with_null_params_defaults_to_empty_dictionary()
    {
        var ex = new DomainException("I-X-1", "msg");

        ex.Params.Should().BeEmpty();
    }
}

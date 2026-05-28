using Vogen;

namespace Sport.Core.Officials;

[ValueObject<string>]
public readonly partial struct FunctionCode
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return Validation.Invalid("FunctionCode is required.");
        if (value.Length > 20) return Validation.Invalid("FunctionCode must be at most 20 characters.");
        return Validation.Ok;
    }
}

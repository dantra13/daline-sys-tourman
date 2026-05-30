using Vogen;

namespace Sport.Core.Results;

[ValueObject<string>]
public readonly partial struct ExtensionCode
{
    public const int MaxLength = 24;

    private static Validation Validate(string value)
    {
        if (string.IsNullOrEmpty(value)) return Validation.Invalid("ExtensionCode is required.");
        if (value.Length > MaxLength) return Validation.Invalid($"ExtensionCode must be at most {MaxLength} characters.");
        foreach (var c in value)
        {
            var ok = c is >= 'A' and <= 'Z' || c is >= '0' and <= '9' || c == '_';
            if (!ok) return Validation.Invalid("ExtensionCode chars must be uppercase alphanumeric or underscore.");
        }
        return Validation.Ok;
    }
}

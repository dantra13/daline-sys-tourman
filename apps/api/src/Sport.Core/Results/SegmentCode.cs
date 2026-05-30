using Vogen;

namespace Sport.Core.Results;

[ValueObject<string>]
public readonly partial struct SegmentCode
{
    public const int MaxLength = 8;

    private static Validation Validate(string value)
    {
        if (string.IsNullOrEmpty(value)) return Validation.Invalid("SegmentCode is required.");
        if (value.Length > MaxLength) return Validation.Invalid($"SegmentCode must be at most {MaxLength} characters.");
        foreach (var c in value)
        {
            var ok = c is >= 'A' and <= 'Z' || c is >= '0' and <= '9' || c == '-' || c == '_';
            if (!ok) return Validation.Invalid("SegmentCode chars must be uppercase alphanumeric, hyphen, or underscore.");
        }
        return Validation.Ok;
    }
}

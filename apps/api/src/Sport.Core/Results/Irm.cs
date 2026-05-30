using Vogen;

namespace Sport.Core.Results;

[ValueObject<string>]
public readonly partial struct Irm
{
    public const int MaxLength = 6;

    private static Validation Validate(string value)
    {
        if (string.IsNullOrEmpty(value)) return Validation.Invalid("Irm is required.");
        if (value.Length > MaxLength) return Validation.Invalid($"Irm must be at most {MaxLength} characters.");
        foreach (var c in value)
        {
            var ok = c is >= 'A' and <= 'Z' || c is >= '0' and <= '9';
            if (!ok) return Validation.Invalid("Irm chars must be uppercase alphanumeric.");
        }
        return Validation.Ok;
    }
}

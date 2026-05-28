using Vogen;

namespace Sport.Core.Structure;

[ValueObject<string>]
public readonly partial struct PhaseCode
{
    public const int MaxLength = 4;

    private static Validation Validate(string value)
    {
        if (string.IsNullOrEmpty(value)) return Validation.Invalid("PhaseCode is required.");
        if (value.Length is < 1 or > MaxLength)
            return Validation.Invalid($"PhaseCode must be 1..{MaxLength} characters.");
        foreach (var c in value)
            if (!(c is >= 'A' and <= 'Z' || c is >= '0' and <= '9'))
                return Validation.Invalid("PhaseCode must be uppercase alphanumeric.");
        return Validation.Ok;
    }
}

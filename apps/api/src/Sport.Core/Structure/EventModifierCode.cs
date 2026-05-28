using Vogen;

namespace Sport.Core.Structure;

[ValueObject<string>]
public readonly partial struct EventModifierCode
{
    public const int MaxLength = 10;

    private static Validation Validate(string value)
    {
        if (value is null) return Validation.Invalid("EventModifierCode is required.");
        if (value.Length > MaxLength)
            return Validation.Invalid($"EventModifierCode must be at most {MaxLength} characters.");
        foreach (var c in value)
            if (!(c is >= 'A' and <= 'Z' || c is >= '0' and <= '9'))
                return Validation.Invalid("EventModifierCode must be uppercase alphanumeric.");
        return Validation.Ok;
    }
}

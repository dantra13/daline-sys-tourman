using Vogen;

namespace Sport.Core.Structure;

[ValueObject<string>]
public readonly partial struct UnitCode
{
    public const int Length = 8;

    private static Validation Validate(string value)
    {
        if (value is null) return Validation.Invalid("UnitCode is required.");
        if (value.Length != Length)
            return Validation.Invalid($"UnitCode must be exactly {Length} characters.");
        foreach (var c in value)
        {
            var ok = c is >= 'A' and <= 'Z' || c is >= '0' and <= '9' || c == '-';
            if (!ok) return Validation.Invalid("UnitCode chars must be uppercase alphanumeric or '-'.");
        }
        return Validation.Ok;
    }
}

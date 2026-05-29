using Vogen;

namespace Sport.Core.Structure;

[ValueObject<string>]
public readonly partial struct SubunitCode
{
    public const int Length = 2;

    private static Validation Validate(string value)
    {
        if (value is null) return Validation.Invalid("SubunitCode is required.");
        if (value.Length != Length)
            return Validation.Invalid($"SubunitCode must be exactly {Length} characters.");
        foreach (var c in value)
            if (!(c is >= 'A' and <= 'Z' || c is >= '0' and <= '9'))
                return Validation.Invalid("SubunitCode chars must be uppercase alphanumeric.");
        if (value == "00")
            return Validation.Invalid("SubunitCode '00' is reserved for the parent-unit marker.");
        return Validation.Ok;
    }
}

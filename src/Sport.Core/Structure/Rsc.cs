using Vogen;

namespace Sport.Core.Structure;

[ValueObject<string>]
public readonly partial struct Rsc
{
    public const int Length = 34;
    public const char Filler = '-';

    private static Validation Validate(string value)
    {
        if (value is null) return Validation.Invalid("RSC is required.");
        if (value.Length != Length) return Validation.Invalid($"RSC must be exactly {Length} characters.");
        foreach (var c in value)
        {
            var ok = c is >= 'A' and <= 'Z'
                  || c is >= '0' and <= '9'
                  || c == '.'
                  || c == '-';
            if (!ok) return Validation.Invalid("RSC may only contain A-Z, 0-9, '.' and '-'.");
        }
        return Validation.Ok;
    }
}

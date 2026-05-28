using Vogen;

namespace Sport.Core.DisciplineRegistry;

[ValueObject<string>]
public readonly partial struct DisciplineCode
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrEmpty(value)) return Validation.Invalid("DisciplineCode is required.");
        if (value.Length != 3) return Validation.Invalid("DisciplineCode must be exactly 3 characters.");
        foreach (var c in value)
            if (c is < 'A' or > 'Z')
                return Validation.Invalid("DisciplineCode must be 3 uppercase ASCII letters.");
        return Validation.Ok;
    }
}

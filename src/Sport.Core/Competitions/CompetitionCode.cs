using Vogen;

namespace Sport.Core.Competitions;

[ValueObject<string>]
public readonly partial struct CompetitionCode
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrEmpty(value)) return Validation.Invalid("CompetitionCode is required.");
        if (value.Length is < 1 or > 64)
            return Validation.Invalid("CompetitionCode must be 1..64 characters.");
        foreach (var c in value)
        {
            var ok = c is >= 'a' and <= 'z' || c is >= '0' and <= '9' || c == '-';
            if (!ok) return Validation.Invalid("CompetitionCode must be lowercase kebab-case (a-z, 0-9, '-').");
        }
        if (value[0] == '-' || value[^1] == '-')
            return Validation.Invalid("CompetitionCode cannot start or end with '-'.");
        return Validation.Ok;
    }
}

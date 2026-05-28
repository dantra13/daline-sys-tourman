using Vogen;

namespace Sport.Core.Participants;

[ValueObject<string>]
public readonly partial struct OrganisationCode
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrEmpty(value)) return Validation.Invalid("OrganisationCode is required.");
        if (value.Length is < 3 or > 10) return Validation.Invalid("OrganisationCode must be 3..10 characters.");
        foreach (var c in value)
            if (!(c is >= 'A' and <= 'Z' || c is >= '0' and <= '9' || c == '-'))
                return Validation.Invalid("OrganisationCode chars must be uppercase A-Z, 0-9 or '-'.");
        return Validation.Ok;
    }
}

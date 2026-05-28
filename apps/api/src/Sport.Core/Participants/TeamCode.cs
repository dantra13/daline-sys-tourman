using Vogen;

namespace Sport.Core.Participants;

[ValueObject<string>]
public readonly partial struct TeamCode
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrEmpty(value)) return Validation.Invalid("TeamCode is required.");
        if (value.Length > 20) return Validation.Invalid("TeamCode must be at most 20 characters.");
        return Validation.Ok;
    }
}

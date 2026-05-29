namespace Sport.Application.Common;

public sealed record ValidationFailure(
    string Code,
    string? Target = null,
    IReadOnlyDictionary<string, object?>? Params = null);

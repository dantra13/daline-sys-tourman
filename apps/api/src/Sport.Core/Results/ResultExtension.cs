namespace Sport.Core.Results;

public sealed record ResultExtension(ExtensionType Type, ExtensionCode Code)
{
    public string? Pos { get; init; }
    public string? Value { get; init; }
    public string? Value2 { get; init; }
    public int? Rank { get; init; }
    public bool RankEqual { get; init; }
    public int? SortOrder { get; init; }
    public string? Diff { get; init; }
    public string? Move { get; init; }
    public string? Attempt { get; init; }
    public IReadOnlyList<ResultExtension> Children { get; init; } = Array.Empty<ResultExtension>();
}

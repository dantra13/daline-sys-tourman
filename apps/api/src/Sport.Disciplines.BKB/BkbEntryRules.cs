using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Shared;

namespace Sport.Disciplines.BKB;

internal sealed class BkbEntryRules : IEntryRules
{
    public IReadOnlyCollection<EntryType> AllowedTypes { get; } = new[] { EntryType.Team };

    public (int Min, int Max) CompositionSize(EntryType type) => type switch
    {
        EntryType.Team => (5, 12),
        _ => (0, 0),
    };

    public Result Validate(EntryCandidate candidate)
    {
        if (!AllowedTypes.Contains(candidate.Type))
            return Result.Fail($"BKB only accepts Team entries, got '{candidate.Type}'.");
        var (min, max) = CompositionSize(candidate.Type);
        if (candidate.CompositionSize < min || candidate.CompositionSize > max)
            return Result.Fail($"BKB Team composition must be {min}..{max}, got {candidate.CompositionSize}.");
        return Result.Ok();
    }
}

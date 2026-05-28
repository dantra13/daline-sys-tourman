using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Shared;

namespace Sport.Disciplines.BOX;

internal sealed class BoxEntryRules : IEntryRules
{
    public IReadOnlyCollection<EntryType> AllowedTypes { get; } = new[] { EntryType.Athlete };

    public (int Min, int Max) CompositionSize(EntryType type) => type switch
    {
        EntryType.Athlete => (1, 1),
        _ => (0, 0),
    };

    public Result Validate(EntryCandidate candidate)
    {
        if (!AllowedTypes.Contains(candidate.Type))
            return Result.Fail($"BOX only accepts Athlete entries, got '{candidate.Type}'.");
        var (min, max) = CompositionSize(candidate.Type);
        if (candidate.CompositionSize < min || candidate.CompositionSize > max)
            return Result.Fail($"BOX Athlete composition must be {min}..{max}, got {candidate.CompositionSize}.");
        return Result.Ok();
    }
}

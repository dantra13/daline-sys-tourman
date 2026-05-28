using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Shared;

namespace Sport.Disciplines.BDM;

internal sealed class BdmEntryRules : IEntryRules
{
    public IReadOnlyCollection<EntryType> AllowedTypes { get; } = new[] { EntryType.Athlete, EntryType.Team };

    public (int Min, int Max) CompositionSize(EntryType type) => type switch
    {
        EntryType.Athlete => (1, 1),
        EntryType.Team => (2, 2),
        _ => (0, 0),
    };

    public Result Validate(EntryCandidate candidate)
    {
        if (!AllowedTypes.Contains(candidate.Type))
            return Result.Fail($"BDM only accepts Athlete or Team entries, got '{candidate.Type}'.");
        var (min, max) = CompositionSize(candidate.Type);
        if (candidate.CompositionSize < min || candidate.CompositionSize > max)
            return Result.Fail($"BDM {candidate.Type} composition must be {min}..{max}, got {candidate.CompositionSize}.");
        return Result.Ok();
    }
}
